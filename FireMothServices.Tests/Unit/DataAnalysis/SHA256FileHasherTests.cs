// <copyright file="SHA256FileHasherTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.DataAnalysis;

using System;
using System.IO;
using System.Security.Cryptography;
using RiotClub.FireMoth.Services.DataAnalysis;
using FluentAssertions;
using Moq.AutoMock;
using Xunit;

/// <summary>
/// <p>
/// ComputeHashFromStream<br/>
/// - Calling on a disposed object throws an ObjectDisposedException.<br/>
/// - Passing [Stream:null] throws an ArgumentNullException.<br/>
/// - Passing [Stream:contains less data than the input buffer size] returns a proper hash of the
///   data.<br/>
/// - Passing [Stream:contains more data than the input buffer size] returns a proper hash of the
///   data.<br/>
/// - Passing [Stream:contains no data] returns the proper hash for an empty data stream.<br/>
/// </p>
/// <p>
/// Dispose<br/>
/// - Calling on a non-disposed object disposes the object.<br/>
/// - Calling on a disposed object does not throw exceptions.<br/>
/// </p>
/// </summary>
// ReSharper disable once InconsistentNaming (following .NET's convention here [see SHA256])
public class SHA256FileHasherTests
{
    private readonly AutoMocker _mocker = new();
    
#region ComputeHashFromStream
    /// <summary>ComputeHashFromStream: Calling on a disposed object throws an
    /// ObjectDisposedException.</summary>
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

    /// <summary>ComputeHashFromStream: Passing [Stream:null] throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void ComputeHashFromStream_StreamNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = _mocker.CreateInstance<SHA256FileHasher>();
        Action action = () => sut.ComputeHashFromStream(null!);

        // Act, Assert
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>ComputeHashFromStream: Passing [Stream:contains less data than the input buffer
    /// size] returns a proper hash of the data.</summary>
    [Fact]
    public void ComputeHashFromStream_StreamContainsLessDataThanInputBufferSize_ReturnsCorrectHash()
    {
        // Arrange
        var testData = new byte[SHA256FileHasher.InputBufferLength - 1];
        Random.Shared.NextBytes(testData);
        var testStream = new MemoryStream(testData);
        string expected;
        using (var hashAlgorithm = SHA256.Create())
        {
            hashAlgorithm.TransformFinalBlock(testData, 0, testData.Length);
            expected = Convert.ToBase64String(hashAlgorithm.Hash!);
        }
        var sut = new SHA256FileHasher();
        
        // Act
        var result = Convert.ToBase64String(sut.ComputeHashFromStream(testStream));

        // Assert
        result.Should().Be(expected);
    }
    
    /// <summary>ComputeHashFromStream: Passing [Stream:contains more data than the input buffer
    /// size] returns a proper hash of the data.</summary>
    [Fact]
    public void ComputeHashFromStream_StreamContainsMoreDataThanInputBufferSize_ReturnsCorrectHash()
    {
        // Arrange
        var testData = new byte[SHA256FileHasher.InputBufferLength + 1];
        Random.Shared.NextBytes(testData);
        var testStream = new MemoryStream(testData);
        string expected;
        using (var hashAlgorithm = SHA256.Create())
        {
            hashAlgorithm.TransformFinalBlock(testData, 0, testData.Length);
            expected = Convert.ToBase64String(hashAlgorithm.Hash!);
        }
        var sut = new SHA256FileHasher();
        
        // Act
        var result = Convert.ToBase64String(sut.ComputeHashFromStream(testStream));

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>ComputeHashFromStream: Passing [Stream:contains no data] returns the proper hash
    /// for an empty data stream.</summary>
    [Fact]
    public void ComputeHashFromStream_StreamContainsNoData_ReturnsSomething()
    {
        // Arrange
        var testData = Array.Empty<byte>();
        var testStream = new MemoryStream(testData);
        string expected;
        using (var hashAlgorithm = SHA256.Create())
        {
            hashAlgorithm.TransformFinalBlock(testData, 0, testData.Length);
            expected = Convert.ToBase64String(hashAlgorithm.Hash!);
        }
        var sut = new SHA256FileHasher();
        
        // Act
        var resultBytes = sut.ComputeHashFromStream(testStream);
        var result = Convert.ToBase64String(resultBytes);

        // Assert
        result.Should().Be(expected);
    }
#endregion

#region Dispose
    /// <summary>Dispose: Calling on a non-disposed object disposes the object.</summary>
    [Fact]
    public void Dispose_CalledOnNonDisposedObject_DisposesObject()
    {
        // Arrange
        var sut = new SHA256FileHasher();

        // Act
        sut.Dispose();
        var callOnDisposedObject = () => sut.ComputeHashFromStream(null!);

        // Assert
        callOnDisposedObject.Should().ThrowExactly<ObjectDisposedException>();
    }
    
    /// <summary>Dispose: Calling on a disposed object does not throw exceptions.</summary>
    [Fact]
    public void Dispose_CalledOnDisposedObject_DoesNotThrow()
    {
        // Arrange
        var sut = new SHA256FileHasher();

        // Act
        sut.Dispose();
        var callOnDisposedObject = () => sut.Dispose();

        // Assert
        callOnDisposedObject.Should().NotThrow();
    }    
#endregion
}