// <copyright file="DuplicateFileMoveHandler.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RiotClub.FireMoth.Services.Repository;

/// <summary>A task handler that performs file move operations on files containing duplicate hash
/// values.</summary>
public class DuplicateFileMoveHandler : ITaskHandler
{
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly DuplicateFileHandlingOptions _duplicateFileHandlingOptions;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DuplicateFileMoveHandler> _logger;

    /// <summary>Initializes a new instance of the <see cref="DuplicateFileMoveHandler"/> class.
    /// </summary>
    /// <param name="fileFingerprintRepository">An <see cref="IFileFingerprintRepository"/> used to
    /// retrieve duplicate records for processing.</param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> that provides file system I/O access.
    /// </param>
    /// <param name="duplicateFileHandlingOptions">An
    /// <see cref="IOptions{DuplicateFileHandlingOptions}"/> containing options specifying where
    /// to move duplicate files.</param>
    /// <param name="logger">An <see cref="ILogger{DuplicateFileHandler}"/> to which logging output
    /// will be written.</param>
    public DuplicateFileMoveHandler(
        IFileFingerprintRepository fileFingerprintRepository,
        IFileSystem fileSystem,
        IOptions<DuplicateFileHandlingOptions> duplicateFileHandlingOptions,
        ILogger<DuplicateFileMoveHandler> logger)
    {
        Guard.IsNotNull(fileFingerprintRepository);        
        Guard.IsNotNull(fileSystem);
        Guard.IsNotNull(duplicateFileHandlingOptions?.Value);
        Guard.IsNotNull(logger);
        _fileFingerprintRepository = fileFingerprintRepository;
        _fileSystem = fileSystem;
        _duplicateFileHandlingOptions = duplicateFileHandlingOptions.Value;
        _logger = logger;
    }

    /// <summary>Runs this task handler by moving files with duplicate hashes to the location
    /// specified in the <see cref="DuplicateFileHandlingOptions"/>.</summary>
    public async Task RunTaskAsync()
    {
        if (_duplicateFileHandlingOptions.DuplicateFileHandlingMethod !=
            DuplicateFileHandlingMethod.Move)
        {
            return;
        }

        if (!ValidateAndCreateMoveDirectory())
            return;
        
        var duplicateRecords = 
            await _fileFingerprintRepository.GetGroupingsWithDuplicateHashesAsync();

        var movedFilesCount = 0;
        long movedFilesSize = 0;
        var destinationDirectory = _fileSystem.DirectoryInfo.New(
            _duplicateFileHandlingOptions.MoveDuplicateFilesToDirectory!);
        
        foreach (var grouping in duplicateRecords)
        {
            _logger.LogDebug("Moving duplicate records with hash {GroupHash}.", grouping.Key);
            var preservedFile = grouping.First();
            var filesToMove = grouping.TakeLast(grouping.Count() - 1);
            foreach (var fingerprint in filesToMove)
            {
                _logger.LogInformation(
                    "Moving file '{DuplicateFile}'; duplicate of {PreservedFile}'.",
                    fingerprint.FullPath,
                    preservedFile.FullPath);
                try                   

                {
                    var destinationFullPath = GetUniqueFileName(
                        destinationDirectory.FullName,
                        _fileSystem.Path.GetFileName(fingerprint.FullPath));
                    _fileSystem.File.Move(fingerprint.FullPath, destinationFullPath);
                    movedFilesCount++;
                    movedFilesSize += fingerprint.FileSize;
                }
                catch (Exception ex) when (ex is IOException or
                                                 ArgumentException or
                                                 UnauthorizedAccessException or
                                                 NotSupportedException)
                {
                    _logger.LogWarning(
                        "Unable to move file '{FileFullPath}': {ExceptionMessage}",
                        fingerprint.FullPath,
                        ex.Message);
                }
            }
        }
        
        _logger.LogInformation(
            "Moved {MovedFilesCount} duplicate files in total ({MovedFilesSize} bytes).",
            movedFilesCount,
            movedFilesSize);
    }
    
    // Validate and create move directory
    private bool ValidateAndCreateMoveDirectory()
    {
        // Verify duplicate file directory is valid
        IDirectoryInfo? moveToDirectory;
        try
        {
            moveToDirectory = _fileSystem.DirectoryInfo.New(
                _duplicateFileHandlingOptions.MoveDuplicateFilesToDirectory!);
        }
        catch (Exception ex) when (ex is ArgumentNullException or
                                       ArgumentException or
                                       SecurityException or
                                       PathTooLongException)
        {
            _logger.LogError(
                "Duplicate file directory '{MoveDuplicateFilesToDirectory}' is invalid: " +
                "{ExceptionMessage}",
                _duplicateFileHandlingOptions.MoveDuplicateFilesToDirectory,
                ex.Message);
            return false;
        }

        // Create directory if it does not already exist
        if (moveToDirectory.Exists)
            return true;

        try
        {
            moveToDirectory.Create();
        }
        catch (Exception ex) when (ex is IOException or
                                         UnauthorizedAccessException or
                                         ArgumentException or
                                         PathTooLongException or
                                         DirectoryNotFoundException or
                                         NotSupportedException)
        {
            _logger.LogError(
                "Unable to create duplicate file directory '{MoveDuplicateFilesToDirectory}':" +
                " {ExceptionMessage}",
                _duplicateFileHandlingOptions.MoveDuplicateFilesToDirectory,
                ex.Message);
            return false;
        }

        return true;
    }
    
    // Given a directory and filename, determines if the file already exists, and if so, adds an
    // integer value to the filename and checks again until a unique filename is found and returned.
    private string GetUniqueFileName(string directory, string originalFileName)
    {
        var destinationPath = _fileSystem.Path.Combine(directory, originalFileName);
        var index = 1;

        while (_fileSystem.File.Exists(destinationPath))
        {
            var newFileName = _fileSystem.Path.GetFileNameWithoutExtension(originalFileName) +
                              $"_({index++})" +
                              _fileSystem.Path.GetExtension(originalFileName);
            destinationPath = _fileSystem.Path.Combine(directory, newFileName);
        }
        
        return destinationPath;
    }
}