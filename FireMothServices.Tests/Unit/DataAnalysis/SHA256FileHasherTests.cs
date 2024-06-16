// <copyright file="SHA256FileHasherTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.IO;
using RiotClub.FireMoth.Services.DataAnalysis;

namespace RiotClub.FireMoth.Services.Tests.Unit.DataAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;
using RiotClub.FireMoth.Services.DataAccess.Sqlite;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Tests.Common.AutoFixture.SpecimenBuilders;
using Xunit;

/// <summary>
/// ComputeHashFromStream
/// - If object is disposed, throws an ObjectDisposedException.
/// - Passing a null Stream throws an ArgumentNullException.
/// - Passing a Stream containing data returns a proper hash of the data.
/// - Passing a Stream containing no data returns ???
///     - What is it supposed to return? I suspect an empty byte array but test and find out.
///         
/// Dispose
/// - After call, object is disposed.
/// </summary>
// ReSharper disable once InconsistentNaming (following .NET's convention here [see SHA256])
public class SHA256FileHasherTests
{
    private readonly AutoMocker _mocker = new();
    
#region ComputeHashFromStream

    /// - Passing a null Stream throws an ArgumentNullException.
    /// - Passing a Stream containing data returns a proper hash of the data.
    /// - Passing a Stream containing no data returns ???
    ///     - What is it supposed to return? I suspect an empty byte array but test and find out.
        
    /// <summary>ComputeHashFromStream: If object is disposed, throws an ObjectDisposedException.
    /// </summary>
    [Fact]
    public void ComputeHashFromStream_ObjectDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var mockStream = _mocker.CreateInstance<MemoryStream>();
        var sut = _mocker.CreateInstance<SHA256FileHasher>();
        sut.Dispose();
        Action action = () => sut.ComputeHashFromStream(mockStream);

        // Act, Assert
        action.Should().ThrowExactly<ObjectDisposedException>();
    }
#endregion
}