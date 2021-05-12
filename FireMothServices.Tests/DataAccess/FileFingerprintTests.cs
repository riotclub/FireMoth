// <copyright file="FileFingerprintTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.FileScanning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions.TestingHelpers;
    using RiotClub.FireMoth.Services.DataAccess;
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
    [ExcludeFromCodeCoverage]
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
    }
}
