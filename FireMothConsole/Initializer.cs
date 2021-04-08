// <copyright file="Initializer.cs" company="Dark Hours Development">
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
    using System.Text.RegularExpressions;
    using FireMothServices.DataAnalysis;
    using Microsoft.Extensions.Options;
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

        private readonly TextWriter statusOutputWriter;
        private readonly CommandLineOptions options;
        private string dataOutputFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="Initializer"/> class.
        /// </summary>
        /// <param name="processArguments">An array of <see cref="string"/>s containing process
        /// startup (command line) arguments.</param>
        /// <param name="outputWriter">A <see cref="TextWriter"/> that initialization messages will
        /// be written to.</param>
        public Initializer(TextWriter outputWriter, IOptions<CommandLineOptions> processArguments)
        {
            this.options = processArguments.Value
                ?? throw new ArgumentNullException(nameof(processArguments));
            this.statusOutputWriter = outputWriter
                ?? throw new ArgumentNullException(nameof(outputWriter));

            if (ContainsInvalidPathCharacters(this.options.ScanDirectory))
            {
                throw new ArgumentException("Scan path contains invalid characters.");
            }
        }

        /// <summary>
        /// Begins scanning the directory specified during application startup.
        /// </summary>
        /// <returns>An <see cref="ExitState"/> indicating the result of the directory scan
        /// operation.</returns>
        public ExitState Start()
        {
            ScanResult scanResult;

            this.dataOutputFile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                + Path.DirectorySeparatorChar + DefaultFilePrefix
                + DateTime.Now.ToString(DefaultFileDateTimeFormat, CultureInfo.InvariantCulture)
                + '.' + DefaultFileExtension;

            // This is closed when the encapsulating CsvDataAccessProvider is disposed.
            StreamWriter dataOutputWriter = new StreamWriter(this.dataOutputFile, false);

            using (SHA256FileHasher hasher = new SHA256FileHasher())
            using (CsvDataAccessProvider dataAccessProvider =
                new CsvDataAccessProvider(dataOutputWriter))
            {
                var fileScanner = new FileScanner(
                    dataAccessProvider, hasher, this.statusOutputWriter);
                var scanDirectory = new FileSystem().DirectoryInfo.FromDirectoryName(
                    this.options.ScanDirectory);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                scanResult = fileScanner.ScanDirectory(
                    scanDirectory, this.options.RecursiveScan);
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
    }
}
