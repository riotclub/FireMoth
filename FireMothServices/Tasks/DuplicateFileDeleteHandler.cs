// <copyright file="DuplicateFileDeleteHandler.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;

/// <summary>A task handler that performs file delete operations on files containing duplicate hash
/// values.</summary>
public class DuplicateFileDeleteHandler : ITaskHandler
{
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DuplicateFileMoveHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="DuplicateFileDeleteHandler"/> class.
    /// </summary>
    /// <param name="fileFingerprintRepository">An <see cref="IFileFingerprintRepository"/> used to
    /// retrieve duplicate records for handling and modify or delete records after handling.</param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> that provides file system I/O access.
    /// </param>
    /// <param name="logger">An <see cref="ILogger{DuplicateFileHandler}"/> to which logging output
    /// will be written.</param>
    public DuplicateFileDeleteHandler(
        IFileFingerprintRepository fileFingerprintRepository,
        IFileSystem fileSystem,
        ILogger<DuplicateFileMoveHandler> logger)
    {
        Guard.IsNotNull(fileFingerprintRepository);
        Guard.IsNotNull(fileSystem);
        Guard.IsNotNull(logger);
        _fileFingerprintRepository = fileFingerprintRepository;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    /// <summary>Runs the duplicate file handler task by deleting files with duplicate hashes in the
    /// <see cref="IFileFingerprintRepository"/>.</summary>
    public async Task RunTaskAsync()
    {
        var duplicateRecords = 
            await _fileFingerprintRepository.GetGroupingsWithDuplicateHashesAsync();

        var deletedFilesCount = 0;
        long deletedFilesSize = 0;
        
        foreach (var grouping in duplicateRecords)
        {
            _logger.LogDebug("Deleting duplicate records with hash {GroupHash}.", grouping.Key);
            var preservedFile = grouping.First();
            var filesToDelete = grouping.TakeLast(grouping.Count() - 1);
            foreach (var fingerprint in filesToDelete)
            {
                _logger.LogInformation(
                    "Deleting file '{DuplicateFile}'; duplicate of {PreservedFile}'.",
                    fingerprint.FullPath,
                    preservedFile.FullPath);
                try
                {
                    _fileSystem.File.Delete(fingerprint.FullPath);
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