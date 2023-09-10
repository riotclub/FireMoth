// <copyright file="Program.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
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
using Serilog;

/// <summary>
/// Application entry point.
/// </summary>
public static class Program
{
    private const int BootstrapLogRetainedFileCountLimit = 2;
    private const uint BootstrapLogFileSizeLimit = 1 << 25;     // 32 MB
    private const string DefaultFilePrefix = "FireMoth_";
    private const string DefaultFileExtension = "csv";
    private const string DefaultFileDateTimeFormat = "yyyyMMdd-HHmmss";

    private static string _outputFileName;

    /// <summary>
    /// Class and application entry point. Validates command-line arguments, performs startup
    /// configuration, and invokes the directory scanning process.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>An <c>int</c> return code indicating invocation result.</returns>
    /// <seealso cref="CommandLineOptions"/>
    public static async Task Main(string[] args)
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

        try
        {
            Log.Information("FireMoth.Console starting up...");
            using var host = CreateHostBuilder(args).Build();
            await host.StartAsync();

            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FireMothContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }
            
            ScanResult scanResult;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var scope = host.Services.CreateScope())
            {
                var scanner = scope
                    .ServiceProvider.GetRequiredService<IDirectoryScanOrchestrator>();
                var commandLineOptions = scope
                    .ServiceProvider.GetRequiredService<IOptions<CommandLineOptions>>().Value;
                var scanDirectoryInfo = new FileSystem().DirectoryInfo.FromDirectoryName(
                    commandLineOptions.ScanDirectory);
                var fingerprintRepository =
                    scope.ServiceProvider.GetRequiredService<IFileFingerprintRepository>();
                var scanOptions = new ScanOptions(
                    scanDirectoryInfo, commandLineOptions.RecursiveScan);

                var result = await fingerprintRepository.DeleteAllAsync();
                Log.Debug(
                    "Cleared {ExistingEntryCount} existing entries from SQLite database.", result);
                
                // Perform scan
                scanResult = await scanner.ScanDirectoryAsync(
                    scanOptions.ScanDirectory.FullName, commandLineOptions.RecursiveScan);
                
                // Output scan result
                IEnumerable<FileFingerprint> fingerprintsToOutput;
                if (!commandLineOptions.DuplicatesOnly)
                {
                    Log.Information("Writing output to '{OutputFileName}'.", _outputFileName);
                    fingerprintsToOutput = scanResult.ScannedFiles;
                }
                else
                {
                    Log.Information(
                        "Writing output (duplicates only) to '{OutputFileName}'.", _outputFileName);
                    var duplicateFingerprints = 
                        await fingerprintRepository.GetFileFingerprintsWithDuplicateHashesAsync();
                    fingerprintsToOutput = duplicateFingerprints.ToList();
                }
                var resultWriter = host.Services.GetRequiredService<IFileFingerprintWriter>();
                await resultWriter.WriteFileFingerprintsAsync(fingerprintsToOutput);
            }
            
            stopwatch.Stop();
            var timeSpan = stopwatch.Elapsed;

            LogScanResult(scanResult);
            Log.Information("Total scan time: {ScanTime}.", timeSpan);
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

    private static void LogScanResult(ScanResult scanResult)
    {
        Log.Information(
            "Scan complete. Scanned {ScannedFilesCount} file(s).", scanResult.ScannedFiles.Count);
        if (scanResult.SkippedFiles.Count == 0) return;

        Log.Information(
            "{SkippedFileCount} file(s) could not be scanned:", scanResult.SkippedFiles.Count);
        foreach (var file in scanResult.SkippedFiles)
        {
            Log.Information("'{SkippedFile}'; reason: {SkipReason}", file.Key, file.Value);
        }
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