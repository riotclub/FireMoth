// <copyright file="FileScanner.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Security;
    using Microsoft.Extensions.Logging;
    using RiotClub.FireMoth.Services.DataAccess;
    using RiotClub.FireMoth.Services.DataAnalysis;

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
        /// <param name="log">A <see cref="TextWriter"/> to which logging output will be
        /// written.</param>
        public FileScanner(
            IDataAccessProvider dataAccessProvider, IFileHasher hasher, ILogger<FileScanner> log)
        {
            this.dataAccessProvider =
                dataAccessProvider ?? throw new ArgumentNullException(nameof(dataAccessProvider));
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <inheritdoc/>
        public ScanResult ScanDirectory(IDirectoryInfo directory, bool recursive)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            this.log.LogInformation("Scanning directory {ScanDirectory}", directory);

            ScanResult scanResult = new ScanResult();

            if (recursive)
            {
                this.log.LogDebug(
                    "Recursive scan requested; enumerating subdirectories of {ScanDirectory}",
                    directory);

                var subDirectories = this.GetSubDirectories(directory, scanResult);

                if (subDirectories != null)
                {
                    foreach (IDirectoryInfo subDirectory in subDirectories)
                    {
                        scanResult += this.ScanDirectory(subDirectory, true);
                    }
                }
            }

            this.log.LogDebug("Enumerating files of {ScanDirectory}", directory);
            var files = this.GetFiles(directory, scanResult);
            if (files == null)
            {
                return scanResult;
            }

            this.ProcessFiles(files, scanResult);
            this.log.LogInformation(
                "Completed scanning {DirectoryName} ({ScannedFileCount}/{TotalFileCount} file(s) scanned)",
                directory.FullName,
                scanResult.ScannedFiles.Count,
                scanResult.ScannedFiles.Count + scanResult.SkippedFiles.Count);

            return scanResult;
        }

        /// <summary>
        /// Hashes a set of files and records the filename and hash string.
        /// </summary>
        /// <param name="files">The set of files to hash and record.</param>
        /// <param name="scanResult">A <see cref="ScanResult"/> to which the names of scanned and
        /// skipped files will be added.</param>
        protected internal virtual void ProcessFiles(
            IEnumerable<IFileInfo> files, ScanResult scanResult)
        {
            foreach (IFileInfo file in files)
            {
                try
                {
                    using (Stream fileStream = file.OpenRead())
                    {
                        var hashString = this.GetBase64HashFromStream(fileStream);
                        this.log.LogDebug(
                            "Adding fingerprint for file {FileName} to data access provider",
                            file.FullName,
                            hashString);
                        this.dataAccessProvider.AddFileRecord(
                            new FileFingerprint(file, hashString));
                        scanResult.ScannedFiles.Add(file.FullName);
                    }
                }
                catch (IOException exception)
                {
                    var messageTemplate =
                        "Could not add record for file {FileName} (skipping): {ExceptionMessage}";
                    this.log.LogError(messageTemplate, file.FullName, exception.Message);
                    scanResult.SkippedFiles.Add(
                        file.FullName,
                        string.Format(messageTemplate, file.FullName, exception.Message));
                    scanResult.Errors.Add(
                        new ScanError(
                            file.FullName, "Could not add record for file.", exception));
                }
            }
        }

        // Attempts to retrieve all files in the provided directory.
        private IEnumerable<IFileInfo>? GetFiles(
            IDirectoryInfo directory, ScanResult scanResult)
        {
            IEnumerable<IFileInfo>? result = null;

            try
            {
                result = directory.EnumerateFiles();
            }
            catch (IOException exception)
            {
                this.log.LogError(
                    exception,
                    "Could not enumerate files of directory {Directory}: {ExceptionMessage}",
                    directory,
                    exception.Message);
                scanResult.Errors.Add(
                    new ScanError(
                        directory.FullName,
                        $"Could not enumerate files of {directory.FullName}",
                        exception));
            }
            catch (SecurityException exception)
            {
                this.log.LogError(
                    exception,
                    "Could not enumerate files of directory {Directory}: {ExceptionMessage}",
                    directory,
                    exception.Message);
                scanResult.Errors.Add(
                    new ScanError(
                        directory.FullName,
                        $"Could not enumerate files of {directory.FullName}",
                        exception));
            }

            return result;
        }

        // Attempts to retrieve all immediate subdirectories of the provided directory.
        private IEnumerable<IDirectoryInfo>? GetSubDirectories(
            IDirectoryInfo directory, ScanResult scanResult)
        {
            IEnumerable<IDirectoryInfo>? result = null;

            try
            {
                result = directory.EnumerateDirectories();
            }
            catch (IOException exception)
            {
                this.log.LogError(
                    exception,
                    "Could not enumerate subdirectories of {Directory}: {ExceptionMessage}",
                    directory,
                    exception.Message);
                scanResult.Errors.Add(
                    new ScanError(
                        directory.FullName,
                        $"Could not enumerate subdirectories of {directory.FullName}",
                        exception));
            }
            catch (SecurityException exception)
            {
                this.log.LogError(
                    exception,
                    "Could not enumerate subdirectories of {Directory}: {ExceptionMessage}",
                    directory,
                    exception.Message);
                scanResult.Errors.Add(
                    new ScanError(
                        directory.FullName,
                        $"Could not enumerate subdirectories of {directory.FullName}",
                        exception));
            }

            return result;
        }

        // Returns a base 64 representation of a data stream's hash.
        private string GetBase64HashFromStream(Stream stream)
        {
            this.log.LogDebug(
                "Calculating hash from file stream using hasher {Hasher}",
                this.hasher.GetType().FullName);
            byte[] hashBytes = this.hasher.ComputeHashFromStream(stream);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
