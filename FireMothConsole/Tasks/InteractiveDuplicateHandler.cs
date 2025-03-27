// <copyright file="InteractiveDuplicateHandler.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console.Tasks;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Services.Tasks;

/// <summary>A task handler that performs interactive duplicate file handling for moving or deleting
/// files.</summary>
public class InteractiveDuplicateHandler : ITaskHandler
{
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<InteractiveDuplicateHandler> _logger;

    private const int CreatedWidth = 19;
    private const int ModifiedWidth = 19;
    private const int SizeWidth = 10;
    private const int MatchWidth = 5;

    private const int MinLineWidth = 80;
    private const int MaxLineWidth = 200;
    private const int MaxFilesPerGroup = 9;

    private const int TotalInterColumnSpacing = 14;
    
    private static int _filePathColumnWidth;
    
    /// <summary>Initializes a new instance of the <see cref="InteractiveDuplicateHandler"/> class.
    /// </summary>
    /// <param name="fileFingerprintRepository">An <see cref="IFileFingerprintRepository"/> used to
    /// retrieve duplicate records and modify or delete records after promping the user.</param>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> that provides file system I/O access.
    /// </param>
    /// <param name="logger">An <see cref="ILogger{InteractiveDuplicateHandler}"/> to which logging
    /// output will be written.</param>
    public InteractiveDuplicateHandler(
        IFileFingerprintRepository fileFingerprintRepository,
        IFileSystem fileSystem,
        ILogger<InteractiveDuplicateHandler> logger)
    {
        Guard.IsNotNull(fileFingerprintRepository);
        Guard.IsNotNull(fileSystem);
        Guard.IsNotNull(logger);
        _fileFingerprintRepository = fileFingerprintRepository;
        _fileSystem = fileSystem;
        _logger = logger;
    }
    
    /// <summary>Runs the interactive duplicate handler task by prompting the user for input
    /// instructing how to handle all duplicate files in the repository.</summary>
    public async Task RunTaskAsync()
    {
        var duplicateFileGroupings =
            (await _fileFingerprintRepository.GetGroupingsWithDuplicateHashesAsync()).ToList();

        foreach (var duplicateFiles in duplicateFileGroupings)
        {
            Console.WriteLine();
            if (duplicateFiles.Count() > MaxFilesPerGroup)
            {
                Console.WriteLine("Maximum of {0} file comparisons in interactive mode; " +
                                      "displaying first {0} files.",
                                  MaxFilesPerGroup);
            }

            var duplicateFilesCulled = duplicateFiles.Take(MaxFilesPerGroup).ToList();
            SetFilePathColumnWidth(duplicateFilesCulled.Select(fp => fp.FullPath));
            WriteHeader();
            var fileIndex = 1;
            foreach (var fileFingerprint in duplicateFilesCulled)
            {
                await Console.Out.WriteLineAsync(
                    FormatFileFingerprint(fileIndex++, fileFingerprint));
            }

            PromptForOpInput(duplicateFiles.Count());
        }
    }

    private static void ToggleFileSelection(int fileIndex, int numberOfFiles)
    {
        var (cursorX, cursorY) = Console.GetCursorPosition();
        var consoleHeight = Console.WindowHeight;
        var consoleWidth = Console.WindowWidth;
        
        Console.SetCursorPosition(0, consoleHeight - 2);
        Console.CursorLeft = 1;
        Console.CursorTop = numberOfFiles - fileIndex;

    }
    
    private static void PromptForOpInput(int numberOfFiles)
    {
        Console.Out.Write(
            $"(O) open files, (1-{numberOfFiles}) select/unselect file, (D) delete selected, (S) skip: ");
        var consoleKeyIn = Console.ReadKey().KeyChar;
        var isKeyDigit = int.TryParse(consoleKeyIn.ToString(), out var consoleKeyInInt);
        switch (char.ToUpper(consoleKeyIn))
        {
            case 'D':
                break;
            case 'O':
                break;
            case 'S':
                break;
            default:
                if (isKeyDigit && consoleKeyInInt is > 0 and <= MaxFilesPerGroup)
                {
                    ToggleFileSelection(consoleKeyInInt, numberOfFiles);
                }

                break;
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

        var filePathMaxWidth = availableLineWidth - CreatedWidth - ModifiedWidth - SizeWidth -
                               MatchWidth - TotalInterColumnSpacing;
        var fileNamesMaxLength = fileNames.Select(fileName => fileName.Length).Max();
        _filePathColumnWidth = fileNamesMaxLength > filePathMaxWidth
            ? filePathMaxWidth
            : fileNamesMaxLength;
    }
    
    private static string FormatFileFingerprint(int fileIndex, FileFingerprint fileFingerprint)
    {
        var fileText =
            $"  {fileIndex,2}  " + 
            $"{GetFileString(fileFingerprint.FullPath).PadRight(_filePathColumnWidth)}  " +
            $"{"exact",MatchWidth}  " +
            $"{DateTime.Now,-CreatedWidth:s}  " +
            $"{DateTime.UtcNow,-ModifiedWidth:s}  " +
            $"{ByteSize.FromBytes(fileFingerprint.FileSize).ToBinaryString(),SizeWidth}";            

        return fileText;
    }
    
    private static string GetFileString(string filePath)
    {
        if (filePath.Length <= _filePathColumnWidth)
            return filePath;

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
            $"{"Match",MatchWidth}  " +
            $"{"Created",-CreatedWidth}  " +
            $"{"Modified",-ModifiedWidth}  " +
            $"{"Size",SizeWidth}";
        Console.Out.WriteLine(headerText);
    }
}