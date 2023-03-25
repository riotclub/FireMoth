// <copyright file="OnDemandScanOrchestrator.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using RiotClub.FireMoth.Services.Output;
using RiotClub.FireMoth.Services.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.DataAccess;
using RiotClub.FireMoth.Services.DataAnalysis;

namespace RiotClub.FireMoth.Services.FileScanning;

/// <summary>
/// Directory scanner implementation that reads the files in a directory and writes the file and hash to an
/// <see cref="IDataAccessLayer{TValue}"/>.
/// </summary>
public class OnDemandScanOrchestrator : RiotClub.FireMoth.Services.Orchestration.IScanOrchestrator
{
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly IFileHasher _fileHasher;
    private readonly IFileFingerprintWriter _fileFingerprintWriter;
    private readonly IScanOptions _scanOptions;
    private readonly ILogger<OnDemandScanOrchestrator> _logger;

    private const string AllDirectoriesSearchPattern = "*";

    /// <summary>
    /// Initializes a new instance of the <see cref="OnDemandScanOrchestrator"/> class.
    /// </summary>
    /// <param name="fileFingerprintRepository">An <see cref="IFileFingerprintRepository"/> that provides access to
    /// the application backing store.</param>
    /// <param name="fileHasher">An <see cref="IFileHasher"/> that is used to compute hash values for scanned files.
    /// </param>
    /// <param name="fileFingerprintWriter">An <see cref="IFileFingerprintWriter"/> used to write file fingerprint data
    /// to an output destination.</param>
    /// <param name="scanOptions">An <see cref="IScanOptions"/> containing options that control the file scan process.
    /// </param>
    /// <param name="logger">An <see cref="ILogger{OnDemandScanOrchestrator}"/> to which logging output will be written.
    /// </param>
    public OnDemandScanOrchestrator(
        IFileFingerprintRepository fileFingerprintRepository,
        IFileHasher fileHasher,
        IFileFingerprintWriter fileFingerprintWriter,
        IScanOptions scanOptions,
        ILogger<OnDemandScanOrchestrator> logger)
    {
        _fileFingerprintRepository = fileFingerprintRepository
                                     ?? throw new ArgumentNullException(nameof(fileFingerprintRepository));
        _fileHasher = fileHasher ?? throw new ArgumentNullException(nameof(fileHasher));
        _fileFingerprintWriter = fileFingerprintWriter
                                 ?? throw new ArgumentNullException(nameof(fileFingerprintWriter));
        _scanOptions = scanOptions ?? throw new ArgumentNullException(nameof(scanOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ScanResult> ScanDirectoryAsync()
    {
        _logger.LogInformation("Scanning directory '{ScanDirectory}'", _scanOptions.ScanDirectory);

        var scanResult = new ScanResult();

        var directoryList = new List<string>
        {
            _scanOptions.ScanDirectory.FullName
        };

        if (_scanOptions.RecursiveScan)
        {
            _logger.LogDebug(
                "Recursive scan requested; enumerating subdirectories of '{ScanDirectory}'",
                _scanOptions.ScanDirectory);

            // var subDirectories = GetSubDirectories(_scanOptions.ScanDirectory, scanResult);

            var subDirs = Directory.EnumerateDirectories(
                _scanOptions.ScanDirectory.FullName,
                AllDirectoriesSearchPattern,
                new EnumerationOptions { RecurseSubdirectories = true });
            directoryList.AddRange(subDirs);

            //if (subDirectories != null)
            //{
            //    foreach (var subDirectory in subDirectories)
            //    {
            //        scanResult += ScanDirectory();
            //    }
            //}
        }

        foreach (var scanDirectory in directoryList)
        {
            _logger.LogDebug("Enumerating files of '{ScanDirectory}'", scanDirectory);
            var files = GetFiles(scanDirectory, scanResult);
            if (files == null)
            {
                _logger.LogDebug("Skipping empty directory '{ScanDirectory}'", _scanOptions.ScanDirectory);
                return scanResult;
            }

            await ProcessFiles(files, scanResult);
            _logger.LogInformation(
                "Completed scanning '{DirectoryName}' ({ScannedFileCount}/{TotalFileCount} file(s) scanned)",
                _scanOptions.ScanDirectory.FullName,
                scanResult.ScannedFiles.Count,
                scanResult.ScannedFiles.Count + scanResult.SkippedFiles.Count);
        }

        return scanResult;
    }

    /// <summary>
    /// Hashes a set of files and records the filename and hash string.
    /// </summary>
    /// <param name="files">The set of files to hash and record.</param>
    /// <param name="scanResult">A <see cref="ScanResult"/> to which the names of scanned and
    /// skipped files will be added.</param>
    protected internal virtual async Task ProcessFiles(
        IEnumerable<string> files, ScanResult scanResult)
    {
        foreach (var file in files)
        {
            _logger.LogInformation("Scanning file '{FileName}'", file);
            var fileInfo = new FileInfo(file);

            try
            {
                //using var fileStream = file.OpenRead();
                using var fileStream = fileInfo.OpenRead();
                _logger.LogDebug(
                    "Computing hash for file '{FileName}' using fileHasher {Hasher}",
                    fileInfo.Name,
                    _fileHasher.GetType().FullName);
                var hashString = GetBase64HashFromStream(fileStream);

                _logger.LogDebug(
                    "Adding fingerprint for file '{FileName}' to data access provider",
                    fileInfo.Name);
                
                var fingerprint = new FileFingerprint(
                    fileInfo.Name,
                    fileInfo.DirectoryName ?? string.Empty,
                    fileInfo.Length,
                    hashString);
                await _fileFingerprintRepository.AddAsync(fingerprint);
                scanResult.ScannedFiles.Add(fingerprint);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                scanResult.SkippedFiles.Add(
                    fileInfo.FullName,
                    $"Could not add record for file '{fileInfo.FullName}': {ex.Message}; skipping file.");
                HandleError(
                    fileInfo.FullName,
                    ex,
                    scanResult,
                    $"Could not add record for file '{fileInfo.FullName}': {ex.Message}; skipping file.",
                    "Could not add record for file '{FileName}': {ExceptionMessage}; skipping file.",
                    fileInfo.FullName,
                    ex.Message);
            }
        }
    }

    private IEnumerable<string>? GetFiles(string directory, ScanResult scanResult)
    {
        IEnumerable<string>? result = null;

        try
        {
            result = Directory.EnumerateFiles(directory);
        }
        catch (Exception ex) when (
            ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            HandleError(
                directory,
                ex,
                scanResult,
                $"Could not enumerate files of directory '{directory}': {ex.Message}",
                "Could not enumerate files of directory '{Directory}': {ExceptionMessage}",
                directory,
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
        params object?[] logMessageArguments)
    {
        _logger.LogError(exception, logMessageTemplate, logMessageArguments);

        scanResult.Errors.Add(new ScanError(path, scanResultMessage, exception));
    }

    private string GetBase64HashFromStream(Stream stream)
    {
        var hashBytes = _fileHasher.ComputeHashFromStream(stream);
        return Convert.ToBase64String(hashBytes);
    }
}