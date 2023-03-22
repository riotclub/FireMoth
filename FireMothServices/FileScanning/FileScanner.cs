// <copyright file="FileScanner.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using RiotClub.FireMoth.Services.Repository;

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using Microsoft.Extensions.Logging;
    using DataAccess;
    using DataAnalysis;

    /// <summary>
    /// Directory scanner implementation that reads the files in a directory and writes the file and hash to an
    /// <see cref="IDataAccessLayer{IFileFingerprint}"/>.
    /// </summary>
    public class FileScanner : IFileScanner
    {
        private readonly IFileFingerprintRepository _fileFingerprintRepository;
        private readonly IFileHasher _hasher;
        private readonly ILogger<FileScanner> _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileScanner"/> class.
        /// </summary>
        /// <param name="fileFingerprintRepository">An <see cref="IFileFingerprintRepository"/> that provides access to
        /// the application backing store.</param>
        /// <param name="hasher">An <see cref="IFileHasher"/> that is used to compute hash values for scanned files.
        /// </param>
        /// <param name="log">An <see cref="ILogger{FileScanner}"/> to which logging output will be written.</param>
        public FileScanner(
            IFileFingerprintRepository fileFingerprintRepository,
            IFileHasher hasher,
            ILogger<FileScanner> log)
        {
            _fileFingerprintRepository = fileFingerprintRepository
                                         ?? throw new ArgumentNullException(nameof(fileFingerprintRepository));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <inheritdoc/>
        public ScanResult ScanDirectory(IScanOptions scanOptions)
        {
            if (scanOptions is null) throw new ArgumentNullException(nameof(scanOptions));

            _log.LogInformation("Scanning directory '{ScanDirectory}'", scanOptions.ScanDirectory);

            var scanResult = new ScanResult();

            if (scanOptions.RecursiveScan)
            {
                _log.LogDebug(
                    "Recursive scan requested; enumerating subdirectories of '{ScanDirectory}'",
                    scanOptions.ScanDirectory);

                var subDirectories = GetSubDirectories(scanOptions.ScanDirectory, scanResult);
                if (subDirectories != null)
                {
                    foreach (var subDirectory in subDirectories)
                    {
                        scanResult += ScanDirectory(new ScanOptions(subDirectory, true));
                    }
                }
            }

            _log.LogDebug("Enumerating files of '{ScanDirectory}'", scanOptions.ScanDirectory);
            var files = GetFiles(scanOptions.ScanDirectory, scanResult);
            if (files == null)
            {
                _log.LogDebug("Skipping empty directory '{ScanDirectory}'", scanOptions.ScanDirectory);
                return scanResult;
            }

            ProcessFiles(files, scanResult);
            _log.LogInformation(
                "Completed scanning '{DirectoryName}' ({ScannedFileCount}/{TotalFileCount} file(s) scanned)",
                scanOptions.ScanDirectory.FullName,
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
            foreach (var file in files)
            {
                _log.LogInformation("Scanning file '{FileName}'", file.Name);
                try
                {
                    using var fileStream = file.OpenRead();
                    _log.LogDebug(
                        "Computing hash for file '{FileName}' using hasher {Hasher}",
                        file.FullName,
                        _hasher.GetType().FullName);
                    var hashString = GetBase64HashFromStream(fileStream);

                    _log.LogDebug(
                        "Adding fingerprint for file '{FileName}' to data access provider",
                        file.FullName);
                    var fingerprint = new FileFingerprint(file.Name, file.DirectoryName, file.Length, hashString);
                    _fileFingerprintRepository.Add(fingerprint);
                    scanResult.ScannedFiles.Add(fingerprint);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    scanResult.SkippedFiles.Add(
                        file.FullName,
                        $"Could not add record for file '{file.FullName}': {ex.Message}; skipping file.");
                    HandleError(
                        file.FullName,
                        ex,
                        scanResult,
                        $"Could not add record for file '{file.FullName}': {ex.Message}; skipping file.",
                        "Could not add record for file '{FileName}': {ExceptionMessage}; skipping file.",
                        file.FullName,
                        ex.Message);
                }
            }
        }

        private IEnumerable<IFileInfo>? GetFiles(IDirectoryInfo directory, ScanResult scanResult)
        {
            IEnumerable<IFileInfo>? result = null;

            try
            {
                result = directory.EnumerateFiles();
            }
            catch (Exception ex) when (
                ex is ArgumentException or IOException or UnauthorizedAccessException)
            {
                HandleError(
                    directory.FullName,
                    ex,
                    scanResult,
                    $"Could not enumerate files of directory '{directory.FullName}': {ex.Message}",
                    "Could not enumerate files of directory '{Directory}': {ExceptionMessage}",
                    directory.FullName,
                    ex.Message);
            }

            return result;
        }

        private IEnumerable<IDirectoryInfo>? GetSubDirectories(
            IDirectoryInfo directory, ScanResult scanResult)
        {
            IEnumerable<IDirectoryInfo>? result = null;

            try
            {
                result = directory.EnumerateDirectories();
            }
            catch (Exception ex) when (
                ex is ArgumentException
                || ex is IOException
                || ex is UnauthorizedAccessException
                || ex is NotSupportedException)
            {
                HandleError(
                    directory.FullName,
                    ex,
                    scanResult,
                    $"Could not enumerate subdirectories of directory '{directory.FullName}': {ex.Message}",
                    "Could not enumerate subdirectories of directory '{Directory}': {ExceptionMessage}",
                    directory.FullName,
                    ex.Message);
            }

            return result;
        }

        private void HandleError(
            string path,
            Exception exception,
            ScanResult scanResult,
            string scanResultMessage,
            string logMessageTemplate,
            params string[] logMessageArguments)
        {
            if (exception is null)
            {
                _log.LogError(logMessageTemplate, logMessageArguments);
            }
            else
            {
                _log.LogError(exception, logMessageTemplate, logMessageArguments);
            }

            scanResult.Errors.Add(new ScanError(path, scanResultMessage, exception));
        }

        private string GetBase64HashFromStream(Stream stream)
        {
            byte[] hashBytes = _hasher.ComputeHashFromStream(stream);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
