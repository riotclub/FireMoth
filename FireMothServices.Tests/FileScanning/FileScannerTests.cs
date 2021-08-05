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
     *      * ScanDirectory_ValidDirectoryWithFiles_AddsFileRecordsToDataAccessProvider
     * * Valid directory produces correct log events
     *      * ScanDirectory_ValidDirectory_LogsScanEvents
     * * Valid directory with errored files returns correct list of scanned files
     *      * ScanDirectory_ValidDirectoryWithErroredFiles_ReturnsCorrectScannedFiles
     * - Valid directory with skipped files returns correct list of skipped files
     *      * ScanDirectory_ValidDirectoryWithErroredFiles_ReturnsCorrectSkippedFiles
     * - Valid directory with errored files returns correct list of scan errors
     *      * ScanDirectory_ValidDirectoryWithErroredFiles_ReturnsCorrectScanErrors
     * - Valid directory with errored files produces log events for errored files
     *      * ScanDirectory_ValidDirectoryWithErroredFiles_LogsErrorEvents
     * - Does not attempt to add unscannable files to the data access provider
     * - Valid empty directory results in successful scan
     *      * ScanDirectory_EmptyDirectory_ReturnsScanSuccessResult
     * - Valid empty directory adds no records to data provider
     *      * ScanDirectory_EmptyDirectory_NoRecordsAddedToDataAccessProvider
     * - Invalid directory results in failed scan
     *      - ScanDirectory_InvalidIDirectoryInfo_ReturnsScanFailureResult
     * - Authorization error while attempt to access directory produces log event
     *      - ScanDirectory_AuthorizationErrorDuringDirectoryAccess_LogsError
     * - Authorization error while attempt to access directory increments skipped file count
     *      - ScanDirectory_AuthorizationErrorDuringDirectoryAccess_IncrementsSkippedFileCount
     * - Authorization error while attempt to access file produces log event
     *      - ScanDirectory_AuthorizationErrorDuringFileAccess_LogsError
     * - Authorization error while attempt to access file increments skipped file count
     *      - ScanDirectory_AuthorizationErrorDuringFileAccess_IncrementsSkippedFileCount
     * - Recursive scan option results in successful scan of all files and subdirectory files
     *      - ScanDirectory_ValidDirectoryWithRecursiveScan_AddsSubdirectoryFilesToDataAccessProvider
     * - Recursive scan returns proper count of scanned files
     *      - ScanDirectory_ValidDirectoryWithRecursiveScan_CountsScannedFilesCorrectly
     */
    [ExcludeFromCodeCoverage]
    public class FileScannerTests : IDisposable
    {
        private readonly Mock<IFileHasher> mockFileHasher;
        private readonly Mock<ILogger<FileScanner>> mockLogger;
        private readonly MockFileSystem mockFileSystem;
        private Mock<IDataAccessProvider> mockDataAccessProvider;

        private bool disposed = false;

        public FileScannerTests()
        {
            this.mockDataAccessProvider = new Mock<IDataAccessProvider>(MockBehavior.Strict);

            this.mockFileHasher = new Mock<IFileHasher>();
            this.mockFileHasher
                .Setup(hasher => hasher.ComputeHashFromStream(It.IsAny<Stream>()))
                .Returns(new byte[] { 0x20, 0x20, 0x20 });

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
        public void ScanDirectory_ValidDirectoryWithFiles_AddsFileRecordsToDataAccessProvider(
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
                    $"Scanning file {file.Name}...", LogLevel.Information);
            }
        }

        // ScanDirectory: Valid directory with errored files returns correct list of scanned files
        [Fact]
        public void ScanDirectory_ValidDirectoryWithErroredFiles_ReturnsCorrectScannedFiles()
        {
            // Arrange
            var testDirectory =
                this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\dirwithfiles");
            var files = testDirectory.EnumerateFiles().ToList();
            var errorFile = new FileFingerprint(
                files.Last(), Convert.ToBase64String(new byte[] { 0x20, 0x20, 0x20 }));
            files.Remove(files.Last());

            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            mockDataAccessProvider.Setup(dap =>
                dap.AddFileRecord(errorFile)).Throws<IOException>();

            var testFileScanner = new FileScanner(
                 mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);

            // Act
            var result = testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            foreach (var file in files)
            {
                Assert.Contains(file.FullName, result.ScannedFiles);
            }
        }

        // ScanDirectory: Valid directory with errored files returns correct list of skipped files
        [Fact]
        public void ScanDirectory_ValidDirectoryWithErroredFiles_ReturnsCorrectSkippedFiles()
        {
            // Arrange
            var testDirectory =
                this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\dirwithfiles");
            var files = testDirectory.EnumerateFiles();
            var errorFiles = files
                .Where(f => f.Name.Contains("eep"))
                .Select(f =>
                    new FileFingerprint(
                        f, Convert.ToBase64String(new byte[] { 0x20, 0x20, 0x20 })));
            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            mockDataAccessProvider.Setup(dap =>
                dap.AddFileRecord(It.IsIn(errorFiles))).Throws<IOException>();
            var testFileScanner = new FileScanner(
                 mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);

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
        public void ScanDirectory_ValidDirectoryWithErroredFiles_ReturnsCorrectScanErrors()
        {
            // Arrange
            var testDirectory =
                this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\dirwithfiles");
            var errorFiles = testDirectory.EnumerateFiles()
                .Where(f => f.Name.Contains("eep"))
                .Select(f =>
                    new FileFingerprint(
                        f, Convert.ToBase64String(new byte[] { 0x20, 0x20, 0x20 })));
            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            mockDataAccessProvider.Setup(dap =>
                dap.AddFileRecord(It.IsIn(errorFiles))).Throws<IOException>();
            var testFileScanner = new FileScanner(
                 mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);

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

        // ScanDirectory: Valid directory with errored files produces log events for skipped files
        [Fact]
        public void ScanDirectory_ValidDirectoryWithErroredFiles_LogsErrorEvents()
        {
            // Arrange
            var testDirectory =
                this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\dirwithfiles");
            var files = testDirectory.EnumerateFiles();
            var errorFiles = files
                .Where(f => f.Name.Contains("eep"))
                .Select(f =>
                    new FileFingerprint(
                        f, Convert.ToBase64String(new byte[] { 0x20, 0x20, 0x20 })));
            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            mockDataAccessProvider.Setup(dap =>
                dap.AddFileRecord(It.IsIn(errorFiles))).Throws<IOException>();
            var testFileScanner = new FileScanner(
                 mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);

            // Act
            testFileScanner.ScanDirectory(testDirectory, false);

            // Assert
            foreach (var file in errorFiles)
            {
                this.mockLogger.VerifyLogCalled(
                    $"Could not add record for file {file.FileInfo.FullName} (skipping): I/O error occurred.",
                    LogLevel.Error);
            }
        }

        // ScanDirectory: Valid empty directory adds no records to data provider
        [Fact]
        public void ScanDirectory_EmptyDirectory_NoRecordsAddedToDataAccessProvider()
        {
            // Arrange
            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            var fileScanner = new FileScanner(
                mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);
            var mockFileFingerprint = new Mock<IFileFingerprint>();

            // Act
            var result = fileScanner.ScanDirectory(
                this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\emptydir"), false);

            // Assert
            mockDataAccessProvider.Verify(
                dap => dap.AddFileRecord(mockFileFingerprint.Object),
                Times.Never);
        }

        // * - Authorization error while attempt to access directory produces log event
        // *      - ScanDirectory_AuthorizationErrorDuringDirectoryAccess_LogsError
        // * - Authorization error while attempt to access directory increments skipped file count
        // *      - ScanDirectory_AuthorizationErrorDuringDirectoryAccess_IncrementsSkippedFileCount
        // * - Authorization error while attempt to access file produces log event
        // *      - ScanDirectory_AuthorizationErrorDuringFileAccess_LogsError

        // * - Authorization error while attempt to access file increments skipped file count
        // *      - ScanDirectory_AuthorizationErrorDuringFileAccess_IncrementsSkippedFileCount
        [Fact]
        public void ScanDirectory_AuthorizationErrorDuringFileAccess_IncrementsSkippedFileCount()
        {
            // Arrange
            var testDirectory =
                this.mockFileSystem.DirectoryInfo.FromDirectoryName(@"c:\dirwithfiles");
            var files = testDirectory.EnumerateFiles().ToList();
            var errorFile = new FileFingerprint(
                files.Last(), Convert.ToBase64String(new byte[] { 0x20, 0x20, 0x20 }));
            this.mockDataAccessProvider = new Mock<IDataAccessProvider>();
            var testFileScanner = this.GetDefaultFileScanner();
            ScanResult result;

            using (this.mockFileSystem.File.Open(
                errorFile.FileInfo.FullName,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None))
            {
                // Act
                result = testFileScanner.ScanDirectory(testDirectory, false);
            }

            // Assert
            foreach (var file in files)
            {
                Assert.Contains(file.FullName, result.ScannedFiles);
            }
        }

        // * - Recursive scan returns proper count of scanned files
        // *      - ScanDirectory_ValidDirectoryWithRecursiveScan_CountsScannedFilesCorrectly

        // * - Invalid directory results in failed scan
        // *      - ScanDirectory_InvalidIDirectoryInfo_ReturnsScanFailureResult
        [Theory]
        [InlineData(@"C:\path/with|invalid/chars")]
        [InlineData(@"\\:\\||>\a\b::t<")]
        public void ScanDirectory_InvalidIDirectoryInfo_ReturnsScanFailureResult(string directory)
        {
            // Arrange
            var fileScanner = this.GetDefaultFileScanner();
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(directory);

            // Act
            var result = fileScanner.ScanDirectory(testDirectory, false);

            // Assert
            Assert.Empty(result.ScannedFiles);
        }

        // * - Valid empty directory results in successful scan
        // *      - ScanDirectory_EmptyDirectory_ReturnsScanSuccessResult
        [Fact]
        public void ScanDirectory_EmptyDirectory_ReturnsScanSuccessResult()
        {
            // Arrange
            var fileScanner = this.GetDefaultFileScanner();
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\testdirectory\subdir\SubdirFile.txt", new MockFileData("111") },
            });
            // this.mockFileSystem.

            var testDirectory = fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory");

            // Act
            ScanResult result = fileScanner.ScanDirectory(testDirectory, false);

            // Assert
            // Assert.True(result.Success);
        }

        // * - Recursive scan option results in successful scan of all files and subdirectory files
        // *      - ScanDirectory_ValidDirectoryWithRecursiveScan_AddsSubdirectoryFilesToDataAccessProvider
        [Theory]
        [InlineData(@"c:\testdirectory\test\testfile")]
        [InlineData(@"c:\testdirectory\subdirectoryA\testsubdirFile.xml")]
        [InlineData(@"c:\testdirectory\subdirectoryA\nestedsubdir\nestedsubdirfile")]
        [InlineData(@"c:\testdirectory\subdirectoryB\000.txt")]
        public void ScanDirectory_RecursiveScan_AddsSubdirectoryFilesToDataAccessProvider(
            string subdirectoryFile)
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\testdirectory\SomeFile.txt", new MockFileData("111") },
                { @"c:\testdirectory\AnotherFile.dat", new MockFileData("222") },
            });
            MockFileInfo mockFileInfo = new MockFileInfo(fileSystem, subdirectoryFile);
            fileSystem.AddFile(mockFileInfo.FullName, new MockFileData("000"));

            var mockDap = new Mock<IDataAccessProvider>();
            var fileScanner = new FileScanner(
                mockDap.Object, this.mockFileHasher.Object, this.mockLogger.Object);

            // Act
            fileScanner.ScanDirectory(
                fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory"), true);

            // Assert
            mockDap.Verify(dap =>
                dap.AddFileRecord(
                    It.Is<IFileFingerprint>(file =>
                        file.FileInfo.Name.Equals(
                            Path.GetFileName(subdirectoryFile),
                            StringComparison.OrdinalIgnoreCase))));
        }

        [Fact]
        public void ScanDirectory_NonRecursiveScan_IgnoresSubdirectories()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\testdirectory\SomeFile.txt", new MockFileData("111") },
                { @"c:\testdirectory\AnotherFile.dat", new MockFileData("222") },
                { @"c:\testdirectory\subdirectory\A.xml", new MockFileData("333") },
            });

            var mockDap = new Mock<IDataAccessProvider>();
            var fileScanner = new FileScanner(
                mockDap.Object, this.mockFileHasher.Object, this.mockLogger.Object);

            // Act
            fileScanner.ScanDirectory(
                fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory"), false);

            // Assert
            mockDap.Verify(
                dap => dap.AddFileRecord(
                    It.Is<IFileFingerprint>(file =>
                        file.FileInfo.DirectoryName.StartsWith(
                            @"c:\testdirectory\subdirectory", StringComparison.OrdinalIgnoreCase))),
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
                });

            // var file = new MockFileData("asdf").AccessControl.SetAccessRule;

            mockFileSystem.AddDirectory(@"c:\emptydir");
            mockFileSystem.AddDirectory(@"c:\dirwithfiles\emptysubdir");

            return mockFileSystem;
        }

        private FileScanner GetDefaultFileScanner()
        {
            return new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockFileHasher.Object,
                this.mockLogger.Object);
        }

        private FileScanner GetFileScannerWithSkippedFile(IDirectoryInfo testDirectory)
        {
            var files = testDirectory.EnumerateFiles();
            var errorFile = new FileFingerprint(
                files.ToList().Last(), Convert.ToBase64String(new byte[] { 0x20, 0x20, 0x20 }));
            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            mockDataAccessProvider.Setup(dap =>
                dap.AddFileRecord(errorFile)).Throws<IOException>();

            return new FileScanner(
                 mockDataAccessProvider.Object, this.mockFileHasher.Object, this.mockLogger.Object);
        }

    }
}
