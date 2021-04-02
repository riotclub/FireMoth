﻿// <copyright file="Initializer.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;
    using CommandLine.Text;
    using FireMothConsole;
    using FireMothServices.DataAnalysis;
    using Microsoft.Extensions.Hosting;
    using RiotClub.FireMoth.Services.DataAccess;
    using RiotClub.FireMoth.Services.FileScanning;

    /// <summary>
    /// Validates application startup parameters and initiates directory scan.
    /// </summary>
    public class Initializer : IHostedService
    {
        private const string DefaultFilePrefix = "FireMothData_";
        private const string DefaultFileExtension = "csv";
        private const string DefaultFileDateTimeFormat = "yyyyMMdd-HHmmss";

        private readonly string[] processArguments;
        private readonly TextWriter statusOutputWriter;
        private string dataOutputFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="Initializer"/> class.
        /// </summary>
        /// <param name="processArguments">An array of <see cref="string"/>s containing process
        /// startup (command line) arguments.</param>
        /// <param name="outputWriter">A <see cref="TextWriter"/> that initialization messages will
        /// be written to.</param>
        public Initializer(string[] processArguments, TextWriter outputWriter)
        {
            this.processArguments = processArguments;
            this.statusOutputWriter = outputWriter;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application has been initialized and is
        /// ready for startup.
        /// </summary>
        public bool Initialized { get; set; } = false;

        /// <summary>
        /// Gets the <see cref="CommandLineOptions"/> containing the command line arguments sent on
        /// application invocation.
        /// </summary>
        /// <remarks> If the application has not been initialized (via <see cref="Initialize"/>),
        /// this value will be <c>null</c>.</remarks>
        public CommandLineOptions CommandLineOptions { get; private set; }

        /// <summary>
        /// Performs initialization tasks to prepare application for startup. This must be called
        /// before the call to the <see cref="Start"/> method.
        /// </summary>
        /// <returns><c>true</c> if initialization was successful and the application is ready for
        /// startup.</returns>
        public bool Initialize()
        {
            ParserResult<CommandLineOptions> parseResult;

            using (var commandLineParser = new Parser(config => ConfigureCommandLineParser(config)))
            {
                parseResult =
                    commandLineParser.ParseArguments<CommandLineOptions>(this.processArguments);
            }

            if (parseResult.Tag == ParserResultType.Parsed)
            {
                // Command line successfully parsed.
                this.CommandLineOptions = ((Parsed<CommandLineOptions>)parseResult).Value;

                // Check for illegal characters in scan directory.
                if (ContainsInvalidPathCharacters(this.CommandLineOptions.ScanDirectory))
                {
                    this.statusOutputWriter.WriteLine(
                        "ERROR: Scan path contains invalid characters.");
                    return false;
                }

                // Update state and return success (true).
                this.Initialized = true;
                return true;
            }
            else
            {
                // Error during command line parsing. Display usage and return failure (false).
                this.DisplayHelpText(parseResult);
                return false;
            }
        }

        /// <summary>
        /// Begins scanning the directory specified during application startup.
        /// </summary>
        /// <returns>An <see cref="ExitState"/> indicating the result of the directory scan
        /// operation.</returns>
        public ExitState Start()
        {
            if (!this.Initialized)
            {
                throw new InvalidOperationException("Can't start in uninitialized state.");
            }

            ScanResult scanResult;

            this.dataOutputFile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                + Path.DirectorySeparatorChar + DefaultFilePrefix
                + DateTime.Now.ToString(DefaultFileDateTimeFormat, CultureInfo.InvariantCulture)
                + '.' + DefaultFileExtension;

            // This is closed when the encapsulating CsvDataAccessProvider is disposed.
            StreamWriter dataOutputWriter = new StreamWriter(this.dataOutputFile, false);

            using (SHA256FileHasher hasher = new SHA256FileHasher())
            using (CsvDataAccessProvider dataAccessProvider = new CsvDataAccessProvider(
                dataOutputWriter))
            {
                var fileScanner = new FileScanner(
                    dataAccessProvider, hasher, this.statusOutputWriter);
                var scanDirectory = new FileSystem().DirectoryInfo.FromDirectoryName(
                    this.CommandLineOptions.ScanDirectory);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                scanResult = fileScanner.ScanDirectory(
                    scanDirectory, this.CommandLineOptions.RecursiveScan);
                stopwatch.Stop();
                TimeSpan timeSpan = stopwatch.Elapsed;

                this.statusOutputWriter.WriteLine(
                    "Scan complete. Scanned {0} files.", fileScanner.TotalFilesScanned);
                if (fileScanner.TotalFilesSkipped > 0)
                {
                    this.statusOutputWriter.WriteLine(
                        "{0} files could not be scanned due to errors.",
                        fileScanner.TotalFilesSkipped);
                }

                this.statusOutputWriter.WriteLine($"Total scan time: {timeSpan}");
            }

            return scanResult == ScanResult.ScanSuccess ? ExitState.Normal : ExitState.RuntimeError;
        }

        /// <summary>
        /// Checks the provided string for invalid path characters.
        /// </summary>
        /// <param name="testPath">The path to test.</param>
        /// <returns><c>true</c> if the provided path contains invalid path characters.</returns>
        private static bool ContainsInvalidPathCharacters(string testPath)
        {
            var invalidPathChars = new FileSystem().Path.GetInvalidPathChars();
            Regex invalidPathCharacters = new Regex(
                "[" + Regex.Escape(new string(invalidPathChars)) + "]");
            if (invalidPathCharacters.IsMatch(testPath))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Configures an instance of <see cref="ParserSettings"/> to be consumed by the command
        /// line parser.
        /// </summary>
        /// <param name="config">A <see cref="ParserSettings"/> object used to configure the
        /// command line parser.</param>
        private static void ConfigureCommandLineParser(ParserSettings config)
        {
            config.AutoVersion = false;
            config.AutoHelp = false;
            config.HelpWriter = null;
        }

        /// <summary>
        /// Writes the application usage message to the status output.
        /// </summary>7
        private void DisplayHelpText<T>(ParserResult<T> parseResult)
        {
            var helpText = HelpText.AutoBuild(
                parseResult,
                help =>
                {
                    help.AutoHelp = false;
                    help.AutoVersion = false;
                    help.AdditionalNewLineAfterOption = false;
                    help.Heading = "FireMoth File Analyzer";
                    help.Copyright = string.Empty;
                    return HelpText.DefaultParsingErrorsHandler(parseResult, help);
                },
                e => e);

            this.statusOutputWriter.WriteLine(helpText);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
