// <copyright file="InteractiveDuplicateHandler.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console.Tasks;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RiotClub.FireMoth.Console.Extensions;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Services.Tasks;

/// <summary>A task handler that performs interactive duplicate file handling for moving or deleting
/// files.</summary>
public class InteractiveDuplicateHandler : ITaskHandler
{
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<InteractiveDuplicateHandler> _logger;
    private readonly InteractiveDuplicateHandlerOptions _interactiveDuplicateHandlerOptions;
    private readonly DuplicateFileHandlingOptions _duplicateFileHandlingOptions;
    
    private const int CreatedColumnWidth = 19;
    private const int ModifiedColumnWidth = 19;
    private const int SizeColumnWidth = 10;
    private const int MatchColumnWidth = 5;
    private const int MinLineWidth = 80;
    private const int MaxLineWidth = 200;
    private const int MaxFilesPerGroup = 9;
    private const int TotalInterColumnSpacing = 14;
    private const char SelectedFileChar = '*';
    private const char UnselectedFileChar = ' ';
    private const string PrimaryPrompt =
        "(O) open all files, (1-{0}) select/unselect file, (D) delete selected, (S) skip: ";
    private const char PrimaryPromptOpenAllOpChar =        'O';
    private const char PrimaryPromptDeleteSelectedOpChar = 'D';
    private const char PrimaryPromptSkipOpChar =           'S';
    private const string DeleteConfirmationNotice =
        "Are you sure you want to delete the following file(s)?";
    private const string DeleteConfirmationPrompt =
        "(D) delete files, (C) cancel: ";
    private const char DeleteConfirmationPromptDeleteOpChar = 'D';
    private const char DeleteConfirmationPromptCancelOpChar = 'C';
    
    private const string FilesPerGroupLimitExceededNotice =
        "Maximum of {0} file comparisons in interactive mode; displaying first {0} files.";
    
    private static readonly bool[] SelectedFileIndexes = new bool[MaxFilesPerGroup];
    private static int _filePathColumnWidth;
    private int _cursorSaveX;
    private int _cursorSaveY;
    private int _deletedFilesCount;
    private long _deletedFilesSize;
    
    /// <summary>Initializes a new instance of the <see cref="InteractiveDuplicateHandler"/> class.
    /// </summary>
    /// <param name="fileFingerprintRepository">An <see cref="IFileFingerprintRepository"/> used to
    /// retrieve duplicate records and modify or delete records after prompting the user.</param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> that provides file system I/O access.
    /// </param>
    /// <param name="interactiveDuplicateHandlerOptions">An <see cref="IOptions{InteractiveDuplicateHandlerOptions}"/>
    /// containing options for this handler.</param>
    /// <param name="logger">An <see cref="ILogger{InteractiveDuplicateHandler}"/> to which logging
    /// output will be written.</param>
    public InteractiveDuplicateHandler(
        IFileFingerprintRepository fileFingerprintRepository,
        IFileSystem fileSystem,
        IOptions<InteractiveDuplicateHandlerOptions> interactiveDuplicateHandlerOptions,
        IOptions<DuplicateFileHandlingOptions> duplicateFileHandlingOptions,
        ILogger<InteractiveDuplicateHandler> logger)
    {
        Guard.IsNotNull(fileFingerprintRepository);
        Guard.IsNotNull(fileSystem);
        Guard.IsNotNull(interactiveDuplicateHandlerOptions.Value);
        Guard.IsNotNull(duplicateFileHandlingOptions.Value);
        Guard.IsNotNull(logger);
        _fileFingerprintRepository = fileFingerprintRepository;
        _fileSystem = fileSystem;
        _interactiveDuplicateHandlerOptions = interactiveDuplicateHandlerOptions.Value;
        _duplicateFileHandlingOptions = duplicateFileHandlingOptions.Value;
        _logger = logger;
    }
    
    /// <summary>Runs the interactive duplicate handler task by prompting the user for input
    /// instructing how to handle all duplicate files in the repository.</summary>
    public async Task RunTaskAsync()
    {
        var duplicateFileSets =
            (await _fileFingerprintRepository.GetGroupingsWithDuplicateHashesAsync()).ToList();
        
        var groupIndex = 0;
        while (groupIndex < duplicateFileSets.Count)
        {
            var currentGrouping = duplicateFileSets.ElementAt(groupIndex);
            List<FileFingerprint> currentFileSet;
            Console.WriteLine();
            if (currentGrouping.Count() > MaxFilesPerGroup)
            {
                Console.WriteLine(FilesPerGroupLimitExceededNotice, MaxFilesPerGroup);
                currentFileSet = currentGrouping.Take(MaxFilesPerGroup).ToList();
            }
            else
            {
                currentFileSet = currentGrouping.ToList();
            }
            
            Array.Fill(SelectedFileIndexes, false, 0, currentFileSet.Count);
            var groupComplete = false;

            DisplayFileList(currentFileSet);
            Console.Write(PrimaryPrompt, currentFileSet.Count);
            
            while (!groupComplete)
            {
                var primaryPromptInput = Console.ReadKey().KeyChar;
                var isInputNumeric = int.TryParse(
                    primaryPromptInput.ToString(), out var primaryPromptInputInt);            
                var deleteOpBackSelected = false;
                
                switch (char.ToUpper(primaryPromptInput))
                {
                    case PrimaryPromptSkipOpChar:
                        Console.WriteLine();
                        groupComplete = true;
                        break;
                    case PrimaryPromptOpenAllOpChar:
                        SaveCursorPosition();
                        OpenWithSystemApplication(currentFileSet);
                        ResetCursor();
                        break;
                    case PrimaryPromptDeleteSelectedOpChar:
                        SaveCursorPosition();
                        if (!SelectedFileIndexes.Take(currentFileSet.Count).Any(index => index))
                        {
                            ResetCursor();
                            break;
                        }
                        
                        DisplayDeleteConfirmation(currentFileSet);
                        var deleteOpComplete = false;

                        while (!deleteOpComplete)
                        {
                            var deleteConfirmInput = Console.ReadKey().KeyChar;
                            SaveCursorPosition();
                            switch (char.ToUpper(deleteConfirmInput))
                            {
                                case DeleteConfirmationPromptDeleteOpChar:
                                    Console.WriteLine();
                                    foreach (var file in currentFileSet)
                                    {
                                        if (SelectedFileIndexes[currentFileSet.IndexOf(file)])
                                            DeleteFile(file);
                                    }
                                    
                                    deleteOpComplete = true;
                                    groupComplete = true;
                                    break;
                                case DeleteConfirmationPromptCancelOpChar:
                                    Console.WriteLine();
                                    groupComplete = true;
                                    deleteOpComplete = true;
                                    deleteOpBackSelected = true;
                                    break;
                                default:
                                    ResetCursor();
                                    break;
                            }
                        }
                        break;
                    default:
                        SaveCursorPosition();
                        if (isInputNumeric &&
                            primaryPromptInputInt > 0 &&
                            primaryPromptInputInt <= currentFileSet.Count)
                        {
                            ToggleFileSelection(primaryPromptInputInt, currentFileSet.Count);
                        }
                        ResetCursor();
                        break;
                }

                if (groupComplete && !deleteOpBackSelected)
                    groupIndex++;
            }
        }
        
        var deletedFilesSizeHumanReadable =
            ByteSize.FromBytes(_deletedFilesSize).ToBinaryString();
        _logger.LogInformation(
            "Deleted {DeletedFilesCount} files, {DeletedFilesSizeBytes} bytes " +
            "({DeletedFilesSizeHumanReadable}).",
            _deletedFilesCount,
            _deletedFilesSize,
            deletedFilesSizeHumanReadable);
    }

    // Toggle the SelectedFileIndex flag for the provided file index and update the console display
    // to indicate the new flag status.
    private static void ToggleFileSelection(int fileIndex, int numberOfFiles)
    {
        SelectedFileIndexes[fileIndex - 1] = !SelectedFileIndexes[fileIndex - 1];
        var (_, cursorOriginY) = Console.GetCursorPosition();
        var selectedCursorY = cursorOriginY - (numberOfFiles - fileIndex + 1);
        
        Console.SetCursorPosition(0, selectedCursorY);
        Console.Write(SelectedFileIndexes[fileIndex - 1] ? SelectedFileChar : UnselectedFileChar);
    }

    // Delete the specified file, performing necessary logging and exception handling.
    private void DeleteFile(FileFingerprint file)
    {
        _logger.LogInformation("Deleting file '{DuplicateFile}'.", file.FullPath);
        try
        {
            _fileSystem.File.Delete(file.FullPath);
            _deletedFilesCount++;
            _deletedFilesSize += file.FileSize;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(
                "Unable to delete file '{FileFullPath}: {ExceptionMessage}'",
                file.FullPath,
                e.Message);
        }
    }
    
    // Attempt to open the provided files using either the platform default application or, if
    // specified in _options, the specified application.
    private void OpenWithSystemApplication(List<FileFingerprint> files)
    {
        foreach (var file in files)
        {
            ProcessStartInfo processStartInfo;
            if (string.IsNullOrWhiteSpace(_interactiveDuplicateHandlerOptions.Application))
            {
                processStartInfo = new ProcessStartInfo
                {
                    FileName = file.FullPath,
                    UseShellExecute = true
                };
            }
            else
            {
                processStartInfo = new ProcessStartInfo
                {
                    FileName = _interactiveDuplicateHandlerOptions.Application,
                    ArgumentList = { file.FullPath },
                    UseShellExecute = false
                };

                if (_interactiveDuplicateHandlerOptions.Arguments is not null &&
                    !string.IsNullOrWhiteSpace(_interactiveDuplicateHandlerOptions.Arguments))
                {
                    var inQuotes = false;
                    var splitArguments = _interactiveDuplicateHandlerOptions.Arguments.Split(c =>
                        {
                            if (c == '\"') inQuotes = !inQuotes;
                            return !inQuotes && c == ' ';
                        })
                        .Select(str => str.Trim().TrimMatchingQuotes('\"'))
                        .Where(str => !string.IsNullOrEmpty(str));
                        
                    foreach (var argument in splitArguments)
                        processStartInfo.ArgumentList.Add(argument);
                }
            }
            Process.Start(processStartInfo);
        }
    }
    
    // Sets the allowed width of the filepath column in a FileFingerprint information string based
    // on the current width of the console.
    private static void SetFilePathColumnWidth(IEnumerable<string> fileNames)
    {
        var availableLineWidth = Console.WindowWidth switch
        {
            < MinLineWidth => MinLineWidth,
            > MaxLineWidth => MaxLineWidth,
            _ => Console.WindowWidth
        };

        var filePathMaxWidth = availableLineWidth - CreatedColumnWidth - ModifiedColumnWidth -
                               SizeColumnWidth - MatchColumnWidth - TotalInterColumnSpacing;
        var fileNamesMaxLength = fileNames.Select(fileName => fileName.Length).Max();
        _filePathColumnWidth = fileNamesMaxLength > filePathMaxWidth
            ? filePathMaxWidth
            : fileNamesMaxLength;
    }

    private void SaveCursorPosition() =>
        (_cursorSaveX, _cursorSaveY) = Console.GetCursorPosition();
    
    // Resets the currently focused console character by overwriting it with whitespace. 
    private void ResetCursor()
    {
        Console.SetCursorPosition(_cursorSaveX - 1, _cursorSaveY);
        Console.Write(' ');
        Console.SetCursorPosition(_cursorSaveX - 1, _cursorSaveY);
    }    
    
    // Returns a string containing formatted data from a FileFingerprint.
    private static string FormatFileFingerprint(
        int fileIndex, FileFingerprint fileFingerprint, bool selected)
    {
        var fileText =
            (selected ? SelectedFileChar : UnselectedFileChar) +
            $" {fileIndex,2}  " + 
            $"{GetFileString(fileFingerprint).PadRight(_filePathColumnWidth)}  " +
            $"{"exact",MatchColumnWidth}  " +
            $"{DateTime.Now,-CreatedColumnWidth:s}  " +
            $"{DateTime.UtcNow,-ModifiedColumnWidth:s}  " +
            $"{ByteSize.FromBytes(fileFingerprint.FileSize).ToBinaryString(),SizeColumnWidth}";            
        return fileText;
    }
    
    // Returns a formatted file path string from a FileFingerprint, abbreviated if it won't fit
    // within the current _filePathColumnWidth. 
    private static string GetFileString(FileFingerprint filePath)
    {
        if (filePath.FullPath.Length <= _filePathColumnWidth) return filePath.FullPath;
        return "..." + Right(filePath.FullPath, _filePathColumnWidth - 3);
    }
 
    // Outputs a formatted list of the provided collection of FileFingerprints to the console.
    private static void DisplayFileList(List<FileFingerprint> fileSet)
    {
        SetFilePathColumnWidth(fileSet.Select(fp => fp.FullPath));
        WriteHeader();
        var fileIndex = 1;
        foreach (var fileFingerprint in fileSet)
        {
            Console.Out.WriteLine(FormatFileFingerprint(
                fileIndex, fileFingerprint, SelectedFileIndexes[fileIndex - 1]));
            fileIndex++;
        }
    }

    // Outputs a delete confirmation prompt for the provided collection of FileFingeprints.
    private static void DisplayDeleteConfirmation(List<FileFingerprint> fileSet)
    {
        Console.WriteLine($"\n\n{DeleteConfirmationNotice}");
        foreach (var file in fileSet)
        {
            if (SelectedFileIndexes[fileSet.IndexOf(file)])
                Console.WriteLine($"\t{file.FullPath}");
        }
        Console.Write(DeleteConfirmationPrompt);
    }
    
    // Given a string return a new string containing the rightmost iMaxLength characters.
    private static string Right(string sValue, int iMaxLength)
    {
        if (string.IsNullOrEmpty(sValue))
            sValue = string.Empty;
        else if (sValue.Length > iMaxLength)
            sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);

        return sValue;
    }
    
    // Output file set header text to console.
    private static void WriteHeader()
    {
        var headerText =
            "*  #  " +
            $"{"File".PadRight(_filePathColumnWidth)}  " +
            $"{"Match",MatchColumnWidth}  " +
            $"{"Created",-CreatedColumnWidth}  " +
            $"{"Modified",-ModifiedColumnWidth}  " +
            $"{"Size",SizeColumnWidth}";
        Console.Out.WriteLine(headerText);
    }
}