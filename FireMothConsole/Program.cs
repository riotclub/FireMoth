// <copyright file="Program.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console;

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RiotClub.FireMoth.Console.Extensions;
using RiotClub.FireMoth.Console.Tasks;
using RiotClub.FireMoth.Services.DataAccess.Sqlite;
using RiotClub.FireMoth.Services.Orchestration;
using RiotClub.FireMoth.Services.Tasks.Output;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Services.Tasks;
using Serilog;

/// <summary>Application entry point.</summary>
public static class Program
{
    private const int BootstrapLogRetainedFileCountLimit = 2;
    private const uint BootstrapLogFileSizeLimit = 1024 * 1024 * 32; // 32 MB

    internal static DateTime ProgramStartDateTime;
    
    private static readonly FileSystem FileSystem = new();

    /// <summary>Class and application entry point. Validates command-line arguments, performs
    /// startup configuration, and invokes the directory scanning process.</summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>An <c>int</c> return code indicating invocation result.</returns>
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console() 
            .WriteTo.File(
                "./bootstrap.log",
                fileSizeLimitBytes: BootstrapLogFileSizeLimit,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: BootstrapLogRetainedFileCountLimit)
            .CreateBootstrapLogger();
        ProgramStartDateTime = DateTime.Now;
        var parser = BuildCommandLineParser(args);
        return parser.InvokeAsync(args).Result;
    }

    private static Parser BuildCommandLineParser(string[] args)
    {
        // TODO: Per documentation, System.CommandLine should accept Option<DirectoryInfo> here, but
        // when attempting to bind the argument with that type, it always ends up null. Figure out
        // why and correct.
        var scanDirectoryOption = new Option<string>(
            aliases: ["--directory", "-d"],
            description: "The directory to scan")
        {
            IsRequired = true
        };
        scanDirectoryOption.AddValidator(result =>
        {
            var scanDirectory = result.GetValueForOption(scanDirectoryOption);
            if (!string.IsNullOrWhiteSpace(scanDirectory) &&
                FileSystem.Directory.Exists(scanDirectory))
                return;
            
            Log.Fatal("Scan directory '{ScanDirectory}' does not exist.", scanDirectory);
            result.ErrorMessage = $"Scan directory '{scanDirectory}' does not exist.";
        });

        var recursiveScanOption = new Option<bool>(
            aliases: ["--recursive", "-r"],
            description: "Recursively scan subdirectories",
            getDefaultValue: () => false);

        var outputFileOption = new Option<string?>(
            aliases: ["--output-file", "-o"],
            description: "File to write output to");
        outputFileOption.AddValidator(result =>
        {
            var isOutputFileValid =
                ValidateOutputFileOption(
                    result.GetValueForOption(outputFileOption), out var errorText);
            if (isOutputFileValid)
                return;
            
            Log.Fatal(errorText + ": '{OutputFilePath}'", errorText);
            result.ErrorMessage = errorText;
        });
        
        var outputDuplicateInfoOnlyOption = new Option<bool?>(
            aliases: ["--output-duplicate-info-only", "-u"],
            description: "Only include files with duplicate hash values in output",
            getDefaultValue: () => false);

        var duplicateFileHandlingMethodOption = new Option<DuplicateFileHandlingMethod?>(
            aliases: ["--duplicate-file-handling-method", "-m"],
            description: "Duplicate file handling method",
            getDefaultValue: () => DuplicateFileHandlingMethod.NoAction);
        
        var moveDuplicateFilesToDirectoryOption = new Option<string?>(
            aliases: ["--move-duplicate-files-to-directory", "-M"],
            description: "Directory to move duplicate files to; ignored if " +
                         "--duplicate-file-handling-method is not Move",
            getDefaultValue: () =>
                CommandLineConfigurationProvider.ScanDirectoryToken
                + Path.DirectorySeparatorChar + "Duplicates");

        var interactiveOption = new Option<bool?>(
            aliases: ["--interactive", "-i"],
            description: "Use interactive duplicate file handling (requires -m option)");
        interactiveOption.AddValidator(result =>
        {
            var duplicateOptionValue = result.GetValueForOption(duplicateFileHandlingMethodOption);
            if (duplicateOptionValue is not null &&
                duplicateOptionValue != DuplicateFileHandlingMethod.NoAction)
                return;

            const string errorText = "Interactive (-i) option requires a duplicate file handling " +
                                     "method option (-m) of \"move\" or \"delete\""; 
            Log.Fatal(errorText);
            result.ErrorMessage = errorText;
        });
        
        var rootCommand =
            new RootCommand(description: "FireMoth file analysis and deduplication program.");
        rootCommand.AddOption(scanDirectoryOption);
        rootCommand.AddOption(recursiveScanOption);
        rootCommand.AddOption(outputFileOption);
        rootCommand.AddOption(outputDuplicateInfoOnlyOption);
        rootCommand.AddOption(duplicateFileHandlingMethodOption);
        rootCommand.AddOption(moveDuplicateFilesToDirectoryOption);
        rootCommand.AddOption(interactiveOption);
        rootCommand.Handler = CommandHandler.Create<
                IHost,
                ParseResult,
                string,
                bool,
                string,
                bool,
                DuplicateFileHandlingMethod,
                string,
                bool>(
            async (
                host,
                parseResult,
                // These "unused" arguments to the async delegate need to be here because the
                // System.CommandLine library uses introspection to map these values when parsing
                // the command line.
                scanDirectory,
                recursiveScan,
                outputFile,
                outputDuplicateInfoOnly,
                duplicateFileHandlingMethod,
                moveDuplicateFilesToDirectory,
                interactive) =>
            {
                Log.Debug("Command line parse result: {ParsedCommandLine}", parseResult);
                await RunAsync(host);
            });
        
        var builder = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseHost(host =>
            {
                host.ConfigureDefaults(args)
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        builder.AddCommandLineConfiguration(
                            context.GetInvocationContext().ParseResult);
                    })
                    .UseConsoleLifetime()
                    .UseSerilog((context, services, configuration) =>
                    {
                        configuration.ReadFrom.Configuration(context.Configuration)
                                     .ReadFrom.Services(services);

                        var seqHost = context.Configuration["SeqHost"];
                        if (seqHost is not null)
                            configuration.WriteTo.Seq(seqHost);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.Configure<DirectoryScanOptions>(
                            hostContext.Configuration.GetSection("CommandLine"));
                        services.Configure<DuplicateFileHandlingOptions>(
                            hostContext.Configuration.GetSection("CommandLine"));
                        services.Configure<ScanOutputOptions>(
                            hostContext.Configuration.GetSection("CommandLine"));
                        services.Configure<InteractiveDuplicateHandlerOptions>(
                            hostContext.Configuration.GetSection(
                                "InteractiveDuplicateHandler:OpenFiles"));
                        services.AddFireMothServices(hostContext.Configuration);
                    });
            });
        
        return builder.Build();
    }
    
    private static async Task RunAsync(IHost host)
    {
        try
        {
            Log.Information("FireMoth.Console starting up.");
            ScanResult scanResult;
            var stopwatch = new Stopwatch();

            using (var scope = host.Services.CreateScope())
            {
                stopwatch.Start();
                await InitializeDatabaseAsync(scope);
                var scanner =
                    scope.ServiceProvider.GetRequiredService<IDirectoryScanOrchestrator>();
                scanResult = await scanner.ScanDirectoryAsync();
                await HandlePostScanTasksAsync(scope);
                stopwatch.Stop();
            }
            
            var timeSpan = stopwatch.Elapsed;
            Log.Information("Total scan time: {ScanTime}.", timeSpan);

            LogScanResult(scanResult);
        }
        catch (Exception exception)
        {
            Log.Fatal(
                exception,
                "FireMoth.Console encountered an unhandled exception: {ExceptionMessage}",
                exception.Message);
        }
        finally
        {
            Log.Information("FireMoth.Console shutting down.");
            Log.CloseAndFlush();
        }
    }
    
    private static async Task HandlePostScanTasksAsync(IServiceScope scope)
    {
        var taskHandlers = scope.ServiceProvider.GetServices<ITaskHandler>();
        foreach (var taskHandler in taskHandlers)
        {
            Log.Debug(
                "Running post-scan task handler of type '{TaskHandlerType}'.",
                taskHandler.GetType());
            await taskHandler.RunTaskAsync();

            var disposableHandler = taskHandler as IDisposable;
            disposableHandler?.Dispose();
        }
    }
    
    private static bool ValidateOutputFileOption(string? outputOptionValue, out string errorText)
    {
        errorText = string.Empty;
        
        if (string.IsNullOrWhiteSpace(outputOptionValue))
            return false;

        var outputFilePath = FileSystem.Path.GetFullPath(outputOptionValue);
        if (FileSystem.Directory.Exists(outputFilePath))
        {
            errorText = "Specified output path is an existing directory";
        }
        else if (FileSystem.File.Exists(outputFilePath))
        {
            errorText = "Output file already exists";
        }
        else
        {
            try
            {
                var outputFileName = FileSystem.Path.GetFileName(outputFilePath);
                var outputFileDirectory = FileSystem.Path.GetDirectoryName(outputFilePath);
                if (outputFileName.IndexOfAny(FileSystem.Path.GetInvalidFileNameChars()) >= 0
                    || (outputFileDirectory is not null 
                        && outputFileDirectory.IndexOfAny(FileSystem.Path.GetInvalidPathChars()) >= 0))
                {
                    errorText = "Invalid output file";
                }
            }
            catch (ArgumentException e)
            {
                errorText = $"Invalid output file: {e.Message}";
            }
        }

        return string.IsNullOrEmpty(errorText);
    }
    
    private static async Task InitializeDatabaseAsync(IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FireMothContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var fingerprintRepository =
            scope.ServiceProvider.GetRequiredService<IFileFingerprintRepository>();
        var result = await fingerprintRepository.DeleteAllAsync();
        Log.Debug("Deleted {ExistingEntryCount} existing entries from SQLite database.", result);
    }
    
    private static void LogScanResult(ScanResult scanResult)
    {
        Log.Information(
            "Scan complete. Scanned {ScannedFilesCount} file(s).", scanResult.ScannedFiles.Count);
        if (scanResult.SkippedFiles.Count == 0)
            return;

        Log.Information(
            "{SkippedFileCount} file(s) could not be scanned:", scanResult.SkippedFiles.Count);
        foreach (var file in scanResult.SkippedFiles)
            Log.Information("'{SkippedFile}'; reason: {SkipReason}", file.Key, file.Value);
    }
}