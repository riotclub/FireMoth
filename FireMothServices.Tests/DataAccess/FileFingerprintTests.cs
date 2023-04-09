// <copyright file="FileFingerprintTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.FileScanning;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions.TestingHelpers;
using RiotClub.FireMoth.Services.DataAnalysis;
using RiotClub.FireMoth.Services.Repository;
using Xunit;

/// <summary>
/// <see cref="FileFingerprint"/> unit tests.
///
/// Test naming convention: [method]_[preconditions]_[expected result]
///
/// Ctor
/// - base64Hash string can't be null or empty
///     * Ctor_NullString_ThrowsArgumentNullException
///     * Ctor_EmptyOrWhitespaceString_ThrowsArgumentException
/// - base64Hash must be a valid base 64 string
///     * Ctor_InvalidBase64String_ThrowsArgumentException
///
/// Equals
/// - Null FileFingerprint returns false
///     - Equal_NullFileFingerprint_ReturnsFalse
/// - Semantically equal FileFingerprint returns true
///     - Equal_EqualFileFingerprint_ReturnsTrue
/// - Semantically different FileFingerprint returns false
///     - Equal_DifferentFileFingerprint_ReturnsFalse
/// - Incompatible type returns false
///     - Equal_IncompatibleType_ReturnsFalse
///
/// operator ==
/// - Two semantically equal FileFingerprint instances returns true
///     - EqualityOperator_LeftOperandEqualsRightOperand_ReturnsTrue
/// - One null and one non-null FileFingerprint instance returns false
///     - EqualityOperator_NullLeftOperandAndNonNullRightOperand_ReturnsFalse
///     - EqualityOperator_NonNullLeftOperandAndNullRightOperand_ReturnsFalse
/// - Two null values returns true
///     - EqualityOperator_BothOperandsNull_ReturnsTrue
///
/// operator !=
/// - Two semantically equal FileFingerprint instances returns false
///     - InequalityOperator_LeftOperandEqualsRightOperand_ReturnsFalse
/// - One null and one non-null FileFingerprint instance returns true
///     - InequalityOperator_NullLeftOperandAndNonNullRightOperand_ReturnsTrue
///     - InequalityOperator_NonNullLeftOperandAndNullRightOperand_ReturnsTrue
/// - Two null values returns false
///     - InequalityOperator_BothOperandsNull_ReturnsFalse
///
/// GetHashCode
/// - Semantically equal instances have equal hashes
///      - GetHashCode_EqualInstances_ReturnEqualHashCodes
/// .
/// </summary>
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

    // Ctor: base64Hash string can't be null or empty
    [Fact]
    public void Ctor_NullString_ThrowsArgumentNullException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentNullException>(() =>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            new FileFingerprint(string.Empty, string.Empty, 0, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    // Ctor: base64Hash string can't be null or empty
    [Theory]
    [InlineData("")]
    [InlineData("     \t\n")]
    public void Ctor_EmptyOrWhitespaceString_ThrowsArgumentException(string value)
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentException>(() =>
            new FileFingerprint(string.Empty, string.Empty, 0, value));
    }

    // Ctor: base64Hash must be a valid base 64 string
    [Fact]
    public void Ctor_InvalidBase64String_ThrowsArgumentException()
    {
        // Arrange, Act, Assert
        Assert.Throws<ArgumentException>(() =>
            new FileFingerprint(string.Empty, string.Empty, 0, "asdf!!!000"));
    }

    // Equals: Semantically equal FileFingerprint returns true
    [Fact]
    public void Equal_EqualFileFingerprint_ReturnsTrue()
    {
        // Arrange
        var testObject = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 1, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");
        var testComparator = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 1, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");

        // Act, Assert
        Assert.True(testObject.Equals(testComparator));
    }

    // Equals: Semantically different FileFingerprint returns false
    [Fact]
    public void Equal_DifferentFileFingerprints_ReturnsFalse()
    {
        // Arrange
        var testObject = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 100, "AAA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");
        var testComparator = new FileFingerprint(
            @"c:\test", "AnotherFile.dat", 200, "BBB2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");

        // Act, Assert
        Assert.False(testObject.Equals(testComparator));
    }

    // Equals: Incompatible type returns false
    [Fact]
    public void Equal_IncompatibleType_ReturnsFalse()
    {
        // Arrange
        var testObject = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 100, "AAA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");
        var testComparator = new SHA256FileHasher();

        // Act, Assert
        Assert.False(testObject.Equals(testComparator));
    }

    // operator ==: Two semantically equal FileFingerprint instances returns true
    [Fact]
    public void EqualityOperator_LeftOperandEqualsRightOperand_ReturnsTrue()
    {
        // Arrange
        var left = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 1, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");
        var right = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 1, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");

        // Act, Assert
        Assert.True(left == right);
    }

    // operator ==: One null and one non-null FileFingerprint instance returns false
    [Fact]
    public void EqualityOperator_NonNullLeftOperandAndNullRightOperand_ReturnsFalse()
    {
        // Arrange
        FileFingerprint left = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 100, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");
        FileFingerprint? right = null;

        // Act, Assert
        Assert.False(left == right);
    }

    // operator ==: One null and one non-null FileFingerprint instance returns false
    [Fact]
    public void EqualityOperator_NullLeftOperandAndNonNullRightOperand_ReturnsFalse()
    {
        // Arrange
        FileFingerprint? left = null;
        FileFingerprint right = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 100, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");

        // Act, Assert
        Assert.False(left == right);
    }

    // operator ==: Two null values returns true
    [Fact]
    public void EqualityOperator_BothOperandsNull_ReturnsTrue()
    {
        // Arrange
        FileFingerprint? left = null;
        FileFingerprint? right = null;

        // Act, Assert
        Assert.True(left == right);
    }

    // operator !=: Two semantically equal FileFingerprint instances returns false
    [Fact]
    public void InequalityOperator_LeftOperandEqualsRightOperand_ReturnsFalse()
    {
        // Arrange
        var left = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 1, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");
        var right = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 1, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");

        // Act, Assert
        Assert.False(left != right);
    }

    // operator !=: One null and one non-null FileFingerprint instance returns true
    [Fact]
    public void InequalityOperator_NonNullLeftOperandAndNullRightOperand_ReturnsTrue()
    {
        // Arrange
        FileFingerprint left = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 100, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");
        FileFingerprint? right = null;

        // Act, Assert
        Assert.True(left != right);
    }

    // operator !=: One null and one non-null FileFingerprint instance returns true
    [Fact]
    public void InequalityOperator_NullLeftOperandAndNonNullRightOperand_ReturnsTrue()
    {
        // Arrange
        FileFingerprint? left = null;
        FileFingerprint right = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 100, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");

        // Act, Assert
        Assert.True(left != right);
    }

    // operator !=: Two null values returns false
    [Fact]
    public void InequalityOperator_BothOperandsNull_ReturnsFalse()
    {
        // Arrange
        FileFingerprint? left = null;
        FileFingerprint? right = null;

        // Act, Assert
        Assert.False(left != right);
    }

    // GetHashCode: Semantically equal instances have equal hashes
    [Fact]
    public void GetHashCode_EqualInstances_ReturnEqualHashCodes()
    {
        // Arrange
        var left = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 100, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");
        var right = new FileFingerprint(
            @"c:\test", "SomeFile.txt", 100, "ByA2dbkxG5oPUX/flw2vMRZDvHmdzSQL0jKAWlrsMVY=");

        // Act, Assert
        Assert.True(left.GetHashCode() == right.GetHashCode());
    }
}