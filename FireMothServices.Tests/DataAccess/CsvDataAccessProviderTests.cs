// <copyright file="CsvDataAccessProviderTests.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using FireMothServices.DataAccess;
    using Moq;
    using RiotClub.FireMoth.Services.DataAccess;
    using Xunit;

    /*
     * Ctor:
     *  - TextWriter can't be null
     *      * Ctor_NullTextWriter_ThrowsArgumentNullException
     * AddFileRecord:
     * - IFileFingerprint can't be null
     *      * AddFileRecord_NullFileFingerprint_ThrowsArgumentNullException
     * - File with commas in name adds file with quotes to backing store
     *      * AddFileRecord_FileWithCommas_AddsRecordWithQuotedFile
     * - Valid IFileFingerprint adds record to backing store
     *      * AddFileRecord_ValidFileFingerprint_AddsRecordToStore
     */
    public class CsvDataAccessProviderTests : IDisposable
    {
        private readonly Mock<IFileInfo> mockFileInfo;
        private readonly Mock<TextWriter> mockDefaultStreamWriter;
        private readonly string testFilePath = @"C:\TestDir";
        private readonly string testFileName = "TestFile.txt";
        private bool disposed;

        public CsvDataAccessProviderTests()
        {
            this.mockFileInfo = new Mock<IFileInfo>();
            this.mockFileInfo
                .SetupGet(mock => mock.FullName)
                .Returns(this.testFilePath + Path.DirectorySeparatorChar + this.testFileName);
            this.mockFileInfo
                .SetupGet(mock => mock.Name)
                .Returns(this.testFileName);

            this.mockDefaultStreamWriter = new Mock<TextWriter>();
            this.mockDefaultStreamWriter.Setup(mock => mock.WriteLine("result from test"));
        }

        [Fact]
        public void Ctor_NullTextWriter_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() => new CsvDataAccessProvider(null));
        }

        [Fact]
        public void AddFileRecord_NullFileFingerprint_ThrowsArgumentNullException()
        {
            // Arrange
            using CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.mockDefaultStreamWriter.Object);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => testObject.AddFileRecord(null));
        }

        [Fact]
        public void AddFileRecord_FileWithCommas_AddsRecordWithQuotedFile()
        {
            var testFullPath = @"C:\dir, with, commas\file, with, commas.dat";
            var testHash = "XdGu4hg63jhhgd84UFNM/38956NDJDIlrsMVY2jio38=";

            var testPath = Path.GetDirectoryName(testFullPath);
            var testPathWithQuotes = '"' + testPath + '"';
            var testFileName = Path.GetFileName(testFullPath);
            var testFileNameWithQuotes = '"' + testFileName + '"';

            // Arrange
            var mockFileInfo = new Mock<IFileInfo>();
            mockFileInfo.SetupGet(mock => mock.DirectoryName).Returns(testPath);
            mockFileInfo.SetupGet(mock => mock.FullName).Returns(testFullPath);
            mockFileInfo.SetupGet(mock => mock.Name).Returns(testFileName);

            var mockStreamWriter = new Mock<TextWriter>();

            using (CsvDataAccessProvider testobject =
                new CsvDataAccessProvider(mockStreamWriter.Object))
            {
                // Act
                testobject.AddFileRecord(new FileFingerprint(mockFileInfo.Object, testHash));
            }

            // Assert
            mockStreamWriter.Verify(writer => writer.Write(testPathWithQuotes));
            mockStreamWriter.Verify(writer => writer.Write(testFileNameWithQuotes));
        }

        [Theory]
        [InlineData(@"C:\somedir\somefile.txt", "CyA2DbkxG5oPUX/flw2v4RZDvHmdzSQL0jKAWlrsMVY=")]
        [InlineData(@"\\NETWORK\LOCATION\networkfile", "XdGu4hg63jhhgd84UFNM/38956NDJDIlrsMVY2jio38=")]
        public void AddFileRecord_ValidFileFingerprint_AddsRecordToStore(string file, string hash)
        {
            var testPath = Path.GetDirectoryName(file);
            var testFileName = Path.GetFileName(file);

            // Arrange
            var mockFileInfo = new Mock<IFileInfo>();
            mockFileInfo.SetupGet(mock => mock.DirectoryName).Returns(testPath);
            mockFileInfo.SetupGet(mock => mock.FullName).Returns(file);
            mockFileInfo.SetupGet(mock => mock.Name).Returns(testFileName);

            var mockStreamWriter = new Mock<TextWriter>(MockBehavior.Default);

            // Act
            using (CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(mockStreamWriter.Object))
            {
                testObject.AddFileRecord(new FileFingerprint(mockFileInfo.Object, hash));
            }

            // Assert
            mockStreamWriter.Verify(writer => writer.Write(testPath));
            mockStreamWriter.Verify(writer => writer.Write(testFileName));
            mockStreamWriter.Verify(writer => writer.Write(hash));
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
                this.mockDefaultStreamWriter.Object.Dispose();
            }

            this.disposed = true;
        }
    }
}
