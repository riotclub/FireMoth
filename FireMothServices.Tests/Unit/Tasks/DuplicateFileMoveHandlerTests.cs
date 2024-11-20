// <copyright file="DuplicateFileMoveHandlerTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.Tasks;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Services.Orchestration;
using Xunit;
using Moq;
using Moq.AutoMock;
using Repository;
using RiotClub.FireMoth.Services.Tests.Unit.Extensions;
using Services.DataAnalysis;
using Services.Tasks;
using RiotClub.FireMoth.Services.Tests.Unit.Helpers;

/// <summary>
/// <p>
/// Ctor<br/>
/// - Passing [IFileFingerprintRepository:null] throws ArgumentNullException.<br/>
/// - Passing [IFileSystem:null] throws ArgumentNullException.<br/>
/// - Passing [IOptions{DuplicateFileHandlingOptions}:null] throws ArgumentNullException.<br/>
/// - Passing [ILogger{DuplicateFileHandler}:null] throws ArgumentNullException.<br/>
/// - Passing [valid parameters] properly initializes an object.<br/>
/// </p>
/// <p>
/// RunTaskAsync<br/>
/// - If option DuplicateFileHandlingOptions.DuplicateFileHandlingMethod is not
///   DuplicateFileHandlingMethod.Move, return without moving files.<br/>
/// - If option DuplicateFileHandlingOptions.MoveDuplicateFilesToDirectory is not a valid directory,
///   return without moving files.<br/>
/// - If invoked with option DuplicateFileHandlingOptions.Move and
///   DuplicateFileHandlingOptions.MoveDuplicateFilesToDirectory is a valid directory that already
///   exists, duplicate files in the repository are moved to the specified directory.<br/>
/// - If invoked with option DuplicateFileHandlingOptions.Move and
///   DuplicateFileHandlingOptions.MoveDuplicateFilesToDirectory is a valid directory that does not
///   exist, the directory is created and duplicate files in the repository are moved to the
///   specified directory.<br/>  
/// - If DuplicateFileHandlingOptions.MoveDuplicateFilesToDirectory contains files with the same
///   name as duplicate files being moved, the duplicate files are given unique names before being
///   moved.<br/>
/// - Relevant messages are logged.<br/>
/// </p>
/// </summary>
public class DuplicateFileMoveHandlerTests
{
    private readonly ILogger<DuplicateFileMoveHandler> _nullLogger =
        NullLogger<DuplicateFileMoveHandler>.Instance;
    private readonly Mock<IFileFingerprintRepository> _mockRepository = new();
    private readonly Mock<IOptions<DuplicateFileHandlingOptions>> _mockOptions = new();
    private readonly MockFileSystem _mockFileSystem = FileSystemTestHelpers.BuildMockFileSystem();
    
    private readonly AutoMocker _mocker = new();

    public DuplicateFileMoveHandlerTests()
    {
        _mockOptions
            .Setup(options => options.Value)
            .Returns(BuildDuplicateFileHandlingOptions());
    }
    
    /// <summary>Ctor: Passing [IFileFingerprintRepository:null] throws ArgumentNullException.
    /// </summary> 
    [Fact]
    public void Ctor_NullIFileFingerprintRepository_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var ctorAction = () =>
            new DuplicateFileMoveHandler(null!, _mockFileSystem, _mockOptions.Object, _nullLogger);
        
        // Assert
        ctorAction.Should().ThrowExactly<ArgumentNullException>();
    }
    
    /// <summary>Ctor: Passing [IFileSystem:null] throws ArgumentNullException.</summary>
    [Fact]
    public void Ctor_NullIFileSystem_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var ctorAction = () =>
            new DuplicateFileMoveHandler(
                _mockRepository.Object, null!, _mockOptions.Object, _nullLogger);
        
        // Assert
        ctorAction.Should().ThrowExactly<ArgumentNullException>();
    }    
    
    /// <summary>Ctor: Passing [IOptions{DuplicateFileHandlingOptions}:null] throws
    /// ArgumentNullException.</summary>
    [Fact]
    public void Ctor_NullIOptions_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var ctorAction = () =>
            new DuplicateFileMoveHandler(
                _mockRepository.Object, _mockFileSystem, null!, _nullLogger);
        
        // Assert
        ctorAction.Should().ThrowExactly<ArgumentNullException>();
    }       

    /// <summary>Ctor: Passing [ILogger{DuplicateFileHandler}:null] throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Ctor_NullILogger_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var ctorAction = () =>
            new DuplicateFileMoveHandler(
                _mockRepository.Object, _mockFileSystem, _mockOptions.Object, null!);
        
        // Assert
        ctorAction.Should().ThrowExactly<ArgumentNullException>();
    }   
    
    /// <summary>Ctor: Passing [valid parameters] properly initializes an object.</summary>
    [Fact]
    public void Ctor_ValidParameters_CreatesNewObject()
    {
        // Arrange, Act
        var result = new DuplicateFileMoveHandler(
            _mockRepository.Object, _mockFileSystem, _mockOptions.Object, _nullLogger);
        
        // Assert
        result.Should().NotBeNull();
    }   
    
    /// RunTaskAsync<br/>

    /// - Relevant messages are logged.<br/>
    
    /// <summary>RunTaskAsync: If option DuplicateFileHandlingOptions.DuplicateFileHandlingMethod is
    /// not DuplicateFileHandlingMethod.Move, exit without moving files.</summary>
    [Fact]
    public async Task RunTaskAsync_MethodNotMove_ReturnsWithoutMovingFiles()
    {
        // Arrange
        _mockOptions
            .Setup(options => options.Value)
            .Returns(BuildDuplicateFileHandlingOptions(
                duplicateFileHandlingMethod: DuplicateFileHandlingMethod.NoAction));
        var sut = new DuplicateFileMoveHandler(
            _mockRepository.Object, _mockFileSystem, _mockOptions.Object, _nullLogger);
        var expected = _mockFileSystem.AllFiles;
        
        // Act
        await sut.RunTaskAsync();
        var result = _mockFileSystem.AllFiles;
        
        // Assert
        result.Should().BeEquivalentTo(expected);
    }
    
    /// <summary>RunTaskAsync: If invoked with option DuplicateFileHandlingOptions.Move and
    /// DuplicateFileHandlingOptions.MoveDuplicateFilesToDirectory is a valid directory, duplicate
    /// files in the repository are moved to the specified directory.</summary>
    [Theory]
    [InlineData("/emptydir")]
    [InlineData("/newdir")]
    public async Task RunTaskAsync_MoveDirectoryExists_MovesDuplicateFiles(string moveDirectory)
    {
        // Arrange
        var allFiles = _mockFileSystem.AllFiles.ToList();
        string[] duplicateFiles = [ allFiles[0], allFiles[3], allFiles[7] ];
        var expectedFilesAfterMove = _mockFileSystem.AllFiles;
        for (var index = 1; index < duplicateFiles.Length; index++)
        {
            var currentFile = duplicateFiles[index];
            expectedFilesAfterMove = expectedFilesAfterMove.Where(
                fileName => fileName != currentFile);
            expectedFilesAfterMove = expectedFilesAfterMove.Append(
                Path.Combine(moveDirectory, Path.GetFileName(currentFile)));
        }
        
        var duplicateFileFingerprints = 
            duplicateFiles
                .Select(duplicateFile =>
                    new FileFingerprint(
                        Path.GetFileName(duplicateFile),
                        Path.GetDirectoryName(duplicateFile)!,
                        1,
                        "000="))
                .ToList();
        _mockRepository
            .Setup(mockRepo => mockRepo.GetGroupingsWithDuplicateHashesAsync())
            .ReturnsAsync(
                duplicateFileFingerprints.GroupBy(fileFingerprint => fileFingerprint.Base64Hash));

        var duplicateFileHandlingOptions = BuildDuplicateFileHandlingOptions(
            moveDuplicateFilesToDirectory: moveDirectory);
        _mockOptions.Setup(options => options.Value).Returns(duplicateFileHandlingOptions);
        var sut = new DuplicateFileMoveHandler(
            _mockRepository.Object, _mockFileSystem, _mockOptions.Object, _nullLogger);
        
        // Act
        await sut.RunTaskAsync();

        // Assert
        var filesAfterMove = _mockFileSystem.AllFiles.ToList();
        filesAfterMove.Should().BeEquivalentTo(expectedFilesAfterMove);
    }

    /// <summary>RunTaskAsync: If option DuplicateFileHandlingOptions.MoveDuplicateFilesToDirectory
    /// is not a valid directory, return without moving files.</summary>
    [Theory]
    [InlineData("***")]
    [InlineData("///")]
    public async Task RunTaskAsync_MoveDirectoryInvalid_ReturnsWithoutMovingFiles(
        string moveDirectory)
    {
        // Arrange
        var allFiles = _mockFileSystem.AllFiles.ToList();
        string[] duplicateFiles = [ allFiles[0], allFiles[3], allFiles[7] ];
        var expectedFilesAfterMove = _mockFileSystem.AllFiles;
        for (var index = 1; index < duplicateFiles.Length; index++)
        {
            var currentFile = duplicateFiles[index];
            expectedFilesAfterMove = expectedFilesAfterMove.Where(
                fileName => fileName != currentFile);
            expectedFilesAfterMove = expectedFilesAfterMove.Append(
                Path.Combine(moveDirectory, Path.GetFileName(currentFile)));
        }
        
        var duplicateFileFingerprints = 
            duplicateFiles
                .Select(duplicateFile =>
                    new FileFingerprint(
                        Path.GetFileName(duplicateFile),
                        Path.GetDirectoryName(duplicateFile)!,
                        1,
                        "000="))
                .ToList();
        _mockRepository
            .Setup(mockRepo => mockRepo.GetGroupingsWithDuplicateHashesAsync())
            .ReturnsAsync(
                duplicateFileFingerprints.GroupBy(fileFingerprint => fileFingerprint.Base64Hash));

        var duplicateFileHandlingOptions = BuildDuplicateFileHandlingOptions(
            moveDuplicateFilesToDirectory: moveDirectory);
        _mockOptions.Setup(options => options.Value).Returns(duplicateFileHandlingOptions);
        var sut = new DuplicateFileMoveHandler(
            _mockRepository.Object, _mockFileSystem, _mockOptions.Object, _nullLogger);
        
        // Act
        await sut.RunTaskAsync();

        // Assert
        var filesAfterMove = _mockFileSystem.AllFiles.ToList();
        filesAfterMove.Should().BeEquivalentTo(allFiles);
    }
    
    /// <summary>RunTaskAsync: If DuplicateFileHandlingOptions.MoveDuplicateFilesToDirectory
    /// contains files with the same name as duplicate files being moved, the duplicate files are
    /// given unique names before being moved.</summary>
    [Fact]
    public async Task RunTaskAsync_MoveDirectoryContainsFilesWithNameConflicts_MovedFilesRenamed()
    {
        // Arrange
        var allFiles = _mockFileSystem.AllFiles.ToList();
        string[] duplicateFiles = [ allFiles[0], allFiles[3], allFiles[7] ];
        const string moveDirectory = "/emptydir";
        _mockFileSystem.AddFile(
            Path.Combine(moveDirectory, Path.GetFileName(duplicateFiles[0])),
            new MockFileData(Guid.NewGuid().ToString()));
        _mockFileSystem.AddFile(
            Path.Combine(moveDirectory, Path.GetFileName(duplicateFiles[1])),
            new MockFileData(Guid.NewGuid().ToString()));
        _mockFileSystem.AddFile(
            Path.Combine(moveDirectory, Path.GetFileName(duplicateFiles[1]) + "_(1)"),
            new MockFileData(Guid.NewGuid().ToString()));
        var expectedFilesAfterMove = _mockFileSystem.AllFiles;
        for (var index = 1; index < duplicateFiles.Length; index++)
        {
            var currentFile = duplicateFiles[index];
            expectedFilesAfterMove = expectedFilesAfterMove.Where(
                fileName => fileName != currentFile);
            expectedFilesAfterMove = expectedFilesAfterMove.Append(
                Path.Combine(moveDirectory, Path.GetFileName(currentFile)));
        }
        
        var duplicateFileFingerprints = 
            duplicateFiles
                .Select(duplicateFile =>
                    new FileFingerprint(
                        Path.GetFileName(duplicateFile),
                        Path.GetDirectoryName(duplicateFile)!,
                        1,
                        "000="))
                .ToList();
        _mockRepository
            .Setup(mockRepo => mockRepo.GetGroupingsWithDuplicateHashesAsync())
            .ReturnsAsync(
                duplicateFileFingerprints.GroupBy(fileFingerprint => fileFingerprint.Base64Hash));

        var duplicateFileHandlingOptions = BuildDuplicateFileHandlingOptions(
            moveDuplicateFilesToDirectory: moveDirectory);
        _mockOptions.Setup(options => options.Value).Returns(duplicateFileHandlingOptions);
        var sut = new DuplicateFileMoveHandler(
            _mockRepository.Object, _mockFileSystem, _mockOptions.Object, _nullLogger);
        
        // Act
        await sut.RunTaskAsync();

        // Assert
        var filesAfterMove = _mockFileSystem.AllFiles.ToList();
        filesAfterMove.Should().BeEquivalentTo(allFiles);
    }
    
    private static DuplicateFileHandlingOptions BuildDuplicateFileHandlingOptions(
        bool interactive = false,
        DuplicateFileHandlingMethod duplicateFileHandlingMethod = DuplicateFileHandlingMethod.Move,
        string moveDuplicateFilesToDirectory = "/")
    {
        return new DuplicateFileHandlingOptions
        {
            Interactive = interactive,
            DuplicateFileHandlingMethod = duplicateFileHandlingMethod,
            MoveDuplicateFilesToDirectory = moveDuplicateFilesToDirectory,
        };
    }
}