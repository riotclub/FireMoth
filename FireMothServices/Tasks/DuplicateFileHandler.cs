// <copyright file="DuplicateFileHandler.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RiotClub.FireMoth.Services.Repository;

/// <summary>A task handler that performs operations on files containing duplicate hash values.
/// </summary>
public abstract class DuplicateFileHandler : ITaskHandler
{
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly DuplicateFileHandlingOptions _duplicateFileHandlingOptions;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DuplicateFileHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="DuplicateFileHandler"/> class.
    /// </summary>
    /// <param name="fileFingerprintRepository">An <see cref="IFileFingerprintRepository"/> used to
    /// retrieve duplicate records for handling and modify or delete records after handling.
    /// </param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> that provides file system I/O access.
    /// </param>
    /// <param name="duplicateFileHandlingOptions">An
    /// <see cref="IOptions{DuplicateFileHandlingOptions}"/> containing configured options for this
    /// task handler.</param>
    /// <param name="logger">An <see cref="ILogger{DuplicateFileHandler}"/> to which logging output
    /// will be written.</param>
    public DuplicateFileHandler(
        IFileFingerprintRepository fileFingerprintRepository,
        IFileSystem fileSystem,
        IOptions<DuplicateFileHandlingOptions> duplicateFileHandlingOptions,
        ILogger<DuplicateFileHandler> logger)
    {
        Guard.IsNotNull(fileFingerprintRepository);
        Guard.IsNotNull(fileSystem);
        Guard.IsNotNull(logger);
        Guard.IsNotNull(duplicateFileHandlingOptions);
        _fileFingerprintRepository = fileFingerprintRepository;
        _duplicateFileHandlingOptions = duplicateFileHandlingOptions.Value;
        _fileSystem = fileSystem;
        _logger = logger;
    }
    
    /// <summary> Runs the duplicate file handler task by either deleting or moving files with
    /// duplicate hashes in the <see cref="IFileFingerprintRepository"/> depending on the provided
    /// <see cref="_duplicateFileHandlingOptions"/>.</summary>
    public async Task RunTaskAsync()
    {
        _logger.LogDebug("Running task for DuplicateFileHandler.");

        switch (_duplicateFileHandlingOptions.DuplicateFileHandlingMethod)
        {
            case DuplicateFileHandlingMethod.Delete:
                _logger.LogDebug("Performing delete operation for duplicate files.");
                await DeleteDuplicateFiles();
                break;
            case DuplicateFileHandlingMethod.Move:
                _logger.LogDebug("Performing move operation for duplicate files.");
                await MoveDuplicateFiles();
                break;
            case DuplicateFileHandlingMethod.NoAction:
                _logger.LogDebug("Performing no action for duplicate files.");
                break;
            default:
                _logger.LogError(
                    "Unexpected DuplicateFileHandlingMethod \"{DuplicateFileHandlingMethod}\" " +
                        "encountered while running DuplicateFileHandler task.",
                    _duplicateFileHandlingOptions.DuplicateFileHandlingMethod);
                throw new ArgumentOutOfRangeException("Unexpected DuplicateFileHandlingMethod " +
                    $"\"{_duplicateFileHandlingOptions.DuplicateFileHandlingMethod}\" " +
                    "encountered while running DuplicateFileHandler task.");
        }
    }
    
    private async Task DeleteDuplicateFiles()
    {
        var processingVerb = 
            _duplicateFileHandlingOptions.DuplicateFileHandlingMethod == DuplicateFileHandlingMethod.Delete
                ? "DELETE" : "MOVE";
        var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        
        var duplicateRecords = 
            await _fileFingerprintRepository.GetGroupingsWithDuplicateHashesAsync();

        var processedFilesCount = 0;
        long processedFilesSize = 0;
        
        foreach (var grouping in duplicateRecords)
        {
            _logger.LogDebug(
                "{ProcessingVerb} duplicate records with hash {GroupHash}.",
                textInfo.ToTitleCase(processingVerb),
                grouping.Key);
            var preservedFile = grouping.First();
            var filesToProcess = grouping.TakeLast(grouping.Count() - 1);
            foreach (var fingerprint in filesToProcess)
            {
                _logger.LogInformation(
                    "{ProcessingVerb} file '{DuplicateFile}'; duplicate of {PreservedFile}'.",
                    processingVerb,
                    fingerprint.FullPath,
                    preservedFile.FullPath);
                try
                {
                    _fileSystem.File.Delete(fingerprint.FullPath);
                    processedFilesCount++;
                    processedFilesSize += fingerprint.FileSize;
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                {
                    _logger.LogError(
                        "Unable to {ProcessingVerb} file '{FileFullPath}: {ExceptionMessage}'",
                        processingVerb,
                        fingerprint.FullPath,
                        e.Message);
                }
            }
        }
        
        _logger.LogInformation(
            "{ProcessingVerb}ed {DeletedFilesCount} files ({DeletedFilesSize} bytes).",
            processingVerb,
            processedFilesCount,
            processedFilesSize);
    }
    
    

    private async Task MoveDuplicateFiles()
    {
        var duplicateRecords =
            await _fileFingerprintRepository.GetGroupingsWithDuplicateHashesAsync();

        var movedFilesCount = 0;
        long movedFilesSize = 0;

        foreach (var grouping in duplicateRecords)
        {
            _logger.LogDebug();
        }
    }
}