// <copyright file="DirectoryScanOrchestrator.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Orchestration;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Linq;
using DataAccess;
using FileScanning;
using Microsoft.Extensions.Options;

/// <summary>
/// Directory scanner implementation that reads the files in a directory and writes the file and
/// hash to an <see cref="IDataAccessLayer{TValue}"/>.
/// </summary>
public class DirectoryScanOrchestrator : IDirectoryScanOrchestrator
{
    private readonly IFileScanOrchestrator _fileScanOrchestrator;
    private readonly IFileSystem _fileSystem;
    private readonly DirectoryScanOptions _directoryScanOptions;
    private readonly ILogger<DirectoryScanOrchestrator> _logger;

    private const string AllFilesSearchPattern = "*";

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryScanOrchestrator"/> class.
    /// </summary>
    /// <param name="fileScanOrchestrator">An <see cref="IFileScanOrchestrator"/> that manages
    /// scanning of files.</param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> that provides file system I/O access.
    /// </param>
    /// <param name="directoryScanOptions">An <see cref="IOptions{DirectoryScanOptions}"/>
    /// containing the configured options for this directory scan orchestrator.</param>
    /// <param name="logger">An <see cref="ILogger{FileScanOrchestrator}"/> to which logging output
    /// will be written.</param>
    public DirectoryScanOrchestrator(
        IFileScanOrchestrator fileScanOrchestrator,
        IFileSystem fileSystem,
        IOptions<DirectoryScanOptions> directoryScanOptions,
        ILogger<DirectoryScanOrchestrator> logger)
    {
        _fileScanOrchestrator = fileScanOrchestrator
                                ?? throw new ArgumentNullException(nameof(fileScanOrchestrator));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        ArgumentNullException.ThrowIfNull(directoryScanOptions);
        if (directoryScanOptions.Value.Directory is null)
        {
            throw new ArgumentException(
                "IOptions<DirectoryScanOptions>.Value.Directory cannot be null",
                nameof(directoryScanOptions));
        }
        _directoryScanOptions = directoryScanOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ScanResult> ScanDirectoryAsync()
    {
        _logger.LogInformation(
            "Scanning directory '{ScanDirectory}' (recursive: {Recursive})",
            _directoryScanOptions.Directory,
            _directoryScanOptions.Recursive);

        // TODO: Null forgiving op used here because the null check for
        // DirectoryScanOptions.Directory was moved to the constructor. Need to consider/test for
        // the possibility of options being modified between object construction and invocation of
        // this method, which could result in a null reference exception being thrown.
        var fileList = _fileSystem.Directory
            .EnumerateFiles(
                _directoryScanOptions.Directory!,
                AllFilesSearchPattern,
                new EnumerationOptions { RecurseSubdirectories = _directoryScanOptions.Recursive })
            .ToList();

        return fileList.Count > 0
            ? await _fileScanOrchestrator.ScanFilesAsync(fileList)
            : new ScanResult();
    }
}