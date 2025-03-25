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
            SetFilePathColumnWidth(duplicateFiles.Select(fp => fp.FullPath));
            WriteHeader();
            var fileIndex = 1;
            foreach (var fileFingerprint in duplicateFiles)
            {
                await Console.Out.WriteLineAsync(
                    FormatFileFingerprint(fileIndex++, fileFingerprint));
            }
        }
    }
    
    private static void SetFilePathColumnWidth(IEnumerable<string> fileNames)
    {
        // Set the available line width based on the current console width.
        var availableLineWidth = Console.WindowWidth switch
        {
            < MinLineWidth => MinLineWidth,
            > MaxLineWidth => MaxLineWidth,
            _ => Console.WindowWidth
        };

        // Given the available line width, determine the max length of the file path column.
        var filePathMaxWidth = availableLineWidth - CreatedWidth - ModifiedWidth - SizeWidth -
                               MatchWidth - 14;
        
        // Set file path column width depending on length of the file names in the grouping.
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
        // If the full path will fit, use it.
        // "/DirectoryA/SubDirectory/ParentDirectory/File.ext"
        if (filePath.Length <= _filePathColumnWidth)
            return filePath;

        return "..." + Right(filePath, _filePathColumnWidth - 3);
        
        // // If the full path will not fit, try to at least get the root directory, parent
        // // subdirectory, and filename.
        // // "/.../ParentDirectory/File.ext"
        // var parentDirectory = Directory.GetParent(filePath) is null 
        //     ? string.Empty
        //     : Directory.GetParent(filePath)!.Name + Path.DirectorySeparatorChar;
        //
        // var filePathString = Path.GetPathRoot(filePath) + "..." + Path.DirectorySeparatorChar +
        //                      parentDirectory + Path.GetFileName(filePath);
        // if (filePathString.Length <= _filePathColumnWidth)
        //     return filePathString;
        //
        // // If that doesn't fit, try the parent subdirectory and filename.
        // // "ParentDirectory/File.ext"
        // filePathString = parentDirectory.Equals(string.Empty)
        //     ? Path.GetPathRoot(filePath)
        //     : parentDirectory;
        // filePathString += Path.GetFileName(filePath);
        // if (filePathString.Length <= _filePathColumnWidth)
        //     return filePathString;
        //
        // // Finally, if none of the above fit, try just the filename.
        // // "File.ext"
        // if (Path.GetFileName(filePath).Length <= _filePathColumnWidth)
        //     return Path.GetFileName(filePath);
        //
        // // And if THAT won't fit, then use as many characters of the filename will fit.
        // // "...allyLongFileName.ext"
        // filePathString = "..." + Right(filePathString, _filePathColumnWidth - 3);
        // return filePathString;
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