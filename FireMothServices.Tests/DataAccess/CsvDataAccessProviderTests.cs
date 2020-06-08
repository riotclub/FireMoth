// <copyright file="CsvDataAccessProviderTests.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.IO;
    using Microsoft.Extensions.FileProviders;
    using Moq;
    using RiotClub.FireMoth.Services.DataAccess;
    using Xunit;

    /*
     * Ctor:
     *  - TextWriter can't be null
     *      * Ctor_NullTextWriter_ThrowsArgumentNullException
     * AddFileRecord:
     * - IFileInfo can't be null
     *      * AddFileRecord_NullFileInfo_ThrowsArgumentNullException
     * - IFileInfo.PhysicalPath can't be null
     *      * AddFileRecord_NullFileInfoPhysicalPath_ThrowsArgumentException
     * - base64Hash can't be null
     *      * AddFileRecord_NullHash_ThrowsArgumentNullException
     * - base64hash can't be empty or whitespace
     *      * AddFileRecord_EmptyOrWhitespaceHash_ThrowsArgumentException
     * - base64hash must be a valid base 64 string
     *      * AddFileRecord_InvalidBase64String_ThrowsArgumentException
     * - Valid IFileInfo and base64Hash adds record to backing store
     *      * AddFileRecord_ValidFileInfoAndHash_AddsRecordToStore
     */
    public class CsvDataAccessProviderTests : IDisposable
    {
        private readonly Mock<IFileInfo> mockFileInfo;
        private readonly Mock<TextWriter> mockDefaultStreamWriter;
        private readonly string testBase64Hash = "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=";
        private readonly string testFilePhysicalPath = @"C:\TestDir\TestFile.txt";

        private bool disposed;

        public CsvDataAccessProviderTests()
        {
            this.mockFileInfo = new Mock<Microsoft.Extensions.FileProviders.IFileInfo>();
            this.mockFileInfo
                .SetupGet(mock => mock.PhysicalPath)
                .Returns(this.testFilePhysicalPath);

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
        public void AddFileRecord_NullFileInfo_ThrowsArgumentNullException()
        {
            // Arrange
            CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.mockDefaultStreamWriter.Object);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                testObject.AddFileRecord(null, this.testBase64Hash));
        }

        [Fact]
        public void AddFileRecord_NullFileInfoPhysicalPath_ThrowsArgumentException()
        {
            // Arrange
            var mockFileInfo = new Mock<IFileInfo>();
            mockFileInfo.SetupGet(mock => mock.PhysicalPath).Returns(() => null);
            CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.mockDefaultStreamWriter.Object);

            // Act, Assert
            Assert.Throws<ArgumentException>(() =>
                testObject.AddFileRecord(mockFileInfo.Object, this.testBase64Hash));
        }

        [Fact]
        public void AddFileRecord_NullHash_ThrowsArgumentNullException()
        {
            // Arrange
            CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.mockDefaultStreamWriter.Object);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                testObject.AddFileRecord(this.mockFileInfo.Object, null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData("\n")]
        public void AddFileRecord_EmptyOrWhitespaceHash_ThrowsArgumentException(
            string hashString)
        {
            // Arrange
            CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.mockDefaultStreamWriter.Object);

            // Act, Assert
            Assert.Throws<ArgumentException>(() =>
                testObject.AddFileRecord(this.mockFileInfo.Object, hashString));
        }

        [Theory]
        [InlineData("abcdefg!#")]
        [InlineData("0123+=$")]
        [InlineData("_0")]
        public void AddFileRecord_InvalidBase64Hash_ThrowsArgumentException(string hashString)
        {
            // Arrange
            CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.mockDefaultStreamWriter.Object);

            // Act, Assert
            Assert.Throws<ArgumentException>(() =>
                testObject.AddFileRecord(this.mockFileInfo.Object, hashString));
        }

        [Fact]
        public void AddFileRecord_ValidFileInfoAndHash_AddsRecordToStore()
        {
            // Arrange
            var mockStreamWriter = new Mock<TextWriter>();
            mockStreamWriter
                .Setup(writer =>
                    writer.WriteLine("{0},{1}", this.testFilePhysicalPath, this.testBase64Hash))
                .Verifiable();
            CsvDataAccessProvider testObject = new CsvDataAccessProvider(mockStreamWriter.Object);

            // Act
            testObject.AddFileRecord(this.mockFileInfo.Object, this.testBase64Hash);

            // Assert
            mockStreamWriter.Verify(writer =>
                writer.WriteLine("{0},{1}", this.testFilePhysicalPath, this.testBase64Hash));
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
