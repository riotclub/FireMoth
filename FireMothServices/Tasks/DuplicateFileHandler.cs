// <copyright file="DuplicateFileHandler.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// A task handler that performs operations on files containing duplicate hash values.
/// </summary>
public class DuplicateFileHandler : ITaskHandler
{
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly DuplicateFileHandlingOptions _duplicateFileHandlingOptions;
    private readonly ILogger<DuplicateFileHandler> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileHandler"/> class.
    /// </summary>
    /// <param name="fileFingerprintRepository">An <see cref="IFileFingerprintRepository"/> used to
    /// retrieve duplicate records for handling and modify or delete records after handling.
    /// </param>
    /// <param name="duplicateFileHandlingOptions">An
    /// <see cref="IOptions{DuplicateFileHandlingOptions}"/> containing configured options for this
    /// task handler.</param>
    /// <param name="logger">An <see cref="ILogger{DuplicateFileHandler}"/> to which logging output
    /// will be written.</param>
    public DuplicateFileHandler(
        IFileFingerprintRepository fileFingerprintRepository,
        IOptions<DuplicateFileHandlingOptions> duplicateFileHandlingOptions,
        ILogger<DuplicateFileHandler> logger)
    {
        Guard.IsNotNull(fileFingerprintRepository);
        Guard.IsNotNull(logger);
        Guard.IsNotNull(duplicateFileHandlingOptions);
        _fileFingerprintRepository = fileFingerprintRepository;
        _duplicateFileHandlingOptions = duplicateFileHandlingOptions.Value;
        _logger = logger;
    }
    
    /// <summary>
    /// Runs the duplicate file handler task by either deleting or moving files with duplicate
    /// hashes in the <see cref="IFileFingerprintRepository"/> depending on the provided
    /// <see cref="_duplicateFileHandlingOptions"/>.
    /// </summary>
    /// <returns><c>true</c></returns>
    public async Task<bool> RunTaskAsync()
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
                throw new NotImplementedException(
                    "Move method for duplicate files not yet implemented.");
            case DuplicateFileHandlingMethod.NoAction:
                _logger.LogDebug("Performing no action for duplicate files.");
                break;
            default:
                _logger.LogError(
                    "Unexpected value {DuplicateFileHandlingMethod} encountered while running " +
                        "DuplicateFileHandler task.",
                    _duplicateFileHandlingOptions.DuplicateFileHandlingMethod);
                throw new ArgumentOutOfRangeException();
        }
        
        return true;
    }

    private async Task DeleteDuplicateFiles()
    {
        var duplicateRecords = 
            await _fileFingerprintRepository.GetGroupingsWithDuplicateHashesAsync();

        var deletedFilesCount = 0;
        long deletedFilesSize = 0;
        
        foreach (var grouping in duplicateRecords)
        {
            _logger.LogDebug("Deleting duplicate records with hash {GroupHash}.", grouping.Key);
            var preservedFile = grouping.First();
            var duplicatesToDelete = grouping.TakeLast(grouping.Count() - 1);
            foreach (var fingerprint in duplicatesToDelete)
            {
                _logger.LogInformation(
                    "Deleting file '{DeletingDuplicateFile}'; duplicate of '{DuplicateFile}'.",
                    fingerprint.FullPath,
                    preservedFile.FullPath);
                try
                {
                    File.Delete(fingerprint.FullPath);
                    deletedFilesCount++;
                    deletedFilesSize += fingerprint.FileSize;
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                {
                    _logger.LogError(
                        "Unable to delete file '{FileFullPath}: {ExceptionMessage}'",
                        fingerprint.FullPath,
                        e.Message);
                }
            }
        }
        
        _logger.LogInformation(
            "Deleted {DeletedFilesCount} files ({DeletedFilesSize} bytes).",
            deletedFilesCount,
            deletedFilesSize);
    }
}