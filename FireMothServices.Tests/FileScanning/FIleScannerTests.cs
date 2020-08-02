// <copyright file="FileScannerTests.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Moq;
    using RiotClub.FireMoth.Services.DataAccess;
    using Xunit;

    /*
     * Constructor:
     *  - IDataAccessProvider can't be null
     *      - Ctor_NullDataAccessProvider_ThrowsArgumentNullException
     *  - HashAlgorithm can't be null
     *      - Ctor_NullHasher_ThrowsArgumentNullException
     *  - TextWriter can't be null
     *      - Ctor_NullOutputWriter_ThrowsArgumentNullException
     * ScanDirectory:
     * - directory can't be null
     *      - ScanDirectory_NullDirectory_ThrowsArgumentNullException
     * - directory must be valid
     *      - ScanDirectory_InvalidDirectory_ReturnsScanFailureResult
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

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => fileScanner.ScanDirectory(null));
        }

        [Fact]
        public void ScanDirectory_ReturnsScanSuccessScanResult()
        {
            // Arrange
            FileScanner fileScanner = new FileScanner(
                this.mockDataAccessProvider.Object,
                this.mockHashAlgorithm.Object,
                this.outputWriter);

            // Act
            ScanResult result = fileScanner.ScanDirectory(this.tempDirectory);

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
