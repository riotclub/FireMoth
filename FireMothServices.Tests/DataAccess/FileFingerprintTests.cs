// <copyright file="FileFingerprintTests.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions.TestingHelpers;
    using FireMothServices.DataAccess;
    using Xunit;

    /*
     * Ctor
     * - IFileInfo can't be null
     *      * Ctor_NullFileInfo_ThrowsArgumentNullException

     * Base64Hash Set
     * - Value can't be null
     *      * Base64HashSet_NullValue_ThrowsArgumentNullException
     * - Value can't be empty or whitespace
     *      * Base64HashSet_EmptyOrWhitespaceValue_ThrowsArgumentException
     * - Value must be a valid base 64 string
     *      * Base64HashSet_InvalidBase64String_ThrowsArgumentException
     */
    public class FileFingerprintTests
    {
        private readonly MockFileSystem mockFileSystem;
        private readonly MockFileInfo mockFileInfo;
        private readonly string testBase64Hash = "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=";

        public FileFingerprintTests()
        {
            this.mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\test\SomeFile.txt", new MockFileData("000") },
                { @"c:\test\AnotherFile.dat", new MockFileData("000") },
            });

            this.mockFileInfo = new MockFileInfo(this.mockFileSystem, @"c:\test\SomeFile.txt");
        }

        [Fact]
        public void Ctor_NullFileInfo_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileFingerprint(null, this.testBase64Hash));
        }

        [Fact]
        public void Base64HashSet_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            FileFingerprint testObject =
                new FileFingerprint(this.mockFileInfo, this.testBase64Hash);

            // Act, Assert
            Assert.Throws<ArgumentException>(() => testObject.Base64Hash = null);
        }

        [Fact]
        public void Base64HashSet_EmptyOrWhitespaceValue_ThrowsArgumentException()
        {
            // Arrange
            FileFingerprint testObject =
                new FileFingerprint(this.mockFileInfo, this.testBase64Hash);

            // Act, Assert
            Assert.Throws<ArgumentException>(() => testObject.Base64Hash = "\t");
        }

        [Fact]
        public void Base64HashSet_InvalidBase64String_ThrowsArgumentException()
        {
            // Arrange
            FileFingerprint testObject =
                new FileFingerprint(this.mockFileInfo, this.testBase64Hash);

            // Act, Assert
            Assert.Throws<ArgumentException>(() => testObject.Base64Hash = "!!!");
        }

        /*
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
            using (CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.mockDefaultStreamWriter.Object))
            {
                // Act, Assert
                Assert.Throws<ArgumentNullException>(() => testObject.AddFileRecord(null));
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData("\n")]
        public void AddFileRecord_EmptyOrWhitespaceHash_ThrowsArgumentException(
            string hashString)
        {
            // Arrange
            using (CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.mockDefaultStreamWriter.Object))
            {
                // Act, Assert
                Assert.Throws<ArgumentException>(() =>
                    testObject.AddFileRecord(this.mockFileInfo.Object, hashString));
            }
        }

        [Theory]
        [InlineData("abcdefg!#")]
        [InlineData("0123+=$")]
        [InlineData("_0")]
        public void AddFileRecord_InvalidBase64Hash_ThrowsArgumentException(string hashString)
        {
            // Arrange
            using (CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(this.mockDefaultStreamWriter.Object))
            {
                // Act, Assert
                Assert.Throws<ArgumentException>(() =>
                    testObject.AddFileRecord(this.mockFileInfo.Object, hashString));
            }
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
            mockFileInfo.SetupGet(mock => mock.FullName).Returns(testFullPath);
            mockFileInfo.SetupGet(mock => mock.Name).Returns(testFileName);

            var mockStreamWriter = new Mock<TextWriter>();

            using (CsvDataAccessProvider testobject = new CsvDataAccessProvider(mockStreamWriter.Object))
            {
                // Act
                testobject.AddFileRecord(mockFileInfo.Object, testHash);
            }

            // Assert
            mockStreamWriter.Verify(writer => writer.Write(testPathWithQuotes));
            mockStreamWriter.Verify(writer => writer.Write(testFileNameWithQuotes));
        }

        [Theory]
        [InlineData(@"C:\somedir\somefile.txt", "CyA2DbkxG5oPUX/flw2v4RZDvHmdzSQL0jKAWlrsMVY=")]
        [InlineData(@"\\NETWORK\LOCATION\networkfile", "XdGu4hg63jhhgd84UFNM/38956NDJDIlrsMVY2jio38=")]
        public void AddFileRecord_ValidFileInfoAndHash_AddsRecordToStore(string file, string hash)
        {
            var testPath = Path.GetDirectoryName(file);
            var testFileName = Path.GetFileName(file);

            // Arrange
            var mockFileInfo = new Mock<IFileInfo>();
            mockFileInfo.SetupGet(mock => mock.FullName).Returns(file);
            mockFileInfo.SetupGet(mock => mock.Name).Returns(testFileName);

            var mockStreamWriter = new Mock<TextWriter>(MockBehavior.Default);

            // Act
            using (CsvDataAccessProvider testObject =
                new CsvDataAccessProvider(mockStreamWriter.Object))
            {
                testObject.AddFileRecord(mockFileInfo.Object, hash);
            }

            // Assert
            mockStreamWriter.Verify(writer => writer.Write(testPath));
            mockStreamWriter.Verify(writer => writer.Write(testFileName));
            mockStreamWriter.Verify(writer => writer.Write(hash));
        }
        */
    }
}
