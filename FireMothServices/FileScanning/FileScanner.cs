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
    using Microsoft.Extensions.Logging;
    using RiotClub.FireMoth.Services.DataAccess;

    /// <summary>
    /// Directory scanner implementation that reads the files in a directory and writes the file
    /// and hash to an <see cref="IDataAccessProvider"/>.
    /// </summary>
    public class FileScanner : IFileScanner
    {
        private readonly IDataAccessProvider dataAccessProvider;
        private readonly IFileHasher hasher;
        private readonly ILogger<FileScanner> log;

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
            IDataAccessProvider dataAccessProvider, IFileHasher hasher, ILogger<FileScanner> log)
        {
            this.dataAccessProvider =
                dataAccessProvider ?? throw new ArgumentNullException(nameof(dataAccessProvider));
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
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

            // if (!directory.Exists)
            // {
            //    this.log.LogError(
            //        "Error: \"{0}\" is not a valid directory. Ensure the directory does not have a trailing backslash.",
            //        directory);
            //    return ScanResult.ScanFailure;
            // }
            this.log.LogInformation("Scanning directory {ScanDirectory}...", directory);

            if (recursive)
            {
                this.log.LogDebug(
                    "Recursive scan requested; enumerating subdirectories of {ScanDirectory}...",
                    directory);
                if (!this.TryGetSubDirectories(
                    directory, out IEnumerable<IDirectoryInfo> subdirectories))
                {
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

            (int scannedFiles, int skippedFiles) = this.ProcessFiles(directory.EnumerateFiles());
            this.log.LogInformation(
                "Completed scanning {DirectoryName} ({ScannedFileCount}/{TotalFileCount} file(s) scanned).",
                directory.FullName,
                scannedFiles,
                scannedFiles + skippedFiles);
            this.TotalFilesScanned += scannedFiles;
            this.TotalFilesSkipped += skippedFiles;

            return ScanResult.ScanSuccess;
        }

        /// <summary>
        /// Hashes a set of files and records the filename and hash string.
        /// </summary>
        /// <param name="files">The set of files to hash and record.</param>
        /// <returns>An tuple <c>(int, int)</c> indicating the number of files scanned and the
        /// number of files skipped, respectively.</returns>
        protected internal virtual (int scannedFiles, int skippedFiles) ProcessFiles(
            IEnumerable<IFileInfo> files)
        {
            Contract.Requires(files != null);

            int scannedFiles = 0;
            int skippedFiles = 0;

            foreach (IFileInfo file in files)
            {
                try
                {
                    using (Stream fileStream = file.OpenRead())
                    {
                        var hashString = this.GetBase64HashFromStream(fileStream);
                        this.dataAccessProvider.AddFileRecord(
                            new FileFingerprint(file, hashString));
                        this.log.LogDebug(
                            "Adding record for file {FileName} with hash {HashString} to data access provider.",
                            file.FullName,
                            hashString);
                        scannedFiles++;

                        if (scannedFiles % 10 == 0)
                        {
                            this.log.LogInformation(
                                "Scanned {ScannedFileCount} files...", scannedFiles);
                        }
                    }
                }
                catch (IOException exception)
                {
                    this.log.LogError(
                        "Could not add record for file {FileName}: {ExceptionMessage} (skipping)",
                        file.FullName,
                        exception.Message);
                    skippedFiles++;
                }
            }

            return (scannedFiles, skippedFiles);
        }

        /// <summary>
        /// Attempts to retrieve all immediate subdirectories of the provided directory.
        /// </summary>
        /// <param name="directory">An <see cref="IDirectoryInfo"/> representing the directory for
        /// which subdirectories will be enumerated.</param>
        /// <param name="subdirectories">An <see cref="IEnumerable{IDirectoryInfo}"/> collection
        /// containing the subdirectories of the provided directory.</param>
        /// <returns><c>true</c> if subdirectories were successfully enumerated and set in
        /// <paramref name="subdirectories"/>.</returns>
        private bool TryGetSubDirectories(
            IDirectoryInfo directory, out IEnumerable<IDirectoryInfo> subdirectories)
        {
            try
            {
                this.log.LogDebug("Enumerating subdirectories of directories");
                subdirectories = directory.EnumerateDirectories();
            }
            catch (IOException ex)
            {
                this.log.LogError(
                    ex, "Could not enumerate subdirectories of {Directory}.", directory);
                subdirectories = null;
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                this.log.LogError(
                    ex, "Could not enumerate subdirectories of {Directory}.", directory);
                subdirectories = null;
                return false;
            }
            catch (SecurityException ex)
            {
                this.log.LogError(
                    ex, "Could not enumerate subdirectories of {Directory}.", directory);
                subdirectories = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates a hash of the provided stream's data and returns a base 64 encoded string of
        /// the hash.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the data to hash.</param>
        /// <returns>A base 64 encoded <see cref="string"/> of the hash.</returns>
        private string GetBase64HashFromStream(Stream stream)
        {
            this.log.LogDebug(
                "Calculating hash from file stream using hasher {Hasher}...",
                this.hasher.GetType().FullName);
            byte[] hashBytes = this.hasher.ComputeHashFromStream(stream);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
