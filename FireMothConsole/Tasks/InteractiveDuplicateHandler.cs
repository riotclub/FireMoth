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
    private readonly InteractiveDuplicateHandlerOptions _options;
    
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
    private const string DeleteConfirmation =
        "Are you sure you want to delete the following file(s)?";
    private const string DeletePrompt =
        "(D) delete files, (B) back: ";
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
    /// retrieve duplicate records and modify or delete records after promping the user.</param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> that provides file system I/O access.
    /// </param>
    /// <param name="options">An <see cref="IOptions{InteractiveDuplicateHandlerOptions}"/>
    /// containing options for this handler.</param>
    /// <param name="logger">An <see cref="ILogger{InteractiveDuplicateHandler}"/> to which logging
    /// output will be written.</param>
    public InteractiveDuplicateHandler(
        IFileFingerprintRepository fileFingerprintRepository,
        IFileSystem fileSystem,
        IOptions<InteractiveDuplicateHandlerOptions> options,
        ILogger<InteractiveDuplicateHandler> logger)
    {
        Guard.IsNotNull(fileFingerprintRepository);
        Guard.IsNotNull(fileSystem);
        Guard.IsNotNull(options.Value);
        Guard.IsNotNull(logger);
        _fileFingerprintRepository = fileFingerprintRepository;
        _fileSystem = fileSystem;
        _options = options.Value;
        _logger = logger;
    }
    
    /// <summary>Runs the interactive duplicate handler task by prompting the user for input
    /// instructing how to handle all duplicate files in the repository.</summary>
    public async Task RunTaskAsync()
    {
        
        // Retrieve duplicate groups from repository. Enumerate immediately via ToList, possibly
        // loading all duplicate elements into memory. Might need to perf test this. 
        var duplicateFileSets =
            (await _fileFingerprintRepository.GetGroupingsWithDuplicateHashesAsync()).ToList();
        
        // init groupIndex = 0
        var groupIndex = 0;

        // for each duplicate group = currentFileSet
        while (groupIndex < duplicateFileSets.Count)
        {
            // 	init groupComplete = false
            var groupComplete = false;

            var currentGrouping = duplicateFileSets.ElementAt(groupIndex);
            List<FileFingerprint>? currentFileSet;
            
            Console.WriteLine();
            // if group item count > MAX_ITEM_COUNT
            if (currentGrouping.Count() > MaxFilesPerGroup)
            {
                // display file list culled warning
                Console.WriteLine(FilesPerGroupLimitExceededNotice, MaxFilesPerGroup);

                // currentGroup = first MAX_ITEM_COUNT items from currentGroup
                currentFileSet = currentGrouping.Take(MaxFilesPerGroup).ToList();
            }
            else
            {
                currentFileSet = currentGrouping.ToList();
            }
            
            //
            // 	display current group file list
            //
            // 	display primary prompt
            // 	
            // 	while (!groupComplete)
            // 	
                // 		primaryInput <- read input key
                // 		
                // 		switch (primaryInput)
                // 		
                    // 			case (S)kip
                    // 			
                        // 				groupComplete = true
                    // 				
                    // 			case (O)pen all files
                    // 			
                        // 				OpenFiles
                    // 				
                    // 			case (D)elete selected
                    // 			
                        // 				display delete confirmation prompt
                        // 				
                        // 				init deleteOpComplete = false
                        // 				
                        // 				while (!deleteOpComplete)
                        //
                            // 					deleteConfirmInput <- read input key
                            //
                            // 					switch (deleteConfirmInput)
                            //
                            // 						case (D)elete
                            //
                            // 							delete selected files
                            //
                            // 							deleteOpComplete = true
                            //
                            // 							groupComplete = true
                            // 							
                            // 						case (B)ack
                            //
                            // 							deleteOpComplete = true
                            // 						
                            // 						case default   // bad input
                            //
                            // 							reset cursor position
                            // 							
                            // 					end switch (deleteConfirmInput)
                            // 					
                        // 				end while (!deleteOpComplete)
                    // 				
                    // 			case default
                    // 			
                    // 				if (primaryInput is integer)
                    // 				
                    // 					update selected items
                    // 					
                    // 				else   // bad input
                    // 				
                    // 					reset cursor position
                // 					
                // 		end switch (primaryInput)
                // 					
                // 		if (groupComplete)
                // 		
                // 			groupIndex++
                // 			
                // 		reset cursor position
            // 		
            // 	end while (!groupComplete)
        // 	
        // end for each duplicate group
		
		
        



            SetFilePathColumnWidth(duplicateFilesCulled.Select(fp => fp.FullPath));
            WriteHeader();
            var fileIndex = 1;
            foreach (var fileFingerprint in duplicateFilesCulled)
            {
                await Console.Out.WriteLineAsync(
                    FormatFileFingerprint(
                        fileIndex, fileFingerprint, SelectedFileIndexes[fileIndex - 1]));
                fileIndex++;
            }

            var userPrompt = PromptForOpInput(duplicateFileSet.ToList());
            Console.WriteLine();
        }
        
        // foreach (var duplicateFileSet in duplicateFileGroups)
        // {
        //     Console.WriteLine();
        //     if (duplicateFileSet.Count() > MaxFilesPerGroup)
        //     {
        //         Console.WriteLine("Maximum of {0} file comparisons in interactive mode; " +
        //                               "displaying first {0} files.",
        //                           MaxFilesPerGroup);
        //     }
        //
        //     var duplicateFilesCulled = duplicateFileSet.Take(MaxFilesPerGroup).ToList();
        //     SetFilePathColumnWidth(duplicateFilesCulled.Select(fp => fp.FullPath));
        //     WriteHeader();
        //     var fileIndex = 1;
        //     foreach (var fileFingerprint in duplicateFilesCulled)
        //     {
        //         await Console.Out.WriteLineAsync(
        //             FormatFileFingerprint(
        //                 fileIndex, fileFingerprint, SelectedFileIndexes[fileIndex - 1]));
        //         fileIndex++;
        //     }
        //
        //     PromptForOpInput(duplicateFileSet.ToList());
        //     Console.WriteLine();
        // }
        
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
    
    // Prompt the user for the action to take for the provided files.
    private bool PromptForOpInput(List<FileFingerprint> files)
    {
        Console.Write(PrimaryPrompt, files.Count);
        Array.Fill(SelectedFileIndexes, false, 0, files.Count);
        while (true)
        {
            var consoleKeyIn = Console.ReadKey().KeyChar;
            var isKeyDigit = int.TryParse(consoleKeyIn.ToString(), out var consoleKeyInInt);            
            switch (char.ToUpper(consoleKeyIn))
            {
                case 'D':
                    SaveCursorPosition();
                    if (HandleDeleteOp(files)) return true;
                    ResetCursor();
                    break;
                case 'O':
                    SaveCursorPosition();
                    HandleOpenOp(files);
                    ResetCursor();
                    break;
                case 'S':
                    return true;
                default:
                    SaveCursorPosition();
                    if (isKeyDigit &&
                        consoleKeyInInt > 0 &&
                        consoleKeyInInt <= files.Count)
                    {
                        ToggleFileSelection(consoleKeyInInt, files.Count);
                    }
                    ResetCursor();
                    break;
            }
        }
    }

    // Display a list of files provided and prompt the user to either (D)elete the listed files or
    // go (B)ack to the original selection prompt.
    private bool HandleDeleteOp(List<FileFingerprint> files)
    {
        if (!SelectedFileIndexes.Take(files.Count).Any(b => b)) 
            return false;
        
        Console.WriteLine($"\n\n{DeleteConfirmation}");
        foreach (var file in files)
        {
            if (SelectedFileIndexes[files.IndexOf(file)])
                Console.WriteLine($"\t{file.FullPath}");
        }
        Console.Write(DeletePrompt);
        
        var consoleKeyIn = Console.ReadKey().KeyChar;
        while (true)
        {
            switch (char.ToUpper(consoleKeyIn))
            {
                case 'D':
                    foreach (var file in files) DeleteFile(file);
                    return true;
                case 'B':
                    return false;
                default:
                    return false;
            }
        }
    }

    // Delete the specified file, performing necessary logging and exception handling.
    private void DeleteFile(FileFingerprint file)
    {
        _logger.LogInformation("Deleting file '{DuplicateFile}'.", file.FullPath);
        try
        {
            // _fileSystem.File.Delete(file.FullPath);
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
    private void HandleOpenOp(List<FileFingerprint> files)
    {
        foreach (var file in files)
        {
            ProcessStartInfo processStartInfo;
            if (string.IsNullOrWhiteSpace(_options.Application))
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
                    FileName = _options.Application,
                    ArgumentList = { file.FullPath },
                    UseShellExecute = false
                };

                if (_options.Arguments is not null &&
                    !string.IsNullOrWhiteSpace(_options.Arguments))
                {
                    var inQuotes = false;
                    var splitArguments = _options.Arguments.Split(c =>
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
    
    private void ResetCursor()
    {
        Console.SetCursorPosition(_cursorSaveX - 1, _cursorSaveY);
        Console.Write(' ');
        Console.SetCursorPosition(_cursorSaveX - 1, _cursorSaveY);
    }    
    
    private static string FormatFileFingerprint(
        int fileIndex, FileFingerprint fileFingerprint, bool selected)
    {
        var fileText =
            (selected ? SelectedFileChar : UnselectedFileChar) +
            $" {fileIndex,2}  " + 
            $"{GetFileString(fileFingerprint.FullPath).PadRight(_filePathColumnWidth)}  " +
            $"{"exact",MatchColumnWidth}  " +
            $"{DateTime.Now,-CreatedColumnWidth:s}  " +
            $"{DateTime.UtcNow,-ModifiedColumnWidth:s}  " +
            $"{ByteSize.FromBytes(fileFingerprint.FileSize).ToBinaryString(),SizeColumnWidth}";            
        return fileText;
    }
    
    private static string GetFileString(string filePath)
    {
        if (filePath.Length <= _filePathColumnWidth) return filePath;
        return "..." + Right(filePath, _filePathColumnWidth - 3);
    }
    
    private static string Right(string sValue, int iMaxLength)
    {
        if (string.IsNullOrEmpty(sValue))
            sValue = string.Empty;
        else if (sValue.Length > iMaxLength)
            sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);

        return sValue;
    }
    
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