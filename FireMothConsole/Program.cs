// <copyright file="Program.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RiotClub.FireMoth.Console.Extensions;
using RiotClub.FireMoth.Services.DataAccess.Sqlite;
using RiotClub.FireMoth.Services.FileScanning;
using RiotClub.FireMoth.Services.Orchestration;
using RiotClub.FireMoth.Services.Output;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Services.Tasks;
using Serilog;

/// <summary>
/// Application entry point.
/// </summary>
public static class Program
{
    private const int BootstrapLogRetainedFileCountLimit = 2;
    private const uint BootstrapLogFileSizeLimit = 1024 * 1024 * 32; // 32 MB
    private const string DefaultFilePrefix = "FireMoth_";
    private const string DefaultFileExtension = "csv";
    private const string DefaultFileDateTimeFormat = "yyyyMMdd-HHmmss";

    private static string? _outputFileName;
    private static bool? _outputDuplicatesOnly;
    private static DateTime _programStartDateTime;

    /// <summary>
    /// Class and application entry point. Validates command-line arguments, performs startup
    /// configuration, and invokes the directory scanning process.
    /// </summary>
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
        _programStartDateTime = DateTime.Now;
        var parser = BuildCommandLineParser(args);
        return parser.InvokeAsync(args).Result;
    }    
    
    private static Parser BuildCommandLineParser(string[] args)
    {
        // TODO: System.CommandLine should accept Option<DirectoryInfo> here, but when attempting to
        //       bind the argument with that type, it always ends up null. Figure out why.
        var scanDirectoryOption = new Option<string>(
            aliases: ["--directory", "-d"],
            description: "The directory to scan")
        {
            IsRequired = true
        };
        scanDirectoryOption.AddValidator(result =>
        {
            var scanDirectory = result.GetValueForOption(scanDirectoryOption);
            if (string.IsNullOrWhiteSpace(scanDirectory) || !Directory.Exists(scanDirectory))
            {
                Log.Fatal("Scan directory '{ScanDirectory}' does not exist.", scanDirectory);
                result.ErrorMessage = $"Scan directory '{scanDirectory}' does not exist.";
            }
        });

        var recursiveScanOption = new Option<bool>(
            aliases: ["--recursive", "-r"],
            description: "Recursively scan subdirectories",
            getDefaultValue: () => false);

        var outputFileOption = new Option<FileInfo?>(
            aliases: ["--output", "-o"],
            description: "File to write output to");
        
        var outputDuplicatesOnlyOption = new Option<bool?>(
            aliases: ["--output-duplicates-only", "-u"],
            description: "Only include files with duplicate hash values in output",
            getDefaultValue: () => false);

        var duplicateFileHandlingMethodOption = new Option<DuplicateFileHandlingMethod?>(
            aliases: ["--duplicate-file-handling-method", "-m"],
            description: "Duplicate file handling method",
            getDefaultValue: () => DuplicateFileHandlingMethod.NoAction);
        
        var moveDuplicateFilesToDirectoryOption = new Option<string?>(
            aliases: ["--move-duplicate-files-to-directory", "-M"],
            description: "Directory to move duplicate files to; ignored if " +
                         "--duplicate-file-handling-method is not Move.",
            getDefaultValue: () =>
                CommandLineConfigurationProvider.ScanDirectoryToken
                + Path.DirectorySeparatorChar + "Duplicates");

        var rootCommand =
            new RootCommand(description: "FireMoth file analysis and deduplication program.");
        rootCommand.AddOption(scanDirectoryOption);
        rootCommand.AddOption(recursiveScanOption);
        rootCommand.AddOption(outputFileOption);
        rootCommand.AddOption(outputDuplicatesOnlyOption);
        rootCommand.AddOption(duplicateFileHandlingMethodOption);
        rootCommand.AddOption(moveDuplicateFilesToDirectoryOption);
        rootCommand.Handler = CommandHandler.Create<
                IHost,
                ParseResult,
                string,
                bool,
                string,
                bool,
                DuplicateFileHandlingMethod,
                string>(
            async (
                host,
                parseResult,
                scanDirectory,
                recursiveScan,
                outputFile,
                outputDuplicatesOnly,
                duplicateFileHandlingMethod,
                moveDuplicateFilesToDirectory) =>
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
                        configuration
                            .ReadFrom.Configuration(context.Configuration)
                            .ReadFrom.Services(services)
                            .WriteTo.Console();

                        var seqHost = context.Configuration["SeqHost"];
                        if (seqHost is not null)
                        {
                            configuration.WriteTo.Seq(seqHost);
                        }
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        // Can we just grab these straight from the options after binding? 
                        _outputFileName = GetOutputFileName(
                            hostContext.Configuration.GetValue<string>("CommandLine:output"));
                        _outputDuplicatesOnly = hostContext.Configuration.GetValue<bool>(
                            "CommandLine:output-duplicates-only");
                        services.Configure<DirectoryScanOptions>(
                            hostContext.Configuration.GetSection("CommandLine"));
                        services.Configure<DuplicateFileHandlingOptions>(
                            hostContext.Configuration.GetSection("CommandLine"));
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
                scanResult = await ScanDirectoryAsync(scope);
                await OutputScanResult(scope, scanResult);
                await HandleFileOpsAsync(scope);
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
                "FireMoth.Console could not complete: {ExceptionMessage}",
                exception.Message);
        }
        finally
        {
            Log.Information("Shutting down");
            Log.CloseAndFlush();
        }          
    }

    private static async Task OutputScanResult(IServiceScope scope, ScanResult result)
    {
        IEnumerable<FileFingerprint> fingerprintsToOutput;
        if (_outputDuplicatesOnly is null or false)
        {
            Log.Information("Writing output to '{OutputFileName}'.", _outputFileName);
            fingerprintsToOutput = result.ScannedFiles;
        }
        else
        {
            Log.Information(
                "Writing output (duplicates only) to '{OutputFileName}'.", _outputFileName);
            var fingerprintRepository =
                scope.ServiceProvider.GetRequiredService<IFileFingerprintRepository>();
            var duplicateFingerprints = 
                await fingerprintRepository.GetRecordsWithDuplicateHashesAsync();
            fingerprintsToOutput = duplicateFingerprints.ToList();
        }
        
        var resultWriter = scope.ServiceProvider.GetRequiredService<IFileFingerprintWriter>();
        await resultWriter.WriteFileFingerprintsAsync(fingerprintsToOutput);
    }

    private static async Task HandleFileOpsAsync(IServiceScope scope)
    {
        var taskHandlers = scope.ServiceProvider.GetServices<ITaskHandler>();
        foreach (var taskHandler in taskHandlers)
        {
            Log.Debug("Running task handler of type '{TaskHandlerType}'.", taskHandler.GetType());
            await taskHandler.RunTaskAsync();
        }
    }
    
    private static async Task<ScanResult> ScanDirectoryAsync(IServiceScope scope)
    {
        var scanner = scope.ServiceProvider.GetRequiredService<IDirectoryScanOrchestrator>();
        return await scanner.ScanDirectoryAsync();
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

    private static string? GetOutputFilePath(string? outputFile)
    {
        return string.IsNullOrWhiteSpace(outputFile) 
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) 
            : Path.GetDirectoryName(outputFile);
    }
    
    private static string GetOutputFileName(string? outputFile)
    {
        // If no outputFile was provided, use default path and filename.
        if (string.IsNullOrWhiteSpace(outputFile))
        {
            return GetOutputFilePath(outputFile) + Path.DirectorySeparatorChar + DefaultFilePrefix
                   + _programStartDateTime.ToString(
                       DefaultFileDateTimeFormat, CultureInfo.InvariantCulture)                   
                   + '.' + DefaultFileExtension;
        }

        var outputFilePath = Path.GetFullPath(outputFile);
        string? errorText = null;
        
        // If the outputFile is an existing directory, throw an IO error.
        if (Directory.Exists(outputFilePath))
            errorText = $"Cannot write to specified output path '{outputFilePath}'; path " +
                        $"is an existing directory.";

        // If the outputFile already exists, throw an IO error.
        if (File.Exists(outputFilePath))
            errorText = $"Output file '{outputFilePath}' already exists";

        if (errorText is null)
            return Path.GetFullPath(outputFilePath);
            
        Log.Fatal(errorText);
        throw new IOException(errorText);
    }
}