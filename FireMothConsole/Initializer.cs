// <copyright file="Initializer.cs" company="Dark Hours Development">
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
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using CommandLine;
    using CommandLine.Text;
    using FireMothConsole;
    using RiotClub.FireMoth.Services.DataAccess;
    using RiotClub.FireMoth.Services.FileScanning;

    /// <summary>
    /// Validates application startup parameters and initiates directory scan.
    /// </summary>
    public class Initializer
    {
        private const string DefaultFilePrefix = "FireMothData_";
        private const string DefaultFileExtension = "csv";
        private const string DefaultFileDateTimeFormat = "yyyyMMdd-HHmmss";

        private readonly string[] processArguments;
        private TextWriter statusOutputWriter;
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
                this.DisplayHelpText(
                    parseResult, ((NotParsed<CommandLineOptions>)parseResult).Errors);
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
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new InvalidOperationException("Can't start in uninitialized state.");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            }

            ScanResult scanResult;

            this.dataOutputFile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                + Path.DirectorySeparatorChar + DefaultFilePrefix
                + DateTime.Now.ToString(DefaultFileDateTimeFormat, CultureInfo.InvariantCulture)
                + '.' + DefaultFileExtension;

            // This is closed when the encapsulating CsvDataAccessProvider is disposed.
            StreamWriter dataOutputWriter = new StreamWriter(this.dataOutputFile, false);

            using (SHA256 hasher = SHA256.Create())
            using (CsvDataAccessProvider dataAccessProvider = new CsvDataAccessProvider(
                dataOutputWriter))
            {
                var fileScanner = new FileScanner(
                    dataAccessProvider, hasher, this.statusOutputWriter);

                DirectoryInfo scanDirectory =
                    new DirectoryInfo(this.CommandLineOptions.ScanDirectory);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                scanResult = fileScanner.ScanDirectory(this.CommandLineOptions.ScanDirectory);

                stopwatch.Stop();
                TimeSpan timeSpan = stopwatch.Elapsed;

                this.statusOutputWriter.WriteLine($"Scan time: {timeSpan}");
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
            Regex invalidPathCharacters = new Regex(
                "[" + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]");
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
        /// </summary>
        private void DisplayHelpText<T>(ParserResult<T> parseResult, IEnumerable<Error> errors)
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
    }
}
