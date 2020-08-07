// <copyright file="FileScannerTests.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions.TestingHelpers;
    using System.Security.Cryptography;
    using System.Text;
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
     * - valid directory returns successful scan result
     *      - ScanDirectory_ValidDirectory_ReturnsScanSuccessResult
     * - valid empty directory returns successful scan result
     *      - ScanDirectory_EmptyDirectory_ReturnsScanSuccessResult
     * - valid empty directory does not add records to data access provider
     *      - ScanDirectory_EmptyDirectory_NoRecordsAddedToDataAccessProvider
     * - valid directory with files adds records for all files contained within directory to the
     *   data access provider
     *      - ScanDirectory_ValidDirectoryWithFiles_AddsFileRecordsToDataAccessProvider
     * - valid directory in recursive mode adds records for all files contained within directory,
     *   including subdirectories
     *      - ScanDirectory_RecursiveScan_AddsSubdirectoryFilesToDataAccessProvider
     */
    public class FileScannerTests : IDisposable
    {
        private readonly Mock<IDataAccessProvider> mockDataAccessProvider;

        private readonly Mock<HashAlgorithm> mockHashAlgorithm;

        private readonly StringWriter outputWriter;

        private readonly IFileScanner defaultFileScanner;

        private readonly string tempDirectory;

        private bool disposed = false;

        public FileScannerTests()
        {
            this.mockDataAccessProvider = new Mock<IDataAccessProvider>();
            this.mockHashAlgorithm = new Mock<HashAlgorithm>();
            /*
            this.mockDataAccessProvider
                .Setup(provider => provider.AddFileRecord())
            */

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
                    this.mockDataAccessProvider.Object, this.mockHashAlgorithm.Object, null));
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
                new FileScanner(null, this.mockHashAlgorithm.Object, this.outputWriter));
        }

        [Fact]
        public void ScanDirectory_NullDirectory_ThrowsArgumentNullException()
        {
            // Arrange
            FileScanner fileScanner = new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockHashAlgorithm.Object,
                this.outputWriter);
            string nullStr = null;

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => fileScanner.ScanDirectory(nullStr));
        }

        [Theory]
        [InlineData(@"C:\path/with|invalid/chars")]
        [InlineData(@"\\:\\||>\a\b::t<")]
        public void ScanDirectory_InvalidDirectory_ReturnsScanFailureResult(string directory)
        {
            // Arrange
            FileScanner fileScanner = new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockHashAlgorithm.Object,
                this.outputWriter);

            // Act, Assert
            Assert.Equal(ScanResult.ScanFailure, fileScanner.ScanDirectory(directory));
        }

        [Fact]
        public void ScanDirectory_ValidDirectory_ReturnsScanSuccessResult()
        {
            // Arrange
            FileScanner fileScanner = new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockHashAlgorithm.Object,
                this.outputWriter);
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\testdirectory\TestFile.txt", new MockFileData("000") },
                { @"c:\testdirectory\subdir\SubdirFileA", new MockFileData("111") },
                { @"c:\testdirectory\subdir\SubdirFileB", new MockFileData("222") },
            });

            var testDirectory = fileSystem.DirectoryInfo.FromDirectoryName(@"c:\testdirectory");

            // Act
            ScanResult result = fileScanner.ScanDirectory(testDirectory);

            // Assert
            Assert.Equal(ScanResult.ScanSuccess, result);
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
