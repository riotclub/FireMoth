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
using Moq.AutoMock;
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
    
    private readonly AutoMocker _mocker = new();
    
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
    /// IFileFingerprintRepository.AddAsync for each file in the collection.</summary>
    [Fact]
    public async void ScanFilesAsync_IEnumerableContainsValidFilePaths_AddsFilesToRepository()
    {
        // Arrange
        var mockFileFingerprintRepository = new Mock<IFileFingerprintRepository>();
        mockFileFingerprintRepository
            .Setup(repo => repo.AddAsync(It.IsAny<FileFingerprint>()));
        var mockFileHasher = new Mock<IFileHasher>();
        mockFileHasher
            .Setup(hasher => hasher.ComputeHashFromStream(It.IsAny<Stream>()))
            .Returns([0x01]);
        _mockFileSystem = BuildMockFileSystem();
        var files = _mockFileSystem.AllFiles.ToList();
        var sut = new FileScanOrchestrator(
            mockFileFingerprintRepository.Object,
            mockFileHasher.Object,
            _mockFileSystem,
            _nullLogger);
        
        // Act
        await sut.ScanFilesAsync(files);

        // Assert
        foreach (var file in files)
        {
            mockFileFingerprintRepository.Verify(repo =>
                repo.AddAsync(It.Is<FileFingerprint>(fingerprint => fingerprint.FullPath == file)));
        }
    }

    /// <summary>ScanFilesAsync: Passing [IEnumerable{string}:containing set of file paths] calls
    /// IFileHasher.ComputeHashAsync for each file in the collection.</summary>
    [Fact]
    public async void ScanFilesAsync_IEnumerableContainsValidFilePaths_ComputesHashForFiles()
    {
        // Arrange
        var mockFileFingerprintRepository = new Mock<IFileFingerprintRepository>();
        mockFileFingerprintRepository
            .Setup(repo => repo.AddAsync(It.IsAny<FileFingerprint>()));
        var mockFileHasher = new Mock<IFileHasher>();
        mockFileHasher
            .Setup(hasher => hasher.ComputeHashFromStream(It.IsAny<Stream>()))
            .Returns([0x01]);
        _mockFileSystem = BuildMockFileSystem();
        var files = _mockFileSystem.AllFiles.ToList();
        var sut = new FileScanOrchestrator(
            mockFileFingerprintRepository.Object,
            mockFileHasher.Object,
            _mockFileSystem,
            _nullLogger);
        
        // Act
        await sut.ScanFilesAsync(files);

        // Assert
        // TODO: Should verify that the streams passed as arguments actually contain the data from
        // the test file system, but it's a pain in the ass...
        mockFileHasher.Verify(hasher =>
            hasher.ComputeHashFromStream(It.IsAny<Stream>()), Times.Exactly(files.Count));
    }    
    
    /// <summary>ScanFilesAsync: Passing [IEnumerable{string}:containing set of file paths] calls
    /// IFileHasher.ComputeHashFromStream and IFileFingerprintRepository.AddAsync for each file in
    /// the collection.</summary>
    //[Fact]
    // public async void ScanFilesAsync_CollectionContainsFilePaths_CallsCorrectServiceMethods()
    // {
    //     // Arrange
    //     _mockFileSystem = BuildMockFileSystem();
    //     var savedStreamData = new List<byte[]>();
    //     var testFileHasher = new SHA256FileHasher();
    //     
    //     // Setup a callback that saves stream data for every hash computed during the test
    //     _mockFileHasher
    //         .Setup(hasher => hasher.ComputeHashFromStream(It.IsAny<Stream>()))
    //         //.Callback<Stream>(stream => SaveStreamDataAndResetPosition(stream, savedStreamData))
    //         .Returns();
    //         // .Returns<byte[]>(hashBytes =>
    //         //      testFileHasher.ComputeHashFromStream(new MemoryStream(hashBytes)));
    //     var testFiles = _mockFileSystem.Directory
    //         .EnumerateFiles("/", "*", new EnumerationOptions { RecurseSubdirectories = true })
    //         .ToList();
    //     var sut = new FileScanOrchestrator(
    //         _mockRepository.Object, _mockFileHasher.Object, _mockFileSystem, _nullLogger);
    //
    //     // Act
    //     await sut.ScanFilesAsync(testFiles);
    //
    //     // Assert
    //     foreach (var file in testFiles)
    //     {
    //         // Validate ComputeHashFromSteam
    //         var fileInfo = _mockFileSystem.FileInfo.New(file);
    //         var fileStream = fileInfo.OpenRead();
    //         var fileData = new byte[fileStream.Length];
    //         var bytesRead = fileStream.Read(fileData, 0, fileData.Length);
    //         if (bytesRead != fileStream.Length)
    //             throw new ArgumentException("Unable to read all stream data.");
    //
    //         savedStreamData.Should().Contain(
    //             streamData => streamData.SequenceEqual(fileData),
    //             "the collection containing stream data used in calls to ComputeHashFromStream "
    //                 + "should contain data matching one of the files in the MockFileSystem.");
    //         
    //         // Validate AddAsync
    //         fileStream.Position = 0;
    //         var hash = Convert.ToBase64String(testFileHasher.ComputeHashFromStream(fileStream));
    //         var fingerprint = new FileFingerprint(
    //             fileInfo.Name, fileInfo.DirectoryName ?? string.Empty, fileInfo.Length, hash);
    //         _mockRepository.Verify(repo =>
    //             repo.AddAsync(It.Is<FileFingerprint>(fp =>
    //                 fp.Equals(fingerprint))));
    //     }
    //     
    //     
    // }

#endregion

    private static void SaveStreamDataAndResetPosition(
        Stream stream, List<byte[]> dataList)
    {
        var inputBuffer = new byte[stream.Length];
        var bytesRead = stream.Read(inputBuffer, 0, inputBuffer.Length);
        if (bytesRead != stream.Length)
            throw new ArgumentException("Unable to read all stream data.");
        
        stream.Position = 0;
        dataList.Add(inputBuffer);
    }

    private static bool IsStreamDataEqual(Stream stream, byte[] data)
    {
        var inputBuffer = new byte[stream.Length];
        var bytesRead = stream.Read(inputBuffer, 0, inputBuffer.Length);
        if (bytesRead != stream.Length)
            throw new ArgumentException("Unable to read all stream data.");
        
        return inputBuffer.SequenceEqual(data);
    }

    private static MockFileSystem BuildMockFileSystem()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { "/RootDirFile", new MockFileData(Guid.NewGuid().ToString()) },
            { "/RootDirFile2", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/TestFile.txt", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/AnotherFile.dat", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/YetAnotherFile.xml", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/beep", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/meep.ext", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/subdirwithfiles/SubdirFileA.1", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/subdirwithfiles/SubdirFileB.2", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/subdirwithfiles/Creep.ext", new MockFileData(Guid.NewGuid().ToString()) },
        };
        
        var mockFileSystem = new MockFileSystem(files);
        mockFileSystem.AddDirectory("/emptydir");
        mockFileSystem.AddDirectory("/dirwithfiles/emptysubdir");

        return mockFileSystem;
    }
}