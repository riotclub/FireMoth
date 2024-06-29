// <copyright file="DirectoryScanOrchestratorTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.Orchestration;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Services.Orchestration;
using Xunit;
using Moq;
using RiotClub.FireMoth.Services.Tests.Unit.Extensions;

/// <summary>
/// <p> 
/// Ctor<br/>
/// - Passing [IFileScanOrchestrator:null] throws ArgumentNullException.<br/>
/// - Passing [IFileSystem:null] throws ArgumentNullException.<br/>
/// - Passing [IOptions{DirectoryScanOptions}:null] throws ArgumentNullException.<br/>
/// - Passing [IOptions{DirectoryScanOptions} with null Value.Directory property] throws
/// ArgumentException.<br/>
/// - Passing [ILogger{DirectoryScanOrchestrator}:null] throws ArgumentNullException.<br/>
/// - Passing [valid parameters] creates a new object.<br/>
/// </p>
/// <p>
/// ScanDirectoryAsync<br/>
/// - When DirectoryScanOptions.Recursive is true, calls IFileScanOrchestrator.ScanFilesAsync with
/// all available files from the scan directory and its subdirectories, enumerated recursively.<br/> 
/// - When DirectoryScanOptions.Recursive is false, calls IFileScanOrchestrator.ScanFilesAsync with
/// all available files from the scan directory, ignoring any files in subdirectories.<br/> 
/// - When DirectoryScanOptions.Directory specifies an empty directory,
/// IFileScanOrchestrator.ScanFilesAsync is not called.<br/>
/// - Relevant messages are logged.<br/>
/// </p>
/// </summary>
public class DirectoryScanOrchestratorTests
{
    private readonly ILogger<DirectoryScanOrchestrator> _nullLogger =
        NullLogger<DirectoryScanOrchestrator>.Instance;
    
#region Ctor
    /// <summary>Ctor: Passing [IFileScanOrchestrator:null] throws ArgumentNullException.</summary>
    [Fact]
    public void Ctor_IFileScanOrchestratorNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockFileSystem = new Mock<IFileSystem>(); 
        var mockOptions = new Mock<IOptions<DirectoryScanOptions>>();

        // Act
        var ctorFunc = () =>
            new DirectoryScanOrchestrator(
                null!, mockFileSystem.Object, mockOptions.Object, _nullLogger);

        // Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>Ctor: Passing [IFileSystem:null] throws ArgumentNullException.</summary>
    [Fact]
    public void Ctor_IFileSystemNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockFileScanOrchestrator = new Mock<IFileScanOrchestrator>();
        var mockOptions = new Mock<IOptions<DirectoryScanOptions>>();

        // Act
        var ctorFunc = () =>
            new DirectoryScanOrchestrator(
                mockFileScanOrchestrator.Object, null!, mockOptions.Object, _nullLogger);

        // Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }
    
    /// <summary>Ctor: Passing [IOptions{DirectoryScanOptions}:null] throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Ctor_IOptionsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockFileScanOrchestrator = new Mock<IFileScanOrchestrator>();
        var mockFileSystem = new Mock<IFileSystem>();

        // Act
        var ctorFunc = () =>
            new DirectoryScanOrchestrator(
                mockFileScanOrchestrator.Object, mockFileSystem.Object, null!, _nullLogger);

        // Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>Ctor: Passing [IOptions{DirectoryScanOptions} with null Value.Directory property]
    /// throws ArgumentException.</summary>
    [Fact]
    public void Ctor_IOptionsValueDirectoryNull_ThrowsArgumentException()
    {
        // Arrange
        var mockFileScanOrchestrator = new Mock<IFileScanOrchestrator>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockOptions = new Mock<IOptions<DirectoryScanOptions>>();
        var testDirectoryScanOptions = new DirectoryScanOptions { Directory = null! };
        mockOptions.Setup(options => options.Value).Returns(testDirectoryScanOptions);
        
        // Act
        var ctorFunc = () =>
            new DirectoryScanOrchestrator(
                mockFileScanOrchestrator.Object,
                mockFileSystem.Object,
                mockOptions.Object,
                _nullLogger);

        // Assert
        ctorFunc.Should().ThrowExactly<ArgumentException>();
    }

    /// <summary>Ctor: Passing [ILogger{DirectoryScanOrchestrator}:null] throws
    /// ArgumentNullException.</summary>
    [Fact]
    public void Ctor_ILoggerNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockFileScanOrchestrator = new Mock<IFileScanOrchestrator>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockOptions = new Mock<IOptions<DirectoryScanOptions>>();
        var testDirectoryScanOptions = new DirectoryScanOptions { Directory = "/" };
        mockOptions.Setup(options => options.Value).Returns(testDirectoryScanOptions);
        
        // Act
        var ctorFunc = () =>
            new DirectoryScanOrchestrator(
                mockFileScanOrchestrator.Object, mockFileSystem.Object, mockOptions.Object, null!);

        // Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>Passing [valid parameters] creates a new object.</summary>
    [Fact]
    public void Ctor_ValidParameters_CreatesNewObject()
    {
        // Arrange
        var mockFileScanOrchestrator = new Mock<IFileScanOrchestrator>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockOptions = new Mock<IOptions<DirectoryScanOptions>>();
        var testDirectoryScanOptions = new DirectoryScanOptions { Directory = "/" };
        mockOptions.Setup(options => options.Value).Returns(testDirectoryScanOptions);
        
        // Act
        var sut = new DirectoryScanOrchestrator(
            mockFileScanOrchestrator.Object,
            mockFileSystem.Object,
            mockOptions.Object,
            _nullLogger);

        // Assert
        sut.Should().NotBeNull();
    }
#endregion

#region ScanDirectoryAsync
    /// <summary>ScanDirectoryAsync: When DirectoryScanOptions.Recursive is true, calls
    /// IFileScanOrchestrator.ScanFilesAsync with all available files from the scan directory and
    /// its subdirectories, enumerated recursively.</summary>
    [Fact]
    public async void ScanDirectoryAsync_RecursiveScanOptionTrue_InitiatesRecursiveScan()
    {
        // Arrange
        var mockFileScanOrchestrator = new Mock<IFileScanOrchestrator>();
        IEnumerable<string> result = new List<string>();
        mockFileScanOrchestrator
            .Setup(orchestrator => orchestrator.ScanFilesAsync(It.IsAny<IEnumerable<string>>()))
            .Callback<IEnumerable<string>>(fileList => result = fileList);
        var mockFileSystem = BuildMockFileSystem();
        var mockOptions = new Mock<IOptions<DirectoryScanOptions>>();
        var testDirectoryScanOptions = new DirectoryScanOptions 
            { Directory = "/", Recursive = true };
        mockOptions.Setup(options => options.Value).Returns(testDirectoryScanOptions);
        var expected = mockFileSystem.Directory.EnumerateFiles(
            "/", "*", new EnumerationOptions { RecurseSubdirectories = true });
        var sut = new DirectoryScanOrchestrator(
            mockFileScanOrchestrator.Object, mockFileSystem, mockOptions.Object, _nullLogger);
        
        // Act
        await sut.ScanDirectoryAsync();

        // Assert
        result.Should().Equal(expected);
    }
    
    /// <summary>ScanDirectoryAsync: When DirectoryScanOptions.Recursive is false, calls
    /// IFileScanOrchestrator.ScanFilesAsync with all available files from the scan directory,
    /// ignoring any files in subdirectories.</summary>
    [Fact]
    public async void ScanDirectoryAsync_RecursiveScanOptionFalse_InitiatesNonRecursiveScan()
    {
        // Arrange
        var mockFileScanOrchestrator = new Mock<IFileScanOrchestrator>();
        IEnumerable<string> result = new List<string>();
        mockFileScanOrchestrator
            .Setup(orchestrator => orchestrator.ScanFilesAsync(It.IsAny<IEnumerable<string>>()))
            .Callback<IEnumerable<string>>(fileList => result = fileList);
        var mockFileSystem = BuildMockFileSystem();
        var mockOptions = new Mock<IOptions<DirectoryScanOptions>>();
        var testDirectoryScanOptions = new DirectoryScanOptions 
            { Directory = "/", Recursive = false };
        mockOptions.Setup(options => options.Value).Returns(testDirectoryScanOptions);
        var expected = mockFileSystem.Directory.EnumerateFiles(
            "/", "*", new EnumerationOptions { RecurseSubdirectories = false });
        var sut = new DirectoryScanOrchestrator(
            mockFileScanOrchestrator.Object, mockFileSystem, mockOptions.Object, _nullLogger);
        
        // Act
        await sut.ScanDirectoryAsync();

        // Assert
        result.Should().Equal(expected);
    }

    /// <summary>ScanDirectoryAsync: When DirectoryScanOptions.Directory specifies an empty
    /// directory, IFileScanOrchestrator.ScanFilesAsync is not called.</summary>
    [Fact]
    public async void ScanDirectoryAsync_EmptyDirectory_DoesNotInitiateScan()
    {
        // Arrange
        var mockFileScanOrchestrator = new Mock<IFileScanOrchestrator>();
        var mockFileSystem = BuildMockFileSystem();
        var mockOptions = new Mock<IOptions<DirectoryScanOptions>>();
        var testDirectoryScanOptions = new DirectoryScanOptions 
            { Directory = "/emptydir", Recursive = false };
        mockOptions.Setup(options => options.Value).Returns(testDirectoryScanOptions);
        var sut = new DirectoryScanOrchestrator(
            mockFileScanOrchestrator.Object, mockFileSystem, mockOptions.Object, _nullLogger);
        
        // Act
        await sut.ScanDirectoryAsync();

        // Assert
        mockFileScanOrchestrator.Verify(
            o => o.ScanFilesAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    /// <summary>ScanDirectoryAsync: Relevant messages are logged.</summary>
    [Fact]
    public async void ScanDirectoryAsync_MethodCalled_LogsExpectedMessages()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DirectoryScanOrchestrator>>();
        var mockFileScanOrchestrator = new Mock<IFileScanOrchestrator>();
        var mockFileSystem = BuildMockFileSystem();
        var mockOptions = new Mock<IOptions<DirectoryScanOptions>>();
        const string scanDirectory = "/emptydir";
        const bool recursive = false;
        var testDirectoryScanOptions = new DirectoryScanOptions 
            { Directory = scanDirectory, Recursive = recursive };
        mockOptions.Setup(options => options.Value).Returns(testDirectoryScanOptions);
        var sut = new DirectoryScanOrchestrator(
            mockFileScanOrchestrator.Object, mockFileSystem, mockOptions.Object, mockLogger.Object);
        var expected = $"Scanning directory '{scanDirectory}' (recursive: {recursive})";
        
        // Act
        await sut.ScanDirectoryAsync();
        
        // Assert
        mockLogger.VerifyLogCalled(expected, LogLevel.Information);
    }
#endregion

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