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
    using System.Security.Cryptography;
    using RiotClub.FireMoth.Services.DataAccess;
    using RiotClub.FireMoth.Services.FileScanning;

    /// <summary>
    /// Validates application startup parameters and initiates directory scan.
    /// </summary>
    public class Initializer
    {
        private const string UsageMessage =
            "Usage: FireMoth.Console.exe --directory [ScanDirectory]";

        private const string DefaultFilePrefix = "FireMothData_";
        private const string DefaultFileExtension = "csv";
        private const string DefaultFileDateTimeFormat = "yyyyMMdd-HHmmss";

        private readonly string[] processArguments;
        private TextWriter statusOutputWriter;
        private string dataOutputFile;
        private string scanPath;

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
        /// Performs initialization tasks to prepare application for startup. This must be called
        /// before the call to the <see cref="Start"/> method.
        /// </summary>
        /// <returns><c>true</c> if initialization was successful and the application is ready for
        /// startup.</returns>
        public bool Initialize()
        {
            if (!this.ValidateStartupArguments(this.processArguments))
            {
                this.DisplayUsageMessage();
                return false;
            }

            this.Initialized = true;
            return true;
        }

        /// <summary>
        /// Begins scanning the directory specified during application startup.
        /// </summary>
        /// <returns>An <see cref="ExitState"/> indicating the result of the directory scan
        /// operation.</returns>
        public ExitState Start()
        {
            if (!this.Initialize())
            {
                return ExitState.StartupError;
            }

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

            using (SHA256 hasher = SHA256.Create())
            using (StreamWriter dataOutputWriter = new StreamWriter(this.dataOutputFile, false))
            {
                CsvDataAccessProvider dataAccessProvider =
                    new CsvDataAccessProvider(dataOutputWriter);
                var fileScanner =
                    new FileScanner(dataAccessProvider, hasher, this.statusOutputWriter);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                scanResult = fileScanner.ScanDirectory(this.scanPath);

                stopwatch.Stop();
                TimeSpan timeSpan = stopwatch.Elapsed;

                this.statusOutputWriter.WriteLine($"Scan time: {timeSpan}");
            }

            return scanResult == ScanResult.ScanSuccess ? ExitState.Normal : ExitState.RuntimeError;
        }

        /// <summary>
        /// Verifies that the provided arguments are valid process startup arguments.
        /// </summary>
        /// <param name="arguments">An array of <see cref="string"/> containing process command line
        /// arguments.</param>
        /// <returns><c>true</c> if the provided array of arguments contains only valid command line
        /// arguments, <c>false</c> otherwise.</returns>
        private bool ValidateStartupArguments(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                return false;
            }

            if (arguments[0] != "-d" && arguments[0] != "--directory")
            {
                this.statusOutputWriter.WriteLine(
                    "Invalid option: {0}" + Environment.NewLine, arguments[0]);
                return false;
            }

            if (arguments.Length < 2)
            {
                this.statusOutputWriter.WriteLine(
                    "A valid scan directory is required." + Environment.NewLine);
                return false;
            }

            if (arguments.Length > 2)
            {
                this.statusOutputWriter.WriteLine(
                    "Invalid option: {0}" + Environment.NewLine, arguments[2]);
                return false;
            }

            this.scanPath = arguments[1];
            return true;
        }

        /// <summary>
        /// Writes the application usage message to the status output.
        /// </summary>
        private void DisplayUsageMessage()
        {
            this.statusOutputWriter.Write(UsageMessage + Environment.NewLine);
        }
    }
}
