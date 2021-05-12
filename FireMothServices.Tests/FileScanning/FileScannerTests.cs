// <copyright file="FileScannerTests.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Abstractions.TestingHelpers;
    using Microsoft.Extensions.Logging;
    using Moq;
    using RiotClub.FireMoth.Services.DataAccess;
    using RiotClub.FireMoth.Services.DataAnalysis;
    using RiotClub.FireMoth.Services.FileScanning;
    using Xunit;

    /*
     * Constructor:
     *  * Null IDataAccessProvider throws exception
     *      * Ctor_NullIDataAccessProvider_ThrowsArgumentNullException
     *  * Null IFileHasher throws exception
     *      * Ctor_NullIFileHasher_ThrowsArgumentNullException
     *  * Null ILogger throws exception
     *      * Ctor_NullILogger_ThrowsArgumentNullException
     *
     * ScanDirectory:
     * * Null IDirectoryInfo throws exception
     *      * ScanDirectory_NullIDirectoryInfo_ThrowsArgumentNullException
     * * Valid directory results in successful scan
     *      * ScanDirectory_ValidDirectory_ReturnsScanSuccessResult
     * * Valid directory adds file fingerprint records to data provider
     *      * ScanDirectory_ValidDirectoryWithFiles_AddsFileRecordsToDataAccessProvider
     * - Valid directory returns proper count of scanned files
     *      - ScanDirectory_ValidDirectory_ReturnsProperScannedFileCount
     * - Valid directory produces correct log events
     *      - ScanDirectory_ValidDirectory_LogsScanEvents
     * - Valid directory with skipped files returns proper count of scanned files
     *      - ScanDirectory_ValidDirectoryWithSkippedFiles_CountsScannedFilesCorrectly
     * - Valid directory with skipped files returns proper count of scanned files
     *      - ScanDirectory_ValidDirectoryWithSkippedFiles_CountsSkippedFilesCorrectly
     * - Valid directory with skipped files produces log events for skipped files
     *      - ScanDirectory_ValidDirectoryWithSkippedFiles_LogsSkippedFileScanEvents
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
     * - Call on disposed object throws exception
     *      - AddFileRecord_DisposedObject_ThrowsObjectDisposedException
     */
    [ExcludeFromCodeCoverage]
    public class FileScannerTests : IDisposable
    {
        private readonly Mock<IDataAccessProvider> mockDataAccessProvider;
        private readonly Mock<IFileHasher> mockFileHasher;
        // private readonly FileSystem testFileSystem;
        private readonly Mock<ILogger<FileScanner>> mockLogger;
        private readonly MockFileSystem mockFileSystem;

        private bool disposed = false;

        public FileScannerTests()
        {
            this.mockDataAccessProvider = new Mock<IDataAccessProvider>(MockBehavior.Strict);
            this.mockFileHasher = new Mock<IFileHasher>();
            this.mockFileHasher.Setup(hasher =>
                hasher
                    .ComputeHashFromStream(It.IsAny<Stream>()))
                    .Returns(new byte[] { 0x20, 0x20, 0x20 });

            // this.testFileSystem = new FileSystem();

            this.mockFileSystem = BuildMockFileSystem();

            // Mock.Get(this.mockFileSystem).Setup(fs => fs.DirectoryInfo).Throws<SecurityException>();
            this.mockLogger = new Mock<ILogger<FileScanner>>();
        }

        // *
        [Fact]
        public void Ctor_NullIDataAccessProvider_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileScanner(null, this.mockFileHasher.Object, this.mockLogger.Object));
        }

        // *
        [Fact]
        public void Ctor_NullIFileHasher_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileScanner(this.mockDataAccessProvider.Object, null, this.mockLogger.Object));
        }

        // *
        [Fact]
        public void Ctor_NullILogger_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileScanner(
                    this.mockDataAccessProvider.Object, this.mockFileHasher.Object, null));
        }

        // *
        [Fact]
        public void ScanDirectory_NullIDirectoryInfo_ThrowsArgumentNullException()
        {
            // Arrange
            var fileScanner = this.GetTestFileScanner();

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => fileScanner.ScanDirectory(null, false));
        }

        // *
        [Fact]
        public void ScanDirectory_ValidDirectory_ReturnsScanSuccessResult()
        {
            // Arrange
            var fileScanner = this.GetTestFileScanner();
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(
                @"c:\dirwithfiles");

            // Act
            ScanResult result = fileScanner.ScanDirectory(testDirectory, false);

            // Assert
            // Assert.True(result.Success);
        }

        // *
        [Theory]
        [InlineData(@"c:\dirwithfiles\")]
        [InlineData(@"c:\dirwithfiles\subdirwithfiles")]
        public void ScanDirectory_ValidDirectoryWithFiles_AddsFileRecordsToDataAccessProvider(
            string directory)
        {
            // Arrange
            var fileScanner = this.GetTestFileScanner();
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(directory);
            var files = testDirectory.EnumerateFiles();
            foreach (var file in files)
            {
                this.mockDataAccessProvider.Setup(dap =>
                    dap.AddFileRecord(It.Is<IFileFingerprint>(ff => ff.Name == file.Name)));
            }

            // Act
            fileScanner.ScanDirectory(testDirectory, false);

            // Assert
            this.mockDataAccessProvider.Verify();
        }

        [Theory]
        [InlineData(@"C:\path/with|invalid/chars")]
        [InlineData(@"\\:\\||>\a\b::t<")]
        public void ScanDirectory_InvalidDirectory_ReturnsScanFailureResult(string directory)
        {
            // Arrange
            var fileScanner = this.GetTestFileScanner();
            var testDirectory = this.mockFileSystem.DirectoryInfo.FromDirectoryName(directory);

            // Act, Assert
            // Assert.False(fileScanner.ScanDirectory(testDirectory, false).Success);
        }

        [Fact]
        public void ScanDirectory_EmptyDirectory_ReturnsScanSuccessResult()
        {
            // Arrange
            var fileScanner = this.GetTestFileScanner();
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

        [Fact]
        public void ScanDirectory_EmptyDirectory_NoRecordsAddedToDataAccessProvider()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\testdirectory\subdirA\SubdirFile.txt", new MockFileData("111") },
                { @"c:\testdirectory\subdirB\SubdirFile.txt", new MockFileData("222") },
            });

            var mockDap = new Mock<IDataAccessProvider>();
            var fileScanner = new FileScanner(
                mockDap.Object, this.mockFileHasher.Object, this.mockLogger.Object);
            var mockFileFingerprint = new Mock<IFileFingerprint>();

            // Act
            var result = fileScanner.ScanDirectory(
                fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory"), false);

            // Assert
            mockDap.Verify(
                dap => dap.AddFileRecord(mockFileFingerprint.Object),
                Times.Never);
        }

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
                        file.Name.Equals(Path.GetFileName(subdirectoryFile), StringComparison.OrdinalIgnoreCase))));
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
                        file.DirectoryName.StartsWith(
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
                    { @"c:\dirwithfiles\subdirwithfiles\SubdirFileA.1", new MockFileData("333") },
                    { @"c:\dirwithfiles\subdirwithfiles\SubdirFileB.2", new MockFileData("444") },
                });

            mockFileSystem.AddDirectory(@"c:\emptydir");
            mockFileSystem.AddDirectory(@"c:\dirwithfiles\emptysubdir");

            return mockFileSystem;
        }

        private FileScanner GetTestFileScanner()
        {
            return new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockFileHasher.Object,
                this.mockLogger.Object);
        }

    }
}
