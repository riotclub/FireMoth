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
     * - base64Hash string can't be null or empty
     *      * Ctor_NullString_ThrowsArgumentNullException
     *      * Ctor_EmptyOrWhitespaceString_ThrowsArgumentException
     * - base64Hash must be a valid base 64 string
     *      * Ctor_InvalidBase64String_ThrowsArgumentException
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
        public void Ctor_NullString_ThrowsArgumentNullException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileFingerprint(this.mockFileInfo, null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("     \t\n")]
        public void Ctor_EmptyOrWhitespaceString_ThrowsArgumentException(string value)
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentException>(() =>
                new FileFingerprint(this.mockFileInfo, value));
        }

        [Fact]
        public void Ctor_InvalidBase64String_ThrowsArgumentException()
        {
            // Arrange, Act, Assert
            Assert.Throws<ArgumentException>(() =>
                new FileFingerprint(this.mockFileInfo, "asdf!!!000"));
        }
    }
}
