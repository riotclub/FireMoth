// <copyright file="DirectoryScanOrchestrator.cs" company="Riot Club">
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
using FileScanning;

/// <summary>
/// Directory scanner implementation that reads the files in a directory and writes the file and hash to an
/// <see cref="IDataAccessLayer{TValue}"/>.
/// </summary>
public class DirectoryScanOrchestrator : IDirectoryScanOrchestrator
{
    private readonly IFileScanOrchestrator _fileScanOrchestrator;
    private readonly ILogger<FileScanOrchestrator> _logger;

    private const string AllFilesSearchPattern = "*";

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryScanOrchestrator"/> class.
    /// </summary>
    /// <param name="fileScanOrchestrator">An <see cref="IFileScanOrchestrator"/> that manages scanning of files.
    /// </param>
    /// <param name="logger">An <see cref="ILogger{FileScanOrchestrator}"/> to which logging output will be written.
    /// </param>
    public DirectoryScanOrchestrator(IFileScanOrchestrator fileScanOrchestrator, ILogger<FileScanOrchestrator> logger)
    {
        _fileScanOrchestrator = fileScanOrchestrator ?? throw new ArgumentNullException(nameof(fileScanOrchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ScanResult> ScanDirectoryAsync(string scanDirectory, bool recursive)
    {
        if (scanDirectory is null)
            throw new ArgumentNullException(nameof(scanDirectory), "Scan directory cannot be null.");
        
        _logger.LogInformation(
            "Scanning directory '{ScanDirectory}' (recursive: {Recursive})", scanDirectory, recursive);

        List<string> fileList; 
        // try
        // {
            fileList = Directory
                .EnumerateFiles(
                    scanDirectory, AllFilesSearchPattern, new EnumerationOptions { RecurseSubdirectories = recursive })
                .ToList();
        //}
        // catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        // {
        //     _logger.LogError(
        //         ex,
        //         "Could not enumerate files of directory '{Directory}': {ExceptionMessage}",
        //         scanDirectory,
        //         ex.Message);
        //
        //     var scanError = new ScanError(
        //         scanDirectory, $"Could not enumerate files of directory '{scanDirectory}': {ex.Message}", ex);
        //     var scanResult = new ScanResult { Errors = { scanError } };
        //
        //     return scanResult;
        // }
        
        return await _fileScanOrchestrator.ScanFilesAsync(fileList);
    }
}