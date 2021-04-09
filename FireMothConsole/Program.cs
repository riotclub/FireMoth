// <copyright file="Program.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
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
    using FireMothServices.DataAnalysis;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using RiotClub.FireMoth.Services.DataAccess;
    using RiotClub.FireMoth.Services.FileScanning;

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
            using var host = CreateHostBuilder(args).Build();
            await host.StartAsync();

            try
            {
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
                if (outputWriter != null)
                {
                    outputWriter.WriteLine(
                        "Scan complete. Scanned {0} files.", fileScanner.TotalFilesScanned);
                    if (fileScanner.TotalFilesSkipped > 0)
                    {
                        outputWriter.WriteLine(
                            "{0} files could not be scanned due to errors.",
                            fileScanner.TotalFilesSkipped);
                    }

                    outputWriter.WriteLine($"Total scan time: {timeSpan}");
                }
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("ERROR: " + exception.Message);
            }
            finally
            {
                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                lifetime.StopApplication();
                await host.WaitForShutdownAsync();
            }
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">Arguments to supply to the host builder.</param>
        /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Information))
                .UseConsoleLifetime()
                .ConfigureHostConfiguration(configuration =>
                {
                    // Perform any configuration needed when building the host here.
                    // ConfigureDefaultBuilder sets content root to GetCurrentDirectory(); loads
                    // host config from DOTNET_* env vars and cmd line args; loads app config from
                    // appsettings.json, appsettings.{env}.json, env vars, and cmd-line args;
                    // and adds logging providers for Console, Debug, EventSource, and EventLog.
                })
                .ConfigureAppConfiguration((hostContext, configuration) =>
                {
                    // Perform any app configuration here (after the host is built).
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure services and add them to the IoC container here.
                    services.Configure<CommandLineOptions>(hostContext.Configuration);
                    services.AddSingleton(Console.Out);
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