// <copyright file="FileScanner.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Security;
    using FireMothServices.DataAccess;
    using FireMothServices.DataAnalysis;
    using RiotClub.FireMoth.Services.DataAccess;

    /// <summary>
    /// Directory scanner implementation that reads the files in a directory and writes the file
    /// and hash to an <see cref="IDataAccessProvider"/>.
    /// </summary>
    public class FileScanner : IFileScanner
    {
        private readonly IDataAccessProvider dataAccessProvider;
        private readonly IFileHasher hasher;
        private readonly TextWriter logWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileScanner"/> class.
        /// </summary>
        /// <param name="dataAccessProvider">A <see cref="IDataAccessProvider"/> that provides
        /// access to the application backing store.</param>
        /// <param name="hasher">An <see cref="IFileHasher"/> that is used to compute hash values
        /// for scanned files.</param>
        /// <param name="logWriter">A <see cref="TextWriter"/> to which logging output will be
        /// written.</param>
        public FileScanner(
            IDataAccessProvider dataAccessProvider, IFileHasher hasher, TextWriter logWriter)
        {
            this.dataAccessProvider =
                dataAccessProvider ?? throw new ArgumentNullException(nameof(dataAccessProvider));
            this.hasher =
                hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.logWriter =
                logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        }

        /// <summary>
        /// Gets the total number of files scanned by this <see cref="FileScanner"/>.
        /// </summary>
        public int TotalFilesScanned { get; private set; }

        /// <summary>
        /// Gets the total number of files that were skpped by this <see cref="FileScanner"/>.
        /// </summary>
        public int TotalFilesSkipped { get; private set; }

        /// <inheritdoc/>
        public ScanResult ScanDirectory(IDirectoryInfo directory, bool recursive)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!directory.Exists)
            {
                this.logWriter.WriteLine(
                    "Error: \"{0}\" is not a valid directory. Ensure the directory does not have a trailing backslash.",
                    directory);
                return ScanResult.ScanFailure;
            }

            this.logWriter.WriteLine($"Scanning directory \"{directory}\"...");

            if (recursive)
            {
                IEnumerable<IDirectoryInfo> subdirectories;
                try
                {
                    subdirectories = directory.EnumerateDirectories();
                }
                catch (IOException ex)
                {
                    this.logWriter.WriteLine(
                        $"Could not enumerate subdirectories (I/O): {ex.Message}");
                    return ScanResult.ScanFailure;
                }
                catch (UnauthorizedAccessException ex)
                {
                    this.logWriter.WriteLine(
                        $"Could not enumerate subdirectories (unauthorized): {ex.Message}");
                    return ScanResult.ScanFailure;
                }
                catch (SecurityException ex)
                {
                    this.logWriter.WriteLine(
                        $"Could not enumerate subdirectories (security): {ex.Message}");
                    return ScanResult.ScanFailure;
                }

                if (subdirectories.Any())
                {
                    foreach (IDirectoryInfo subDirectory in subdirectories)
                    {
                        this.ScanDirectory(subDirectory, true);
                    }
                }
            }

            (int scannedFiles, int skippedFiles) scanCount =
                this.ProcessFiles(directory.EnumerateFiles());
            this.logWriter.WriteLine(
                "Completed scanning \"{0}\" ({1}/{2} file(s) scanned).",
                directory.FullName,
                scanCount.scannedFiles,
                scanCount.scannedFiles + scanCount.skippedFiles);
            this.TotalFilesScanned += scanCount.scannedFiles;
            this.TotalFilesSkipped += scanCount.skippedFiles;

            return ScanResult.ScanSuccess;
        }

        /// <summary>
        /// Hashes a set of files and records the filename and hash string.
        /// </summary>
        /// <param name="files">The set of files to hash and record.</param>
        /// <returns>An tuple <c>(int, int)</c> indicating the number of files scanned and the
        /// number of files skipped, respectively.</returns>
        protected internal virtual (int scannedFiles, int skippedFiles) ProcessFiles(
            IEnumerable<System.IO.Abstractions.IFileInfo> files)
        {
            Contract.Requires(files != null);

            int scannedFiles = 0;
            int skippedFiles = 0;

            foreach (System.IO.Abstractions.IFileInfo file in files)
            {
                try
                {
                    using (Stream fileStream = file.OpenRead())
                    {
                        this.logWriter.Write(file.FullName);
                        var hashString = this.GetBase64HashFromStream(fileStream);
                        this.dataAccessProvider.AddFileRecord(
                            new FileFingerprint(file, hashString));
                        this.logWriter.WriteLine($" [{hashString}]");
                        scannedFiles++;
                    }
                }
                catch (IOException exception)
                {
                    this.logWriter.WriteLine(
                        $"Could not read from \"{file.FullName}\": {exception.Message}");
                    skippedFiles++;
                }
            }

            return (scannedFiles, skippedFiles);
        }

        /// <summary>
        /// Calculates a hash of the provided stream's data and returns a base 64 encoded string of
        /// the hash.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the data to hash.</param>
        /// <returns>A base 64 encoded <see cref="string"/> of the hash.</returns>
        private string GetBase64HashFromStream(Stream stream)
        {
            byte[] hashBytes = this.hasher.ComputeHashFromStream(stream);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
