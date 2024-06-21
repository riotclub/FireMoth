// <copyright file="ScanErrorTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.FileScanning;

using System;
using RiotClub.FireMoth.Services.FileScanning;
using AutoFixture;
using FluentAssertions;
using Moq.AutoMock;
using Xunit;

/// <summary>
/// Ctor
/// - Passing [message: null] throws an ArgumentNullException.
/// - Passing [message: empty or whitespace] throws an ArgumentException.
/// - Passing [valid parameters] properly initializes an object.
/// </summary>
public class ScanErrorTests
{
    private readonly Fixture _fixture = new();
    
#region Ctor
    /// <summary>Ctor: Passing [message: null] throws an ArgumentNullException.</summary>
    [Fact]
    public void Ctor_MessageNull_ThrowsArgumentNullException()
    {
        // Arrange
        var ctorFunc = () => new ScanError(null, null!, null);

        // Act, Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>Ctor: Passing [message: empty or whitespace] throws an ArgumentException.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData("\n\n")]
    public void Ctor_MessageEmptyOrWhitespace_ThrowsArgumentException(string testData)
    {
        // Arrange
        var ctorFunc = () => new ScanError(null, testData, null);

        // Act, Assert
        ctorFunc.Should().ThrowExactly<ArgumentException>();
    }
    
    /// <summary>Ctor: Passing [valid parameters] properly initializes an object.</summary>
    [Fact]
    public void Ctor_ValidParameters_InitializesObject()
    {
        // Arrange
        var expectedPath = _fixture.Create<string>();
        var expectedMessage = _fixture.Create<string>();
        var expectedException = _fixture.Create<Exception>();

        // Act
        var created = new ScanError(expectedPath, expectedMessage, expectedException);

        // Assert
        created.Path.Should().Be(expectedPath);
        created.Message.Should().Be(expectedMessage);
        created.Exception.Should().Be(expectedException);
    }    
#endregion
}