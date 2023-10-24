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
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

    private static string _outputFileName;

    private static Parser BuildCommandLineParser(string[] args) =>
        new CommandLineBuilder(
            new RootCommand
            {
                Handler = CommandHandler.Create<IHost>(RunAsync)
            })
            .UseDefaults()
            .UseHost(host =>
            {
                host.ConfigureDefaults(args)
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
                        services.Configure<CommandLineOptions>(hostContext.Configuration);
                        services.AddFireMothServices(hostContext.Configuration);
                        var commandLineOptions =
                            hostContext.Configuration.Get<CommandLineOptions>();
                        _outputFileName = GetOutputFileName(commandLineOptions.OutputFile);
                        services.AddTransient(_ => new StreamWriter(_outputFileName));
                    });
            })
            .Build();
    
    /// <summary>
    /// Class and application entry point. Validates command-line arguments, performs startup
    /// configuration, and invokes the directory scanning process.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>An <c>int</c> return code indicating invocation result.</returns>
    /// <seealso cref="CommandLineOptions"/>
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

        var parser = BuildCommandLineParser(args);
        return parser.InvokeAsync(args).Result;
    }

    private static async Task RunAsync(IHost host)
    {
        try
        {
            Log.Information("FireMoth.Console starting up...");
            
            await host.StartAsync();

            ScanResult scanResult;
            var stopwatch = new Stopwatch();

            using (var scope = host.Services.CreateScope())
            {
                stopwatch.Start();
                await InitializeDatabaseAsync(scope);
                var commandLineOptions = scope
                    .ServiceProvider.GetRequiredService<IOptions<CommandLineOptions>>().Value;
                scanResult = await ScanDirectoryAsync(scope, commandLineOptions);
                await OutputScanResult(scope, scanResult, commandLineOptions);
                await HandleFileOpsAsync(scope, commandLineOptions);
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
                "FireMoth.Console could not complete: {ExceptionMessage}.",
                exception.Message);
        }
        finally
        {
            Log.Information("Shutting down");
            Log.CloseAndFlush();
        }          
    }

    private static async Task OutputScanResult(
        IServiceScope scope, ScanResult result, CommandLineOptions options)
    {
        IEnumerable<FileFingerprint> fingerprintsToOutput;
        if (!options.OutputDuplicatesOnly)
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

    private static async Task HandleFileOpsAsync(IServiceScope scope, CommandLineOptions options)
    {
        var taskHandlers = scope.ServiceProvider.GetServices<ITaskHandler>();
        foreach (var taskHandler in taskHandlers)
        {
            Log.Debug("Running task handler of type '{TaskHandlerType}'.", taskHandler.GetType());
            await taskHandler.RunTaskAsync();
        }
    }
    
    private static async Task<ScanResult> ScanDirectoryAsync(
        IServiceScope scope, CommandLineOptions options)
    {
        var scanDirectoryInfo = new FileSystem().DirectoryInfo.FromDirectoryName(
            options.ScanDirectory);
        var scanOptions = new ScanOptions(scanDirectoryInfo, options.RecursiveScan);
        var scanner = scope.ServiceProvider.GetRequiredService<IDirectoryScanOrchestrator>();
        return await scanner.ScanDirectoryAsync(
            scanOptions.ScanDirectory.FullName, options.RecursiveScan);
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

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
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
                services.Configure<CommandLineOptions>(hostContext.Configuration);
                services.AddFireMothServices(hostContext.Configuration);
                var commandLineOptions = hostContext.Configuration.Get<CommandLineOptions>();
                _outputFileName = GetOutputFileName(commandLineOptions.OutputFile);
                services.AddTransient(_ => new StreamWriter(_outputFileName));
            });

    private static string GetOutputFileName(string outputFile)
    {
        if (string.IsNullOrWhiteSpace(outputFile))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                   + Path.DirectorySeparatorChar + DefaultFilePrefix
                   + DateTime.Now.ToString(DefaultFileDateTimeFormat, CultureInfo.InvariantCulture)
                   + '.' + DefaultFileExtension;
        }

        var outputFileFullPath = Path.GetFullPath(outputFile);
        
        if (!File.Exists(outputFileFullPath))
            return Path.GetFullPath(outputFileFullPath);

        var fileExistsError = $"Output file '{outputFileFullPath}' already exists";
        Log.Fatal(fileExistsError);
        throw new IOException(fileExistsError);
    }
}