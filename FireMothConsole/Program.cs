﻿// <copyright file="Program.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using RiotClub.FireMoth.Services.DataAccess;
    using RiotClub.FireMoth.Services.DataAnalysis;
    using RiotClub.FireMoth.Services.FileScanning;
    using Serilog;

    /// <summary>
    /// Application entry point.
    /// </summary>
    public static class Program
    {
        private const string DefaultFilePrefix = "FireMothData_";
        private const string DefaultFileExtension = "csv";
        private const string DefaultFileDateTimeFormat = "yyyyMMdd-HHmmss";

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
                .WriteTo.File("./bootstrap.log")
                .CreateBootstrapLogger();

            try
            {
                Log.Information("FireMoth.Console starting up...");
                using var host = CreateHostBuilder(args).Build();
                await host.StartAsync();

                var fileScanner = host.Services.GetRequiredService<FileScanner>();
                var options = host.Services.GetRequiredService<IOptions<CommandLineOptions>>();
                var scanDirectory = new FileSystem().DirectoryInfo.FromDirectoryName(
                    options.Value.ScanDirectory);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var scanResult =
                   fileScanner.ScanDirectory(scanDirectory, options.Value.RecursiveScan);
                stopwatch.Stop();
                TimeSpan timeSpan = stopwatch.Elapsed;
                var outputWriter = host.Services.GetService<TextWriter>();

                LogScanResult(scanResult);
                Log.Information("Total scan time: {ScanTime}.", timeSpan);
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "FireMoth.Console startup failed.", exception.Message);
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
                "Scan complete. Scanned {ScannedFilesCount} file(s).",
                scanResult.ScannedFiles.Count);
            if (scanResult.SkippedFiles.Count > 0)
            {
                Log.Information(
                    "{SkippedFileCount} file(s) could not be scanned:",
                    scanResult.SkippedFiles.Count);
                foreach (var file in scanResult.SkippedFiles)
                {
                    Log.Information("'{SkippedFile}'; reason: {SkipReason}", file.Key, file.Value);
                }
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) =>
                    configuration
                        .WriteTo.Console()
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services))
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                    });
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .UseConsoleLifetime()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<CommandLineOptions>(hostContext.Configuration);
                    services.AddSingleton<FileScanner>();
                    services.AddTransient<IFileHasher, SHA256FileHasher>();
                    services.AddTransient<IDataAccessProvider, CsvDataAccessProvider>();
                    services.AddTransient(provider =>
                        new StreamWriter(
                            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                            + Path.DirectorySeparatorChar + DefaultFilePrefix
                            + DateTime.Now.ToString(DefaultFileDateTimeFormat, CultureInfo.InvariantCulture)
                            + '.' + DefaultFileExtension));
                });
    }
}