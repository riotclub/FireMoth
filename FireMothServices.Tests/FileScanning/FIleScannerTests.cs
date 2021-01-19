// <copyright file="FileScannerTests.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using FireMothServices.DataAccess;
    using FireMothServices.DataAnalysis;
    using Moq;
    using RiotClub.FireMoth.Services.DataAccess;
    using Xunit;

    /*
     * Constructor:
     *  * IDataAccessProvider can't be null
     *      * Ctor_NullDataAccessProvider_ThrowsArgumentNullException
     *  * HashAlgorithm can't be null
     *      * Ctor_NullHashAlgorithm_ThrowsArgumentNullException
     *  * TextWriter can't be null
     *      * Ctor_NullTextWriter_ThrowsArgumentNullException
     * ScanDirectory:
     * * directory must be valid
     *      * ScanDirectory_NullDirectory_ThrowsArgumentNullException
     *      * ScanDirectory_InvalidDirectory_ReturnsScanFailureResult
     * * valid directory returns successful scan result
     *      * ScanDirectory_ValidDirectory_ReturnsScanSuccessResult
     * * valid empty directory returns successful scan result
     *      * ScanDirectory_EmptyDirectory_ReturnsScanSuccessResult
     * * valid empty directory does not add records to data access provider
     *      * ScanDirectory_EmptyDirectory_NoRecordsAddedToDataAccessProvider
     * - valid directory with files adds records for all files contained within directory to the
     *   data access provider
     *      * ScanDirectory_ValidDirectoryWithFiles_AddsFileRecordsToDataAccessProvider
     * - valid directory in recursive mode adds records for all files contained within directory,
     *   including subdirectories
     *      - ScanDirectory_RecursiveScan_AddsSubdirectoryFilesToDataAccessProvider
     *      - ScanDirectory_NonRecursiveScan_IgnoresSubdirectories
     */
    public class FileScannerTests : IDisposable
    {
        private readonly Mock<IDataAccessProvider> mockDataAccessProvider;

        private readonly Mock<SHA256> mockHashAlgorithm;

        private readonly Mock<IFileHasher> mockFileHasher;

        private readonly Mock<Stream> mockStream;

        private readonly StringWriter outputWriter;

        private readonly string tempDirectory;

        private readonly FileSystem testFileSystem;

        private bool disposed = false;

        public FileScannerTests()
        {
            this.mockDataAccessProvider = new Mock<IDataAccessProvider>();
            //this.mockHashAlgorithm = new Mock<HashAlgorithm>();
            this.mockHashAlgorithm = new Mock<SHA256>();

            this.mockStream = new Mock<Stream>();
            this.mockFileHasher = new Mock<IFileHasher>();
            this.mockFileHasher.Setup(hasher => hasher.ComputeHashFromStream(It.IsAny<Stream>()))
                .Returns(new byte[] { 0x20, 0x20, 0x20 });

            /*
            this.mockStream = new Mock<Stream>();
            this.mockHashAlgorithm
                .Setup(h => h.(this.mockStream.Object))
                .Returns(new byte[] { 0x20, 0x20, 0x20 });
            */
            /*
            this.mockDataAccessProvider
                .Setup(provider => provider.AddFileRecord())
            */
            this.testFileSystem = new FileSystem();

            this.outputWriter = new StringWriter(new StringBuilder());

            this.tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(this.tempDirectory);

            var tempFile = Path.Combine(this.tempDirectory, Path.GetRandomFileName());
            var fileContent = "I am the very model of a modern Major-General.";
            File.WriteAllText(tempFile, fileContent);
        }

        [Fact]
        public void Ctor_NullTextWriter_ThrowsArgumentNullException()
        {
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileScanner(
                    this.mockDataAccessProvider.Object, this.mockFileHasher.Object, null));
        }

        [Fact]
        public void Ctor_NullHashAlgorithm_ThrowsArgumentNullException()
        {
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileScanner(this.mockDataAccessProvider.Object, null, this.outputWriter));
        }

        [Fact]
        public void Ctor_NullDataAccessProvider_ThrowsArgumentNullException()
        {
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileScanner(null, this.mockFileHasher.Object, this.outputWriter));
        }

        [Fact]
        public void ScanDirectory_NullDirectory_ThrowsArgumentNullException()
        {
            // Arrange
            FileScanner fileScanner = new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockFileHasher.Object,
                this.outputWriter);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => fileScanner.ScanDirectory(null, false));
        }

        [Theory]
        [InlineData(@"C:\path/with|invalid/chars")]
        [InlineData(@"\\:\\||>\a\b::t<")]
        public void ScanDirectory_InvalidDirectory_ReturnsScanFailureResult(string directory)
        {
            // Arrange
            FileScanner fileScanner = new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockFileHasher.Object,
                this.outputWriter);
            var testDirectory = this.testFileSystem.DirectoryInfo.FromDirectoryName(directory);

            // Act, Assert
            Assert.Equal(ScanResult.ScanFailure, fileScanner.ScanDirectory(testDirectory, false));
        }

        [Fact]
        public void ScanDirectory_ValidDirectory_ReturnsScanSuccessResult()
        {
            // Arrange
            FileScanner fileScanner = new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockFileHasher.Object,
                this.outputWriter);
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\testdirectory\TestFile.txt", new MockFileData("000") },
                { @"c:\testdirectory\subdir\SubdirFileA", new MockFileData("111") },
                { @"c:\testdirectory\subdir\SubdirFileB", new MockFileData("222") },
            });

            var testDirectory = fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory");

            // Act
            ScanResult result = fileScanner.ScanDirectory(testDirectory, false);

            // Assert
            Assert.Equal(ScanResult.ScanSuccess, result);
        }

        [Fact]
        public void ScanDirectory_EmptyDirectory_ReturnsScanSuccessResult()
        {
            // Arrange
            FileScanner fileScanner = new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockFileHasher.Object,
                this.outputWriter);
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\testdirectory\subdir\SubdirFile.txt", new MockFileData("111") },
            });

            var testDirectory = fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory");

            // Act
            ScanResult result = fileScanner.ScanDirectory(testDirectory, false);

            // Assert
            Assert.Equal(ScanResult.ScanSuccess, result);
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

            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            var fileScanner = new FileScanner(
                mockDataAccessProvider.Object, this.mockFileHasher.Object, this.outputWriter);
            var mockFileFingerprint = new Mock<IFileFingerprint>();

            // Act
            var result = fileScanner.ScanDirectory(
                fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory"), false);

            // Assert
            mockDataAccessProvider.Verify(
                dap => dap.AddFileRecord(mockFileFingerprint.Object),
                Times.Never);

        }

        [Fact]
        public void ScanDirectory_ValidDirectoryWithFiles_AddsFileRecordsToDataAccessProvider()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\testdirectory\SomeFile.txt", new MockFileData("111") },
                { @"c:\testdirectory\AnotherFile.dat", new MockFileData("222") },
                { @"c:\testdirectory\YetAnotherFile.xml", new MockFileData("333") },
            });

            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            var fileScanner = new FileScanner(
                mockDataAccessProvider.Object, this.mockFileHasher.Object, this.outputWriter);

            // Act
            var result = fileScanner.ScanDirectory(
                fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory"), false);

            // Assert
            mockDataAccessProvider.Verify(
                dap => dap.AddFileRecord(It.IsAny<IFileFingerprint>()),
                Times.Exactly(fileSystem.AllFiles.Count()));
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

            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            var fileScanner = new FileScanner(
                mockDataAccessProvider.Object, this.mockFileHasher.Object, this.outputWriter);

            // Act
            fileScanner.ScanDirectory(
                fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory"), true);

            // Assert
            mockDataAccessProvider.Verify(dap =>
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

            var mockDataAccessProvider = new Mock<IDataAccessProvider>();
            var fileScanner = new FileScanner(
                mockDataAccessProvider.Object, this.mockFileHasher.Object, this.outputWriter);

            // Act
            fileScanner.ScanDirectory(
                fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory"), false);

            // Assert
            mockDataAccessProvider.Verify(
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
                this.outputWriter.Dispose();
                Directory.Delete(this.tempDirectory, true);
            }

            this.disposed = true;
        }
    }
}
