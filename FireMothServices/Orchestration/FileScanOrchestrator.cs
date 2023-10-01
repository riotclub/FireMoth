// <copyright file="FileScanOrchestrator.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Orchestration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DataAccess;
using DataAnalysis;
using FileScanning;
using Repository;

/// <summary>
/// File scanner implementation that scans a collection of files and writes file fingerprint data to an
/// <see cref="IDataAccessLayer{TValue}"/>.
/// </summary>
public class FileScanOrchestrator : IFileScanOrchestrator
{
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly IFileHasher _fileHasher;
    private readonly ILogger<FileScanOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileScanOrchestrator"/> class.
    /// </summary>
    /// <param name="fileFingerprintRepository">An <see cref="IFileFingerprintRepository"/> that provides access to
    /// the application backing store.</param>
    /// <param name="fileHasher">An <see cref="IFileHasher"/> that is used to compute hash values for scanned files.
    /// </param>
    /// <param name="logger">An <see cref="ILogger{FileScanOrchestrator}"/> to which logging output will be written.
    /// </param>
    public FileScanOrchestrator(
        IFileFingerprintRepository fileFingerprintRepository,
        IFileHasher fileHasher,
        ILogger<FileScanOrchestrator> logger)
    {
        _fileFingerprintRepository = fileFingerprintRepository
            ?? throw new ArgumentNullException(nameof(fileFingerprintRepository));
        _fileHasher = fileHasher ?? throw new ArgumentNullException(nameof(fileHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ScanResult> ScanFilesAsync(IEnumerable<string> files)
    {
        var fileList = files.ToList();
        _logger.LogInformation("Scanning {FileCount} files...", fileList.Count);
        var scanResult = new ScanResult();
        await ProcessFiles(fileList, scanResult);

        _logger.LogInformation(
            "Completed scan ({ScannedFileCount}/{TotalFileCount} file(s) scanned)",
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
    private async Task ProcessFiles(IEnumerable<string> files, ScanResult scanResult)
    {
        foreach (var file in files)
        {
            _logger.LogInformation("Scanning file '{FileName}'", file);
            var fileInfo = new FileInfo(file);

            try
            {
                await using var fileStream = fileInfo.OpenRead();
                _logger.LogDebug(
                    "Computing hash for file '{FileName}' using fileHasher {Hasher}",
                    fileInfo.Name,
                    _fileHasher.GetType().FullName);
                
                // TODO: Convert to async implementation
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