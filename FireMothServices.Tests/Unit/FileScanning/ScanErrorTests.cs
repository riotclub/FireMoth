// <copyright file="ScanErrorTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.FileScanning;

using System;
using RiotClub.FireMoth.Services.FileScanning;
using AutoFixture;
using FluentAssertions;
using Xunit;

/// <summary>
/// <p>
/// Ctor<br/>
/// - Passing [message:null] throws an ArgumentNullException.<br/>
/// - Passing [message:empty or whitespace] throws an ArgumentException.<br/>
/// - Passing [valid parameters] properly initializes an object.<br/>
/// </p>
/// <p>
/// GetPath (property)<br/>
/// - Returns the proper value of the path property.<br/>
/// </p>
/// <p>
/// GetMessage (property)<br/>
/// - Returns the proper value of the message property.<br/>
/// </p>
/// <p>
/// GetException (property)<br/>
/// - Returns the proper value of the exception property.<br/>
/// </p>
/// </summary>
public class ScanErrorTests
{
    private readonly Fixture _fixture = new();
    
#region Ctor
    /// <summary>Ctor: Passing [message:null] throws an ArgumentNullException.</summary>
    [Fact]
    public void Ctor_MessageNull_ThrowsArgumentNullException()
    {
        // Arrange
        var ctorFunc = () => new ScanError(null, null!, null);

        // Act, Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>Ctor: Passing [message:empty or whitespace] throws an ArgumentException.</summary>
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

#region GetPath
    /// <summary>GetPath: Returns the proper value of the path property.</summary>
    [Fact]
    public void GetPath_MethodCalled_ReturnsCorrectValue()
    {
        // Arrange
        var expected = _fixture.Create<string>();
        var sut = new ScanError(expected, _fixture.Create<string>(), _fixture.Create<Exception>());
        
        // Act
        var result = sut.Path;

        // Assert
        result.Should().Be(expected);
    }
#endregion

#region GetMessage
    /// <summary>GetMessage: Returns the proper value of the message property.</summary>
    [Fact]
    public void GetMessage_MethodCalled_ReturnsCorrectValue()
    {
        // Arrange
        var expected = _fixture.Create<string>();
        var sut = new ScanError(_fixture.Create<string>(), expected, _fixture.Create<Exception>());
        
        // Act
        var result = sut.Message;

        // Assert
        result.Should().Be(expected);
    }
#endregion    
    
#region GetException
    /// <summary>GetException: Returns the proper value of the exception property.</summary>
    [Fact]
    public void GetException_MethodCalled_ReturnsCorrectValue()
    {
        // Arrange
        var expected = _fixture.Create<Exception>();
        var sut = new ScanError(_fixture.Create<string>(), _fixture.Create<string>(), expected);
        
        // Act
        var result = sut.Exception;

        // Assert
        result.Should().Be(expected);
    }
#endregion
}