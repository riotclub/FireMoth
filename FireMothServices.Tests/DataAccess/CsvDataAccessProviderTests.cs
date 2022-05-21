// <copyright file="CsvDataAccessProviderTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Moq;
    using RiotClub.FireMoth.Services.DataAccess;
    using Xunit;

    /*
     * Ctor
     *  * StreamWriter can't be null
     *      * Ctor_NullStreamWriter_ThrowsArgumentNullException
     *  * ILogger can't be null
     *      * Ctor_NullILogger_ThrowsArgumentNullException
     *
     * AddFileRecord
     * - IFileFingerprint can't be null
     *      * AddFileRecord_NullIFileFingerprint_ThrowsArgumentNullException
     * - Valid IFileFingerprint adds record to backing store
     *      * AddFileRecord_ValidFileFingerprint_AddsRecordToStore
     * - File with commas in name adds file with quotes to backing store
     *      * AddFileRecord_FileWithCommas_AddsRecordWithQuotedFile
     * - Call on disposed object throws exception
     *      * AddFileRecord_DisposedObject_ThrowsObjectDisposedException
     *
     * Dispose
     * - If constructed with leaveOpen true, underlying StreamWriter object is undisposed after disposal
     *      * Dispose_LeaveOpenTrue_StreamWriterUndisposed
     * - If constructed with leaveOpen false, underlying StreamWriter object is disposed after disposal
     *      - Dispose_LeaveOpenFalse_StreamWriterDisposed
     */
    [ExcludeFromCodeCoverage]
    public class CsvDataAccessProviderTests : IDisposable
    {
        // private readonly Mock<IFileInfo> mockFileInfo;
        private readonly StreamWriter testStreamWriter;
        private readonly string testFilePath = @"C:\TestDir";
        private readonly string testFileName = "TestFile.txt";
        private readonly string testHash = "CyA2DbkxG5oPUX/flw2v4RZDvHmdzSQL0jKAWlrsMVY=";
        private readonly ILogger<CsvDataAccessProvider> testLogger;
        private bool disposed;

        public CsvDataAccessProviderTests()
        {
            this.testStreamWriter = new StreamWriter(new MemoryStream(), Encoding.UTF8);

            this.testLogger = LoggerFactory
                .Create(builder => builder.SetMinimumLevel(LogLevel.Information))
                .CreateLogger<CsvDataAccessProvider>();
        }

        [Fact]
        public void Ctor_NullStreamWriter_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() => new CsvDataAccessProvider(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null, this.testLogger, true));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact]
        public void Ctor_NullILogger_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() => new CsvDataAccessProvider(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                this.testStreamWriter, null, true));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact]
        public void Ctor_ValidArguments_CreatesObject()
        {
            // Arrange, Act
            var testObject = new FileFingerprint(
                this.testFilePath, this.testFileName, 100, this.testHash);

            // Assert
            Assert.NotNull(testObject);
        }

        [Fact]
        public void AddFileRecord_NullIFileFingerprint_ThrowsArgumentNullException()
        {
            // Arrange
            using CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.testStreamWriter, this.testLogger, true);

            // Act, Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => testObject.AddFileRecord(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Theory]
        [InlineData(@"C:\somedir\somefile.txt", "CyA2DbkxG5oPUX/flw2v4RZDvHmdzSQL0jKAWlrsMVY=")]
        [InlineData(@"\\NETWORK\LOCATION\networkfile", "XdGu4hg63jhhgd84UFNM/38956NDJDIlrsMVY2jio38=")]
        public async void AddFileRecord_ValidFileFingerprint_AddsRecordToStore(
            string file, string hash)
        {
            // Arrange
            var testPath = Path.GetDirectoryName(file);
            var testFileName = Path.GetFileName(file);

            // Act
            var dapOutput = await this.GetAddFileRecordOutput(
                new FileFingerprint(testFileName, testPath!, 100, hash));

            // Assert
            var expectedOutput = string.Join(
                ',', new List<string> { testFileName, testPath!, "100", hash });
            Assert.Contains(expectedOutput, dapOutput);
        }

        [Fact]
        public async void AddFileRecord_FileWithCommas_AddsRecordWithQuotedFile()
        {
            // Arrange
            var testFullPath = @"C:\dir, with, commas\file, with, commas.dat";
            var testHash = "XdGu4hg63jhhgd84UFNM/38956NDJDIlrsMVY2jio38=";

            var testPath = Path.GetDirectoryName(testFullPath);
            var testPathWithQuotes = '"' + testPath + '"';
            var testFileName = Path.GetFileName(testFullPath);
            var testFileNameWithQuotes = '"' + testFileName + '"';

            // Act
            var dapOutput = await this.GetAddFileRecordOutput(
                new FileFingerprint(testFileName, testPath!, 100, testHash));

            // Assert
            var expectedOutput = string.Join(
                ',',
                new List<string> { testFileNameWithQuotes, testPathWithQuotes, "100", testHash });
            Assert.Contains(expectedOutput, dapOutput);
        }

        [Fact]
        public void AddFileRecord_DisposedObject_ThrowsObjectDisposedException()
        {
            // Arrange
            var testObject = new CsvDataAccessProvider(this.testStreamWriter, this.testLogger);

            // Act
            testObject.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() =>
                testObject.AddFileRecord(
                    new FileFingerprint(this.testFilePath, this.testFileName, 100, this.testHash)));
        }

        [Fact]
        public void Dispose_LeaveOpenTrue_StreamWriterUndisposed()
        {
            // Arrange
            var testString = "test$1234567890";
            var testStream = new MemoryStream();
            var testWriter = new StreamWriter(testStream, Encoding.UTF8);
            CsvDataAccessProvider testDataAccessProvider =
                new CsvDataAccessProvider(testWriter, this.testLogger, true);

            // Act
            testDataAccessProvider.Dispose();
            testWriter.WriteLine(testString);
            testWriter.Flush();
            testStream.Position = 0;
            using StreamReader reader = new StreamReader(testStream);
            var result = reader.ReadToEnd();

            // Assert
            Assert.Contains(testString, result);
        }

        [Fact]
        public void Dispose_LeaveOpenFalse_StreamWriterDisposed()
        {
            // Arrange
            var testStream = new MemoryStream();
            var testWriter = new StreamWriter(testStream, Encoding.UTF8);
            CsvDataAccessProvider testDataAccessProvider =
                new CsvDataAccessProvider(testWriter, this.testLogger, false);

            // Act
            testDataAccessProvider.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => testWriter.WriteLine());
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
                this.testStreamWriter.Dispose();
            }

            this.disposed = true;
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

        // Given an IFileFingerprint, creates a CsvDataAccessProvider, calls AddFileRecord, and
        // returns the output generated from the DAP.
        private async Task<string> GetAddFileRecordOutput(IFileFingerprint fileFingerprint)
        {
            var testStream = new MemoryStream();

            // StreamWriter disposes underlying MemoryStream when disposed
            var testWriter = new StreamWriter(testStream, Encoding.UTF8);

            using (CsvDataAccessProvider dataAccessProvider =
                new CsvDataAccessProvider(testWriter, this.testLogger, true))
            {
                dataAccessProvider.AddFileRecord(fileFingerprint);
            }

            testStream.Position = 0;
            using StreamReader reader = new StreamReader(testStream);
            return await reader.ReadToEndAsync();
        }
    }
}
