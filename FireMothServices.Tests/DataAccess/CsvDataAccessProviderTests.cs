// <copyright file="CsvDataAccessProviderTests.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using System.Threading.Tasks;
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
        public async void AddFileRecord_FileWithCommas_AddsRecordWithQuotedFile()
        {
            var testFullPath = @"C:\dir, with, commas\file, with, commas.dat";
            var testHash = "XdGu4hg63jhhgd84UFNM/38956NDJDIlrsMVY2jio38=";

            var testPath = Path.GetDirectoryName(testFullPath);
            var testPathWithQuotes = '"' + testPath + '"';
            var testFileName = Path.GetFileName(testFullPath);
            var testFileNameWithQuotes = '"' + testFileName + '"';

            // Arrange
            var mockFileInfo = GetFileInfoMock(testPath, testFullPath, testFileName);
            var dapOutput = await GetAddFileRecordOutput(
                new FileFingerprint(mockFileInfo.Object, testHash));

            // Assert
            var expectedOutput = string.Join(
                ',',
                new List<string> { testPathWithQuotes, testFileNameWithQuotes, "0", testHash });
            Assert.Contains(expectedOutput, dapOutput);
        }

        [Theory]
        [InlineData(@"C:\somedir\somefile.txt", "CyA2DbkxG5oPUX/flw2v4RZDvHmdzSQL0jKAWlrsMVY=")]
        [InlineData(@"\\NETWORK\LOCATION\networkfile", "XdGu4hg63jhhgd84UFNM/38956NDJDIlrsMVY2jio38=")]
        public async void AddFileRecord_ValidFileFingerprint_AddsRecordToStore(string file, string hash)
        {
            var testPath = Path.GetDirectoryName(file);
            var testFileName = Path.GetFileName(file);

            // Arrange
            var mockFileInfo = GetFileInfoMock(testPath, file, testFileName);
            var dapOutput = await GetAddFileRecordOutput(
                new FileFingerprint(mockFileInfo.Object, hash));

            // Assert
            var expectedOutput = string.Join(
                ',', new List<string> { testPath, testFileName, "0", hash });
            Assert.Contains(expectedOutput, dapOutput);
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

        // Given an IFileFingerprint, creates a CsvDataAccessProvider, calls AddFileRecord, and
        // returns the output generated from the DAP.
        private static async Task<string> GetAddFileRecordOutput(IFileFingerprint fileFingerprint)
        {
            var testStream = new MemoryStream();
            var testWriter = new StreamWriter(testStream, Encoding.UTF8);

            using (CsvDataAccessProvider dataAccessProvider =
                new CsvDataAccessProvider(testWriter, true))
            {
                dataAccessProvider.AddFileRecord(fileFingerprint);
            }

            testStream.Position = 0;
            using (StreamReader reader = new StreamReader(testStream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private static Mock<IFileInfo> GetFileInfoMock(
            string directoryName, string fullName, string name)
        {
            var mockFileInfo = new Mock<IFileInfo>();
            mockFileInfo.SetupGet(mock => mock.DirectoryName).Returns(directoryName);
            mockFileInfo.SetupGet(mock => mock.FullName).Returns(fullName);
            mockFileInfo.SetupGet(mock => mock.Name).Returns(name);

            return mockFileInfo;
        }
    }
}
