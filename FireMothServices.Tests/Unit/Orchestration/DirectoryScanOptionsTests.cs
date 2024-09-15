// <copyright file="DirectoryScanOptionsTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.Orchestration;

using FluentAssertions;
using Services.Orchestration;
using Xunit;

/// <summary>
/// <p> 
/// InitDirectory (Property)<br/>
/// - Sets Directory property.<br/>
/// </p>
/// <p> 
/// InitRecursive (property)<br/>
/// - Sets Recursive property.<br/>
/// </p>
/// </summary>
public class DirectoryScanOptionsTests
{
    /// <summary>InitDirectory: Sets Directory property.</summary>
    [Fact]
    public void InitDirectory_MethodCalled_SetsDirectoryProperty()
    {
        // Arrange
        const string expected = "/home/";

        // Act
        var sut = new DirectoryScanOptions { Directory = expected };

        // Assert
        sut.Directory.Should().Be(expected);
    }
    
    /// <summary>InitRecursive: Sets Recursive property.</summary>
    [Fact]
    public void InitRecursive_MethodCalled_SetsRecursiveProperty()
    {
        // Arrange
        const bool expected = true;

        // Act
        var sut = new DirectoryScanOptions { Recursive = expected };

        // Assert
        sut.Recursive.Should().Be(expected);
    }
}