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
/// </summary>
public class ScanErrorTests
{
    private readonly AutoMocker _mocker = new();
    private readonly Fixture _fixture = new();
    
#region ComputeHashFromStream

    /// <summary></summary>
    [Fact]
    public void Method_Precondition_Postcondition()
    {
        // Arrange
        
        // Act
        
        // Assert
    }

#endregion

}