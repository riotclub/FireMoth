// <copyright file="FileScanOrchestratorTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.Orchestration;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Services.Orchestration;
using Xunit;
using Moq;
using Repository;
using RiotClub.FireMoth.Services.Tests.Unit.Extensions;
using Services.DataAnalysis;

/// <summary>
/// <p> 
/// Ctor<br/>
/// - Passing [IFileFingerprintRepository:null] throws ArgumentNullException.<br/>
/// - Passing [IFileHasher:null] throws ArgumentNullException.<br/>
/// - Passing [IFileSystem:null] throws ArgumentNullException.<br/>
/// - Passing [ILogger{FileScanOrchestrator}:null] throws ArgumentNullException.<br/>
/// - Passing [valid parameters] creates a new object.<br/>
/// </p>
/// <p>
/// ScanFilesAsync<br/>
/// - Passing [IEnumerable{string}:null] throws ArgumentNullException.<br/>
/// - Passing [IEnumerable{string}:containing set of file paths] calls
///   IFileHasher.ComputeHashFromStream and IFileFingerprintRepository.AddAsync for each file in the
///   collection.<br/>
/// - Passing [IEnumerable{string}:containing set of file paths] returns a ScanResult containing the
///   correct results of the scan.<br/>
/// - Passing [IEnumerable{string}:containing files that throw IOException or
///   UnauthorizedAccessException during scanning] skips the errored files and adds them to the
///   ScanResult.SkippedFiles collection.<br/>
/// - Passing [IEnumerable{string}:containing files that throw IOException or
///   UnauthorizedAccessException during scanning] skips the errored files and adds an error to the
///   ScanResult.Errors collection.<br/>
/// - Passing [IEnumerable{string}:containing files that throw IOException or
///   UnauthorizedAccessException during scanning] writes a properly formatted error message to the
///   log.<br/>  
/// - Completing the scan writes a properly formatted information message to the log.<br/>
/// - The method writes properly formatted information messages to the log for each file scanned.
/// <br/> 
/// </p>
/// </summary>
public class FileScanOrchestratorTests
{
    private readonly ILogger<FileScanOrchestrator> _nullLogger =
        NullLogger<FileScanOrchestrator>.Instance;
    private readonly Mock<IFileFingerprintRepository> _mockRepository = new();
    private readonly Mock<IFileHasher> _mockFileHasher = new();
    private MockFileSystem _mockFileSystem = new();
    
#region Ctor
    /// <summary>Ctor: Passing [IFileFingerprintRepository:null] throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Ctor_FileFingerprintRepositoryNull_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var ctorFunc = () =>
            new FileScanOrchestrator(null!, _mockFileHasher.Object, _mockFileSystem, _nullLogger);

        // Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>Ctor: Passing [IFileHasher:null] throws ArgumentNullException.</summary>
    [Fact]
    public void Ctor_FileHasherNull_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var ctorFunc = () =>
            new FileScanOrchestrator(_mockRepository.Object, null!, _mockFileSystem, _nullLogger);

        // Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>Ctor: Passing [IFileSystem:null] throws ArgumentNullException.</summary>
    [Fact]
    public void Ctor_FileSystemNull_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var ctorFunc = () =>
            new FileScanOrchestrator(
                _mockRepository.Object, _mockFileHasher.Object, null!, _nullLogger);

        // Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }
    
    /// <summary>Ctor: Passing [ILogger{FileScanOrchestrator}:null] throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Ctor_LoggerNull_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var ctorFunc = () =>
            new FileScanOrchestrator(
                _mockRepository.Object, _mockFileHasher.Object, _mockFileSystem, null!);

        // Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }
    
    /// <summary>Ctor: Passing [valid parameters] creates a new object.</summary>
    [Fact]
    public void Ctor_ValidParameters_CreatesNewObject()
    {
        // Arrange, Act
        var result = new FileScanOrchestrator(
            _mockRepository.Object, _mockFileHasher.Object, _mockFileSystem, _nullLogger);

        // Assert
        result.Should().NotBeNull();
    }
#endregion

#region ScanFilesAsync

/// ScanFilesAsync<br/>
/// - <br/>
/// - Passing [IEnumerable{string}:containing set of file paths] returns a ScanResult containing the
///   correct results of the scan.<br/>
/// - Passing [IEnumerable{string}:containing files that throw IOException or
///   UnauthorizedAccessException during scanning] skips the errored files and adds them to the
///   ScanResult.SkippedFiles collection.<br/>
/// - Passing [IEnumerable{string}:containing files that throw IOException or
///   UnauthorizedAccessException during scanning] skips the errored files and adds an error to the
///   ScanResult.Errors collection.<br/>
/// - Passing [IEnumerable{string}:containing files that throw IOException or
///   UnauthorizedAccessException during scanning] writes a properly formatted error message to the
///   log.<br/>  
/// - Completing the scan writes a properly formatted information message to the log.<br/>
/// - The method writes properly formatted information messages to the log for each file scanned.

    /// <summary>ScanFilesAsync: Passing [IEnumerable{string}:null] throws ArgumentNullException.
    /// </summary> 
    [Fact]
    public void ScanFilesAsync_EnumerableNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new FileScanOrchestrator(
            _mockRepository.Object, _mockFileHasher.Object, _mockFileSystem, _nullLogger);

        // Act
        var scanFilesAsyncFunc = () => sut.ScanFilesAsync(null!);

        // Assert
        scanFilesAsyncFunc.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    /// <summary>ScanFilesAsync: Passing [IEnumerable{string}:containing set of file paths] calls
    /// IFileHasher.ComputeHashFromStream and IFileFingerprintRepository.AddAsync for each file in
    /// the collection.</summary>
    [Fact]
    public void ScanFilesAsync_CollectionContainsFilePaths_CallsCorrectServiceMethods()
    {
        // Arrange
        _mockFileSystem = BuildMockFileSystem();
        _mockFileHasher
            .Setup(hasher => hasher.ComputeHashFromStream(It.IsAny<Stream>()))
            .Returns([0]);
        var testFiles = _mockFileSystem.Directory
            .EnumerateFiles("/", "*", new EnumerationOptions { RecurseSubdirectories = true })
            .ToList();
        var sut = new FileScanOrchestrator(
            _mockRepository.Object, _mockFileHasher.Object, _mockFileSystem, _nullLogger);

        // Act
        var result = sut.ScanFilesAsync(testFiles);

        // Assert
        result.Should().NotBeNull();
        foreach (var file in testFiles)
        {
            var fileInfo = _mockFileSystem.FileInfo.New(file);
            var fileStream = fileInfo.OpenRead();
            _mockFileHasher.Verify(hasher =>
                hasher.ComputeHashFromStream(It.Is<Stream>(stream =>
                    IsStreamDataEqual(stream, fileStream))));
        }
    }

#endregion

    private static bool IsStreamDataEqual(Stream a, Stream b)
    {
        var inputBufferA = new byte[a.Length];
        var bytesReadA = a.Read(inputBufferA, 0, inputBufferA.Length);
        var inputBufferB = new byte[b.Length];
        var bytesReadB = b.Read(inputBufferB, 0, inputBufferB.Length);

        return inputBufferA.SequenceEqual(inputBufferB);
    }

    private static MockFileSystem BuildMockFileSystem()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { "/RootDirFile", new MockFileData("7") },
            { "/RootDirFile2", new MockFileData("8") },
            { "/dirwithfiles/TestFile.txt", new MockFileData("0") },
            { "/dirwithfiles/AnotherFile.dat", new MockFileData("1") },
            { "/dirwithfiles/YetAnotherFile.xml", new MockFileData("2") },
            { "/dirwithfiles/beep", new MockFileData("3") },
            { "/dirwithfiles/meep.ext", new MockFileData("2") },
            { "/dirwithfiles/subdirwithfiles/SubdirFileA.1", new MockFileData("3") },
            { "/dirwithfiles/subdirwithfiles/SubdirFileB.2", new MockFileData("4") },
            { "/dirwithfiles/subdirwithfiles/Creep.ext", new MockFileData("5") },
        };
        
        var mockFileSystem = new MockFileSystem(files);
        mockFileSystem.AddDirectory("/emptydir");
        mockFileSystem.AddDirectory("/dirwithfiles/emptysubdir");

        return mockFileSystem;
    }
}