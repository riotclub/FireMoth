// <copyright file="FileScannerTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.FileScanning;

/*
 * Constructor:
 * * Null IDataAccessLayer throws exception
 *      * Ctor_NullIDataAccessProvider_ThrowsArgumentNullException
 * * Null IFileHasher throws exception
 *      * Ctor_NullIFileHasher_ThrowsArgumentNullException
 * * Null ILogger throws exception
 *      * Ctor_NullILogger_ThrowsArgumentNullException
 *
 * ScanDirectory:
 * * Null IDirectoryInfo throws exception
 *      * ScanDirectory_NullIDirectoryInfo_ThrowsArgumentNullException
 * * Valid directory adds file fingerprint records to data provider
 *      * ScanDirectory_ValidDirectory_AddsFileRecordsToDataAccessProvider
 * * Valid directory produces correct log events
 *      * ScanDirectory_ValidDirectory_LogsScanEvents
 * * Valid directory with errored files returns correct list of scanned files
 *      * ScanDirectory_DirectoryWithErroredFiles_ReturnsCorrectScannedFiles
 * * Valid directory with skipped files returns correct list of skipped files
 *      * ScanDirectory_DirectoryWithErroredFiles_ReturnsCorrectSkippedFiles
 * * Valid directory with errored files returns correct list of scan errors
 *      * ScanDirectory_DirectoryWithErroredFiles_ReturnsCorrectScanErrors
 * * Valid directory with errored files produces log events for errored files
 *      * ScanDirectory_DirectoryWithErroredFiles_LogsErrorEvents
 * * Does not attempt to add unscannable files to the data access provider
 *      * ScanDirectory_DirectoryWithErroredFiles_NoErroredFilesAddedToDataAccessProvider
 * * Valid empty directory results in successful scan
 *      * ScanDirectory_EmptyDirectory_ReturnsCorrectScanResult
 * * Valid empty directory adds no records to data provider
 *      * ScanDirectory_EmptyDirectory_NoRecordsAddedToDataAccessProvider
 * * Invalid directory returns correct scan result
 *      * ScanDirectory_InvalidDirectory_ReturnsCorrectScanResult
 * * Access to scan directory denied returns correct scan result
 *      * ScanDirectory_DirectoryAccessDenied_ReturnsCorrectScanResult
 * * Access to file denied produces log event
 *      * ScanDirectory_FileAccessDenied_LogsErrorEvent
 * * Access to file denied adds file to skipped files
 *      * ScanDirectory_FileAccessDenied_ReturnsCorrectSkippedFiles
 * * Access to file denied adds file to errored files
 *      * ScanDirectory_FileAccessDenied_ReturnsCorrectScanErrors
 * * Recursive scan option results in successful scan of all files and subdirectory files
 *      * ScanDirectory_RecursiveScan_AddsSubdirectoryFilesToDataAccessProvider
 * * Non-recursive scan option ignores subdirectories
 *      * ScanDirectory_NonRecursiveScan_IgnoresSubdirectories
 */
    
/*
[ExcludeFromCodeCoverage]
public class FileScannerTests : IDisposable
{
    private readonly Mock<IFileHasher> mockFileHasher;
    private readonly Mock<ILogger<OnDemandScanOrchestrator>> mockLogger;
    private readonly MockFileSystem mockFileSystem;
    private readonly byte[] testHashData = new byte[] { 0x20, 0x20, 0x20 };
    private readonly IDirectoryInfo testDirectory;

    private Mock<IDataAccessLayer<IFileFingerprint>> mockDataAccessProvider;
    private Mock<IFileFingerprintRepository> mockFileFingerprintRepository;
    private bool disposed = false;

    public FileScannerTests()
    {
        this.mockDataAccessProvider = new Mock<IDataAccessLayer<IFileFingerprint>>(MockBehavior.Strict);
        mockFileFingerprintRepository = new Mock<IFileFingerprintRepository>();
        this.mockFileHasher = new Mock<IFileHasher>();
        this.mockFileHasher
            .Setup(hasher => hasher.ComputeHashFromStream(It.IsAny<Stream>()))
            .Returns(this.testHashData);
        this.mockFileSystem = BuildMockFileSystem();
        this.mockLogger = new Mock<ILogger<OnDemandScanOrchestrator>>();

        this.testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
            @"c:\dirwithfiles");
    }

    // Ctor: Null IDataAccessLayer throws exception
    [Fact]
    public void Ctor_NullIDataAccessProvider_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            new OnDemandScanOrchestrator(null, this.mockFileHasher.Object, this.mockLogger.Object));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    // Ctor: Null IFileHasher throws exception
    [Fact]
    public void Ctor_NullIFileHasher_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            new OnDemandScanOrchestrator(this.mockDataAccessProvider.Object, null, this.mockLogger.Object));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    // Ctor: Null ILogger throws exception
    [Fact]
    public void Ctor_NullILogger_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OnDemandScanOrchestrator(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                mockFileFingerprintRepository.Object, this.mockFileHasher.Object, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    // ScanDirectory: Null ScanOptions throws exception
    [Fact]
    public void ScanDirectory_NullIDirectoryInfo_ThrowsArgumentNullException()
    {
        // Arrange
        var fileScanner = this.GetDefaultFileScanner();

        // Act, Assert
        Assert.Throws<ArgumentNullException>(() => fileScanner.ScanDirectory(null!));
    }

    // ScanDirectory: Valid directory adds file fingerprint records to data provider
    [Theory]
    [InlineData(@"c:\dirwithfiles\")]
    [InlineData(@"c:\dirwithfiles\subdirwithfiles")]
    public void ScanDirectory_ValidDirectory_AddsFileRecordsToDataAccessProvider(
        string directory)
    {
        // Arrange
        var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(directory);
        var files = testDirectory.EnumerateFiles();
        foreach (var file in files)
        {
            this.mockDataAccessProvider
                .Setup(dap =>
                    dap.Add(It.Is<IFileFingerprint>(ff =>
                        ff.FileName == file.Name)));
        }

        // Act
        this.GetDefaultFileScanner().ScanDirectory(new ScanOptions(testDirectory));

        // Assert
        this.mockDataAccessProvider.VerifyAll();
    }

    // ScanDirectory: Valid directory produces correct log events
    [Fact]
    public void ScanDirectory_ValidDirectory_LogsScanEvents()
    {
        // Arrange
        this.mockDataAccessProvider.Setup(dap =>
            dap.Add(It.IsAny<IFileFingerprint>()));
        var files = this.testDirectory.EnumerateFiles();

        // Act
        this.GetDefaultFileScanner().ScanDirectory(
            new ScanOptions(this.testDirectory));

        // Assert
        foreach (var file in files)
        {
            this.mockLogger.VerifyLogCalled(
                $"Scanning file '{file.Name}'", LogLevel.Information);
        }
    }

    // ScanDirectory: Valid directory with errored files returns correct list of scanned files
    [Fact]
    public void ScanDirectory_DirectoryWithErroredFiles_ReturnsCorrectScannedFiles()
    {
        // Arrange
        var mockDirectory = this.GetMockDirectory(fullPath: this.testDirectory.FullName);
        var mockScanOptions = GetMockScanOptions(mockDirectory, false, OutputDuplicateFileFingerprintsOption.All);
        var errorFiles = GetFileFingerprints(this.testDirectory, "eep");
        var expectedFiles = this.testDirectory.EnumerateFiles().ToList();
        expectedFiles.RemoveAll(fileInfo =>
            errorFiles.Contains(new FileFingerprint(
                fileInfo.Name,
                fileInfo.DirectoryName,
                fileInfo.Length,
                Convert.ToBase64String(this.testHashData))));
        var testFileScanner = this.GetFileScannerWithErroredFiles(
            errorFiles, new IOException());

        // Act
        var result = testFileScanner.ScanDirectory(mockScanOptions.Object);

        // Assert
        foreach (var expectedFile in expectedFiles)
        {
            Assert.Contains(
                result.ScannedFiles,
                fingerprint => fingerprint.FileName == expectedFile.Name);
        }
    }

    // ScanDirectory: Valid directory with errored files returns correct list of skipped files
    [Fact]
    public void ScanDirectory_DirectoryWithErroredFiles_ReturnsCorrectSkippedFiles()
    {
        // Arrange
        var mockDirectory = this.GetMockDirectory(fullPath: this.testDirectory.FullName);
        var mockScanOptions = GetMockScanOptions(mockDirectory, false, OutputDuplicateFileFingerprintsOption.All);
        var errorFiles = GetFileFingerprints(this.testDirectory, "eep");
        var testFileScanner = this.GetFileScannerWithErroredFiles(
            errorFiles, new IOException());

        // Act
        var result = testFileScanner.ScanDirectory(mockScanOptions.Object);

        // Assert
        Assert.Equal(errorFiles.Count(), result.SkippedFiles.Count);
        foreach (var file in errorFiles)
        {
            Assert.Contains(file.FullPath, (IDictionary<string, string>)result.SkippedFiles);
        }
    }

    // ScanDirectory: Valid directory with errored files returns correct list of scan errors
    [Fact]
    public void ScanDirectory_DirectoryWithErroredFiles_ReturnsCorrectScanErrors()
    {
        // Arrange
        var mockDirectory = this.GetMockDirectory(fullPath: this.testDirectory.FullName);
        var mockScanOptions = GetMockScanOptions(mockDirectory, false, OutputDuplicateFileFingerprintsOption.All);
        var directoryFiles = this.mockFileSystem.DirectoryInfo
            .FromDirectoryName(this.testDirectory.FullName)
            .EnumerateFiles();
        mockDirectory
            .Setup(dir => dir.EnumerateFiles())
            .Returns(directoryFiles);
        var errorFiles = GetFileFingerprints(this.testDirectory, "eep");
        var testFileScanner = this.GetFileScannerWithErroredFiles(
            errorFiles, new IOException());

        // Act
        var scanResult = testFileScanner.ScanDirectory(mockScanOptions.Object);

        // Assert
        var resultErrorFiles = scanResult.Errors.Select(e => e.Path);
        Assert.Equal(errorFiles.Count(), resultErrorFiles.Count());
        foreach (var errorFileFingerprint in errorFiles)
        {
            Assert.Contains(errorFileFingerprint.FullPath, resultErrorFiles);
        }
    }

    // ScanDirectory: Valid directory with errored files produces log events for errored files
    [Fact]
    public void ScanDirectory_DirectoryWithErroredFiles_LogsErrorEvents()
    {
        // Arrange
        var errorFileNamePattern = "eep";

        var mockDirectory = this.GetMockDirectory(fullPath: this.testDirectory.FullName);
        var errorFiles = GetFileFingerprints(this.testDirectory, errorFileNamePattern);
        var errorFileInfo = this.testDirectory.EnumerateFiles('*' + errorFileNamePattern + '*');
        mockDirectory.Setup(dir => dir.EnumerateFiles()).Returns(errorFileInfo);
        var mockScanOptions = GetMockScanOptions(mockDirectory, true, OutputDuplicateFileFingerprintsOption.All);
        var testFileScanner = this.GetFileScannerWithErroredFiles(
            errorFiles, new IOException());

        // Act
        testFileScanner.ScanDirectory(mockScanOptions.Object);

        // Assert
        foreach (var file in errorFiles)
        {
            this.mockLogger.VerifyLogCalled(
                $"Could not add record for file '{file.FullPath}': I/O error occurred.; skipping file.",
                LogLevel.Error);
        }
    }

    // ScanDirectory: Does not attempt to add unscannable files to the data access provider
    [Fact]
    public void ScanDirectory_DirectoryWithErroredFiles_NoErroredFilesAddedToDataAccessProvider()
    {
        // Arrange
        var errorFiles = GetFileFingerprints(this.testDirectory, "AnotherFile");
        foreach (var errorFile in errorFiles)
        {
            this.mockFileSystem.GetFile(errorFile.FullPath).AllowedFileShare = FileShare.None;
        }

        var mockDataAccessProvider = new Mock<IDataAccessLayer<IFileFingerprint>>(MockBehavior.Loose);
        mockDataAccessProvider.Setup(dap => dap.Add(It.IsAny<IFileFingerprint>()));
        var testFileScanner = new OnDemandScanOrchestrator(
            mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);

        // Act
        var scanResult = testFileScanner.ScanDirectory(
            new ScanOptions(this.testDirectory));

        // Assert
        foreach (var errorFile in errorFiles)
        {
            mockDataAccessProvider.Verify(
                dap => dap.Add(errorFile), Times.Never);
        }
    }

    // ScanDirectory: Valid empty directory results in successful scan
    [Fact]
    public void ScanDirectory_EmptyDirectory_ReturnsCorrectScanResult()
    {
        // Arrange
        var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\emptydir");

        // Act
        var result = this.GetDefaultFileScanner().ScanDirectory(
            new ScanOptions(testDirectory));

        // Assert
        Assert.Empty(result.ScannedFiles);
        Assert.Empty(result.SkippedFiles);
        Assert.Empty(result.Errors);
    }

    // ScanDirectory: Valid empty directory adds no records to data provider
    [Fact]
    public void ScanDirectory_EmptyDirectory_NoRecordsAddedToDataAccessProvider()
    {
        // Arrange
        var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\emptydir");
        var mockDataAccessProvider = new Mock<IDataAccessLayer<IFileFingerprint>>();
        var fileScanner = new OnDemandScanOrchestrator(
            mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);

        // Act
        var result = fileScanner.ScanDirectory(new ScanOptions(testDirectory));

        // Assert
        mockDataAccessProvider.Verify(
            dap => dap.Add(It.IsAny<IFileFingerprint>()), Times.Never);
    }

    // ScanDirectory: Invalid directory returns correct scan result
    [Theory]
    [InlineData(@"C:\path/with|invalid/chars")]
    [InlineData(@"\\:\\||>\a\b::t<")]
    public void ScanDirectory_InvalidDirectory_ReturnsCorrectScanResult(string directory)
    {
        // Arrange
        var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(directory);

        // Act
        var result = this.GetDefaultFileScanner().ScanDirectory(
            new ScanOptions(testDirectory, true));

        // Assert
        Assert.Empty(result.ScannedFiles);
        Assert.Empty(result.SkippedFiles);
        Assert.Collection(
            result.Errors,
            subdirError =>
            {
                Assert.Equal(testDirectory.FullName, subdirError.Path);
                Assert.StartsWith(
                    "Could not enumerate subdirectories of directory", subdirError.Message);
                Assert.True(
                    subdirError.Exception!.GetType() == typeof(NotSupportedException)
                    || subdirError.Exception.GetType() == typeof(ArgumentException));
            },
            fileError =>
            {
                Assert.Equal(testDirectory.FullName, fileError.Path);
                Assert.StartsWith("Could not enumerate files of directory", fileError.Message);
                Assert.True(
                    fileError?.Exception?.GetType() == typeof(NotSupportedException)
                    || fileError?.Exception?.GetType() == typeof(ArgumentException));
            });
    }

    // ScanDirectory: Access to scan directory denied returns correct scan result
    [Fact]
    public void ScanDirectory_DirectoryAccessDenied_ReturnsCorrectScanResult()
    {
        // Arrange
        var mockDirectory = this.GetMockDirectory(
            fullPath: this.testDirectory.FullName,
            throwOnDirectoryEnumeration: new UnauthorizedAccessException(),
            throwOnFileEnumeration: new UnauthorizedAccessException());
        var mockScanOptions = GetMockScanOptions(mockDirectory, true, OutputDuplicateFileFingerprintsOption.All);
        this.mockDataAccessProvider = new Mock<IDataAccessLayer<IFileFingerprint>>();

        // Act
        var result = this.GetDefaultFileScanner().ScanDirectory(mockScanOptions.Object);

        // Assert
        Assert.Empty(result.ScannedFiles);
        Assert.Empty(result.SkippedFiles);
        Assert.Collection(
            result.Errors,
            errorOne =>
            {
                Assert.Equal(this.testDirectory.FullName, errorOne.Path);
                Assert.StartsWith(
                    $"Could not enumerate subdirectories of directory '{this.testDirectory}'",
                    errorOne.Message);
                Assert.IsType<UnauthorizedAccessException>(errorOne.Exception);
            },
            errorTwo =>
            {
                Assert.Equal(this.testDirectory.FullName, errorTwo.Path);
                Assert.StartsWith(
                    $"Could not enumerate files of directory '{this.testDirectory}'",
                    errorTwo.Message);
                Assert.IsType<UnauthorizedAccessException>(errorTwo.Exception);
            });
    }

    // ScanDirectory: Access to file denied produces log event
    [Fact]
    public void ScanDirectory_FileAccessDenied_LogsErrorEvent()
    {
        // Arrange
        var errorFiles = GetFileFingerprints(this.testDirectory, "eep");
        var testFileScanner = this.GetFileScannerWithErroredFiles(
            errorFiles, new UnauthorizedAccessException());

        // Act
        testFileScanner.ScanDirectory(new ScanOptions(this.testDirectory));

        // Assert
        foreach (var file in errorFiles)
        {
            this.mockLogger.VerifyLogCalled(
                $"Could not add record for file '{file.FullPath}': Attempted to " +
                    $"perform an unauthorized operation.; skipping file.",
                LogLevel.Error);
        }
    }

    // ScanDirectory: Access to file denied adds file to skipped files
    [Fact]
    public void ScanDirectory_FileAccessDenied_ReturnsCorrectSkippedFiles()
    {
        // Arrange
        var errorFiles = GetFileFingerprints(this.testDirectory, "eep");
        var testFileScanner = this.GetFileScannerWithErroredFiles(
            errorFiles, new UnauthorizedAccessException());

        // Act
        var result = testFileScanner.ScanDirectory(
            new ScanOptions(this.testDirectory));

        // Assert
        Assert.Equal(errorFiles.Count(), result.SkippedFiles.Count);
        foreach (var file in errorFiles)
        {
            Assert.Contains(file.FullPath, (IDictionary<string, string>)result.SkippedFiles);
        }
    }

    // ScanDirectory: Access to file denied adds file to errored files
    [Fact]
    public void ScanDirectory_FileAccessDenied_ReturnsCorrectScanErrors()
    {
        // Arrange
        var errorFiles = GetFileFingerprints(this.testDirectory, "eep");
        var testFileScanner = this.GetFileScannerWithErroredFiles(
            errorFiles, new UnauthorizedAccessException());

        // Act
        var scanResult = testFileScanner.ScanDirectory(
            new ScanOptions(this.testDirectory));

        // Assert
        var resultErrorFiles = scanResult.Errors.Select(e => e.Path);
        Assert.Equal(errorFiles.Count(), resultErrorFiles.Count());
        foreach (var errorFile in errorFiles)
        {
            Assert.Contains(errorFile.FullPath, resultErrorFiles);
        }
    }

    // ScanDirectory: Recursive scan option results in successful scan of all files and subdirectory files
    [Fact]
    public void ScanDirectory_RecursiveScan_AddsSubdirectoryFilesToDataAccessProvider()
    {
        // Arrange
        this.mockDataAccessProvider = new Mock<IDataAccessLayer<IFileFingerprint>>();

        // Act
        var scanResult = this.GetDefaultFileScanner().ScanDirectory(
            new ScanOptions(this.testDirectory, true));

        // Assert
        var expectedFiles = this.mockFileSystem.AllFiles.ToList();
        foreach (var file in expectedFiles)
        {
            this.mockDataAccessProvider.Verify(dap =>
                dap.Add(It.Is<IFileFingerprint>(fingerprint =>
                    fingerprint.FullPath.Equals(file, StringComparison.OrdinalIgnoreCase))));
        }
    }

    // ScanDirectory: Non-recursive scan option ignores subdirectories
    [Fact]
    public void ScanDirectory_NonRecursiveScan_IgnoresSubdirectories()
    {
        // Arrange
        this.mockDataAccessProvider = new Mock<IDataAccessLayer<IFileFingerprint>>();

        // Act
        var scanResult = this.GetDefaultFileScanner().ScanDirectory(
            new ScanOptions(this.testDirectory));

        // Assert
        this.mockDataAccessProvider.Verify(
            dap => dap.Add(
                It.Is<IFileFingerprint>(file =>
                    file.DirectoryName.StartsWith(
                        @"c:\dirwithfiles\subdirwithfiles",
                        StringComparison.OrdinalIgnoreCase))),
            Times.Never);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and, optionally, managed resources.
    /// </summary>
    /// <param name="disposing">If true, managed resources are freed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing)
        {
        }

        this.disposed = true;
    }

    private static Mock<IScanOptions> GetMockScanOptions(
        Mock<IDirectoryInfo> mockDirectory, bool recursive, OutputDuplicateFileFingerprintsOption outputOption)
    {
        var mockScanOptions = new Mock<IScanOptions>();
        mockScanOptions
            .SetupGet(scanOptions => scanOptions.ScanDirectory)
            .Returns(mockDirectory.Object);
        mockScanOptions
            .SetupGet(scanOptions => scanOptions.RecursiveScan)
            .Returns(recursive);

        return mockScanOptions;
    }

    private static MockFileSystem BuildMockFileSystem()
    {
        var mockFileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { @"c:\dirwithfiles\TestFile.txt", new MockFileData("000") },
                { @"c:\dirwithfiles\AnotherFile.dat", new MockFileData("111") },
                { @"c:\dirwithfiles\YetAnotherFile.xml", new MockFileData("222") },
                { @"c:\dirwithfiles\beep", new MockFileData("333") },
                { @"c:\dirwithfiles\meep.ext", new MockFileData("222") },
                { @"c:\dirwithfiles\subdirwithfiles\SubdirFileA.1", new MockFileData("333") },
                { @"c:\dirwithfiles\subdirwithfiles\SubdirFileB.2", new MockFileData("444") },
                { @"c:\dirwithfiles\subdirwithfiles\Creep.ext", new MockFileData("555") },
            });

        mockFileSystem.AddDirectory(@"c:\emptydir");
        mockFileSystem.AddDirectory(@"c:\dirwithfiles\emptysubdir");

        return mockFileSystem;
    }

    private static IEnumerable<IFileFingerprint> GetFileFingerprints(
        IDirectoryInfo directory, string? fileNameFilter = null)
    {
        var files = directory.EnumerateFiles();

        if (fileNameFilter is not null)
        {
            files = files.Where(fileInfo => fileInfo.Name.Contains(fileNameFilter));
        }

        return files.Select(fileInfo =>
            new FileFingerprint(
                fileInfo.Name,
                fileInfo.DirectoryName,
                fileInfo.Length,
                Convert.ToBase64String(new byte[] { 0x20, 0x20, 0x20 })));
    }

    private Mock<IDirectoryInfo> GetMockDirectory(
        string fullPath,
        Exception? throwOnDirectoryEnumeration = null,
        Exception? throwOnFileEnumeration = null)
    {
        var mockDirectory = new Mock<IDirectoryInfo>();
        mockDirectory.SetupGet(dir => dir.FullName).Returns(fullPath);
        if (throwOnDirectoryEnumeration is not null)
        {
            mockDirectory
                .Setup(dir => dir.EnumerateDirectories())
                .Throws(throwOnDirectoryEnumeration);
        }
        else
        {
            mockDirectory
                .Setup(dir => dir.EnumerateDirectories())
                .Returns(
                    this.mockFileSystem.DirectoryInfo.FromDirectoryName(fullPath).EnumerateDirectories());
        }

        if (throwOnFileEnumeration is not null)
        {
            mockDirectory
                .Setup(dir => dir.EnumerateFiles())
                .Throws(throwOnFileEnumeration);
        }
        else
        {
            mockDirectory
                .Setup(dir => dir.EnumerateFiles())
                .Returns(
                    this.mockFileSystem.DirectoryInfo.FromDirectoryName(fullPath).EnumerateFiles());
        }

        return mockDirectory;
    }

    private OnDemandScanOrchestrator GetDefaultFileScanner()
    {
        return new OnDemandScanOrchestrator(
            this.mockDataAccessProvider.Object,
            this.mockFileHasher.Object,
            this.mockLogger.Object);
    }

    private OnDemandScanOrchestrator GetFileScannerWithErroredFiles(
        IEnumerable<IFileFingerprint> errorFiles, Exception thrownException)
    {
        var looseMockDataAccessProvider = new Mock<IFileFingerprintRepository>();
        looseMockDataAccessProvider.Setup(dap =>
            dap.Add(It.IsIn(errorFiles))).Throws(thrownException);
        return new OnDemandScanOrchestrator(
            looseMockDataAccessProvider.Object,
            this.mockFileHasher.Object,
            this.mockLogger.Object);
    }
}
*/