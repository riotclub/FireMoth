// <copyright file="FileScannerTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.FileScanning
{
    using Microsoft.Extensions.Logging;
    using Moq;
    using RiotClub.FireMoth.Services.DataAccess;
    using RiotClub.FireMoth.Services.DataAnalysis;
    using RiotClub.FireMoth.Services.FileScanning;
    using RiotClub.FireMoth.Services.Tests.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Linq;
    using Xunit;

    /*
     * Constructor:
     * * Null IDataAccessProvider throws exception
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
    [ExcludeFromCodeCoverage]
    public class FileScannerTests : IDisposable
    {
        private readonly Mock<IFileHasher> mockFileHasher;
        private readonly Mock<ILogger<FileScanner>> mockLogger;
        private readonly MockFileSystem mockFileSystem;
        private Mock<IDataAccessProvider> mockDataAccessProvider;

        private byte[] testHashData = new byte[] { 0x20, 0x20, 0x20 };

        private bool disposed = false;

        public FileScannerTests()
        {
            this.mockDataAccessProvider = new Mock<IDataAccessProvider>(MockBehavior.Strict);

            this.mockFileHasher = new Mock<IFileHasher>();
            this.mockFileHasher
                .Setup(hasher => hasher.ComputeHashFromStream(It.IsAny<Stream>()))
                .Returns(this.testHashData);

            this.mockFileSystem = BuildMockFileSystem();

            this.mockLogger = new Mock<ILogger<FileScanner>>();
        }

        // Ctor: Null IDataAccessProvider throws exception
        [Fact]
        public void Ctor_NullIDataAccessProvider_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileScanner(null, this.mockFileHasher.Object, this.mockLogger.Object));
        }

        // Ctor: Null IFileHasher throws exception
        [Fact]
        public void Ctor_NullIFileHasher_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileScanner(this.mockDataAccessProvider.Object, null, this.mockLogger.Object));
        }

        // Ctor: Null ILogger throws exception
        [Fact]
        public void Ctor_NullILogger_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileScanner(
                    this.mockDataAccessProvider.Object, this.mockFileHasher.Object, null));
        }

        // ScanDirectory: Null IDirectoryInfo throws exception
        [Fact]
        public void ScanDirectory_NullIDirectoryInfo_ThrowsArgumentNullException()
        {
            // Arrange
            var fileScanner = this.GetDefaultFileScanner();

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => fileScanner.ScanDirectory(null, false));
        }

        // ScanDirectory: Valid directory adds file fingerprint records to data provider
        [Theory]
        [InlineData(@"c:\dirwithfiles\")]
        [InlineData(@"c:\dirwithfiles\subdirwithfiles")]
        public void ScanDirectory_ValidDirectory_AddsFileRecordsToDataAccessProvider(
            string directory)
        {
            // Arrange
            var fileScanner = this.GetDefaultFileScanner();
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(directory);
            var files = testDirectory.EnumerateFiles();

            foreach (var file in files)
            {
                this.mockDataAccessProvider
                    .Setup(dap =>
                        dap.AddFileRecord(It.Is<IFileFingerprint>(ff =>
                            ff.FileInfo.Name == file.Name)));
            }

            // Act
            fileScanner.ScanDirectory(testDirectory, false);

            // Assert
            this.mockDataAccessProvider.VerifyAll();
        }

        // ScanDirectory: Valid directory produces correct log events
        [Fact]
        public void ScanDirectory_ValidDirectory_LogsScanEvents()
        {
            // Arrange
            var testDirectory =
                this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\dirwithfiles");
            this.mockDataAccessProvider.Setup(dap =>
                dap.AddFileRecord(It.IsAny<IFileFingerprint>()));
            var files = testDirectory.EnumerateFiles();

            // Act
            this.GetDefaultFileScanner().ScanDirectory(testDirectory, false);

            // Assert
            foreach (var file in files)
            {
                this.mockLogger.VerifyLogCalled(
                    $"Scanning file '{file.Name}'...", LogLevel.Information);
            }
        }

        // ScanDirectory: Valid directory with errored files returns correct list of scanned files
        [Fact]
        public void ScanDirectory_DirectoryWithErroredFiles_ReturnsCorrectScannedFiles()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");
            var errorFiles = this.GetFileFingerprints(testDirectory, "eep");
            var expectedFiles = testDirectory.EnumerateFiles().ToList();
            expectedFiles.RemoveAll(fileInfo =>
                    errorFiles.Contains(
                        new FileFingerprint(fileInfo, Convert.ToBase64String(this.testHashData))));
            var testFileScanner = this.GetFileScannerWithErroredFiles(errorFiles, new IOException());

            // Act
            var result = testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            foreach (var expectedFile in expectedFiles)
            {
                Assert.Contains(expectedFile.FullName, result.ScannedFiles);
            }
        }

        // ScanDirectory: Valid directory with errored files returns correct list of skipped files
        [Fact]
        public void ScanDirectory_DirectoryWithErroredFiles_ReturnsCorrectSkippedFiles()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");
            var errorFiles = this.GetFileFingerprints(testDirectory, "eep");
            var testFileScanner = this.GetFileScannerWithErroredFiles(errorFiles, new IOException());

            // Act
            var result = testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            Assert.Equal(errorFiles.Count(), result.SkippedFiles.Count);
            foreach (var file in errorFiles)
            {
                Assert.Contains(
                    file.FileInfo.FullName, (IDictionary<string, string>)result.SkippedFiles);
            }
        }

        // ScanDirectory: Valid directory with errored files returns correct list of scan errors
        [Fact]
        public void ScanDirectory_DirectoryWithErroredFiles_ReturnsCorrectScanErrors()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");
            var errorFiles = this.GetFileFingerprints(testDirectory, "eep");
            var testFileScanner = this.GetFileScannerWithErroredFiles(errorFiles, new IOException());

            // Act
            var scanResult = testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            var resultErrorFiles = scanResult.Errors.Select(e => e.Path);
            Assert.Equal(errorFiles.Count(), resultErrorFiles.Count());
            foreach (var errorFile in errorFiles)
            {
                Assert.Contains(errorFile.FileInfo.FullName, resultErrorFiles);
            }
        }

        // ScanDirectory: Valid directory with errored files produces log events for errored files
        [Fact]
        public void ScanDirectory_DirectoryWithErroredFiles_LogsErrorEvents()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");
            var errorFiles = this.GetFileFingerprints(testDirectory, "eep");
            var testFileScanner = this.GetFileScannerWithErroredFiles(errorFiles, new IOException());

            // Act
            testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            foreach (var file in errorFiles)
            {
                this.mockLogger.VerifyLogCalled(
                    $"Could not add record for file '{file.FileInfo.FullName}': I/O error occurred.; skipping file.",
                    LogLevel.Error);
            }
        }

        // ScanDirectory: Does not attempt to add unscannable files to the data access provider
        [Fact]
        public void ScanDirectory_DirectoryWithErroredFiles_NoErroredFilesAddedToDataAccessProvider()
        {
            // Arrange
            var testDirectory =
                this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\dirwithfiles");
            var errorFiles = this.GetFileFingerprints(testDirectory, "AnotherFile");
            foreach (var errorFile in errorFiles)
            {
                this.mockFileSystem.GetFile(errorFile.FileInfo.FullName).AllowedFileShare
                    = FileShare.None;
            }

            var mockDataAccessProvider = new Mock<IDataAccessProvider>(MockBehavior.Loose);
            mockDataAccessProvider.Setup(dap =>
                dap.AddFileRecord(It.IsAny<IFileFingerprint>()));
            var testFileScanner = new FileScanner(
                mockDataAccessProvider.Object,
                this.mockFileHasher.Object,
                this.mockLogger.Object);

            // Act
            var scanResult = testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            foreach (var errorFile in errorFiles)
            {
                mockDataAccessProvider.Verify(
                    dap => dap.AddFileRecord(errorFile), Times.Never);
            }
        }

        // ScanDirectory: Valid empty directory results in successful scan
        [Fact]
        public void ScanDirectory_EmptyDirectory_ReturnsCorrectScanResult()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\emptydir");

            // Act
            var result = this.GetDefaultFileScanner().ScanDirectory(testDirectory, false);

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
            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            var fileScanner = new FileScanner(
                mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);

            // Act
            var result = fileScanner.ScanDirectory(
                this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\emptydir"), false);

            // Assert
            mockDataAccessProvider.Verify(
                dap => dap.AddFileRecord(It.IsAny<IFileFingerprint>()), Times.Never);
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
            var result = this.GetDefaultFileScanner().ScanDirectory(testDirectory, true);

            // Assert
            Assert.Empty(result.ScannedFiles);
            Assert.Empty(result.SkippedFiles);
            // Assert.Single(result.Errors);
            Assert.Collection(
                result.Errors,
                subdirError =>
                {
                    Assert.Equal(testDirectory.FullName, subdirError.Path);
                    Assert.StartsWith(
                        "Could not enumerate subdirectories of directory", subdirError.Message);
                    Assert.True(
                        subdirError.Exception.GetType() == typeof(NotSupportedException)
                        || subdirError.Exception.GetType() == typeof(ArgumentException));
                },
                fileError =>
                {
                    Assert.Equal(testDirectory.FullName, fileError.Path);
                    Assert.StartsWith("Could not enumerate files of directory", fileError.Message);
                    Assert.True(
                        fileError.Exception.GetType() == typeof(NotSupportedException)
                        || fileError.Exception.GetType() == typeof(ArgumentException));
                });
        }

        // ScanDirectory: Access to scan directory denied returns correct scan result
        [Fact]
        public void ScanDirectory_DirectoryAccessDenied_ReturnsCorrectScanResult()
        {
            // Arrange
            var testDirectory = @"c:\dirwithfiles";
            var mockDirectory = new Mock<IDirectoryInfo>();
            mockDirectory.SetupGet(dir => dir.FullName).Returns(testDirectory);
            mockDirectory
                .Setup(dir => dir.EnumerateDirectories())
                .Throws<UnauthorizedAccessException>();
            mockDirectory
                .Setup(dir => dir.EnumerateFiles())
                .Throws<UnauthorizedAccessException>();

            // Act
            var result = this.GetDefaultFileScanner().ScanDirectory(mockDirectory.Object, true);

            // Assert
            Assert.Empty(result.ScannedFiles);
            Assert.Empty(result.SkippedFiles);
            Assert.Collection(
                result.Errors,
                errorOne =>
                {
                    Assert.Equal(testDirectory, errorOne.Path);
                    Assert.StartsWith(
                        $"Could not enumerate subdirectories of directory '{testDirectory}'",
                        errorOne.Message);
                    Assert.IsType<UnauthorizedAccessException>(errorOne.Exception);
                },
                errorTwo =>
                {
                    Assert.Equal(testDirectory, errorTwo.Path);
                    Assert.StartsWith(
                        $"Could not enumerate files of directory '{testDirectory}'",
                        errorTwo.Message);
                    Assert.IsType<UnauthorizedAccessException>(errorTwo.Exception);
                });
        }

        // ScanDirectory: Access to file denied produces log event
        [Fact]
        public void ScanDirectory_FileAccessDenied_LogsErrorEvent()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");
            var errorFiles = this.GetFileFingerprints(testDirectory, "eep");
            var testFileScanner = this.GetFileScannerWithErroredFiles(
                errorFiles, new UnauthorizedAccessException());

            // Act
            testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            foreach (var file in errorFiles)
            {
                this.mockLogger.VerifyLogCalled(
                    $"Could not add record for file '{file.FileInfo.FullName}': Attempted to " +
                        $"perform an unauthorized operation.; skipping file.",
                    LogLevel.Error);
            }
        }

        // ScanDirectory: Access to file denied adds file to skipped files
        [Fact]
        public void ScanDirectory_FileAccessDenied_ReturnsCorrectSkippedFiles()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");
            var errorFiles = this.GetFileFingerprints(testDirectory, "eep");
            var testFileScanner = this.GetFileScannerWithErroredFiles(
                errorFiles, new UnauthorizedAccessException());

            // Act
            var result = testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            Assert.Equal(errorFiles.Count(), result.SkippedFiles.Count);
            foreach (var file in errorFiles)
            {
                Assert.Contains(
                    file.FileInfo.FullName, (IDictionary<string, string>)result.SkippedFiles);
            }
        }

        // ScanDirectory: Access to file denied adds file to errored files
        [Fact]
        public void ScanDirectory_FileAccessDenied_ReturnsCorrectScanErrors()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");
            var errorFiles = this.GetFileFingerprints(testDirectory, "eep");
            var testFileScanner = this.GetFileScannerWithErroredFiles(
                errorFiles, new UnauthorizedAccessException());

            // Act
            var scanResult = testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            var resultErrorFiles = scanResult.Errors.Select(e => e.Path);
            Assert.Equal(errorFiles.Count(), resultErrorFiles.Count());
            foreach (var errorFile in errorFiles)
            {
                Assert.Contains(errorFile.FileInfo.FullName, resultErrorFiles);
            }
        }

        // ScanDirectory: Recursive scan option results in successful scan of all files and subdirectory files
        [Fact]
        public void ScanDirectory_RecursiveScan_AddsSubdirectoryFilesToDataAccessProvider()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");
            this.mockDataAccessProvider = new Mock<IDataAccessProvider>();

            // Act
            var scanResult = this.GetDefaultFileScanner().ScanDirectory(testDirectory, true);

            // Assert
            var expectedFiles = this.mockFileSystem.AllFiles.ToList();
            foreach (var file in expectedFiles)
            {
                this.mockDataAccessProvider.Verify(dap =>
                    dap.AddFileRecord(It.Is<IFileFingerprint>(fingerprint =>
                        fingerprint.FileInfo.FullName.Equals(
                            file, StringComparison.OrdinalIgnoreCase))));
            }
        }

        // ScanDirectory: Non-recursive scan option ignores subdirectories
        [Fact]
        public void ScanDirectory_NonRecursiveScan_IgnoresSubdirectories()
        {
            // Arrange
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");
            this.mockDataAccessProvider = new Mock<IDataAccessProvider>();

            // Act
            var scanResult = this.GetDefaultFileScanner().ScanDirectory(testDirectory, false);

            // Assert
            this.mockDataAccessProvider.Verify(
                dap => dap.AddFileRecord(
                    It.Is<IFileFingerprint>(file =>
                        file.FileInfo.DirectoryName.StartsWith(
                            @"c:\dirwithfiles\subdirwithfiles", StringComparison.OrdinalIgnoreCase))),
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

        private IEnumerable<IFileFingerprint> GetFileFingerprints(
            IDirectoryInfo directory, string fileNameFilter)
        {
            return directory.EnumerateFiles()
                .Where(fileInfo => fileInfo.Name.Contains(fileNameFilter))
                .Select(fileInfo =>
                    new FileFingerprint(
                        fileInfo, Convert.ToBase64String(new byte[] { 0x20, 0x20, 0x20 })));
        }

        private FileScanner GetDefaultFileScanner()
        {
            return new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockFileHasher.Object,
                this.mockLogger.Object);
        }

        private FileScanner GetFileScannerWithErroredFiles(
            IEnumerable<IFileFingerprint> errorFiles, Exception exception)
        {
            var looseMockDataAccessProvider = new Mock<IDataAccessProvider>();
            looseMockDataAccessProvider.Setup(dap =>
                dap.AddFileRecord(It.IsIn(errorFiles))).Throws(exception);
            return new FileScanner(
                looseMockDataAccessProvider.Object,
                this.mockFileHasher.Object,
                this.mockLogger.Object);
        }

    //    var testDirectory =
    //this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\dirwithfiles");
    //    var files = testDirectory.EnumerateFiles().ToList();
    //    var errorFile = new FileFingerprint(
    //        files.Last(), Convert.ToBase64String(this.testHashData));
    //    files.Remove(files.Last());

    //        var mockDataAccessProvider = new Mock<IDataAccessProvider>();
    //    mockDataAccessProvider.Setup(dap =>
    //            dap.AddFileRecord(errorFile)).Throws<IOException>();

    //        var testFileScanner = new FileScanner(
    //             mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);
    }
}
