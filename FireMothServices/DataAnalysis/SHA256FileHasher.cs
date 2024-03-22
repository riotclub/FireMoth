// <copyright file="SHA256FileHasher.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAnalysis;

using System;
using System.IO;
using System.Security.Cryptography;
using RiotClub.FireMoth.Services.Tasks.Output;

/// <summary>
/// Provides file hashing services using the SHA256 algorithm.
/// </summary>
// ReSharper disable once InconsistentNaming (following .NET's convention here [see SHA256])
public sealed class SHA256FileHasher : IFileHasher, IDisposable
{
    private readonly HashAlgorithm _hashAlgorithm;
    private bool _disposed;
    private const int InputBufferLength = 2 << 20;  // 2 << 20 = 2 MB

    /// <summary>
    /// Initializes a new instance of the <see cref="SHA256FileHasher"/> class.
    /// </summary>
    public SHA256FileHasher()
    {
        _hashAlgorithm = SHA256.Create();
    }

    /// <inheritdoc/>
    public byte[] ComputeHashFromStream(Stream inputStream)
    {
        long inputOffset = 0;
        var inputBuffer = new byte[InputBufferLength];
        int bytesRead;
        ConsoleProgressBar.TrySetCursorVisibility(false);
        
        while (inputStream.Length - inputOffset >= inputBuffer.Length)
        {
            bytesRead = inputStream.Read(inputBuffer, 0, inputBuffer.Length);
            _hashAlgorithm.TransformBlock(inputBuffer, 0, bytesRead, inputBuffer, 0);
            ConsoleProgressBar.WriteProgressBar((float)inputStream.Position / inputStream.Length);
            inputOffset += bytesRead;
        }

        bytesRead = inputStream.Read(inputBuffer, 0, inputBuffer.Length);
        _hashAlgorithm.TransformFinalBlock(inputBuffer, 0, bytesRead);
        ConsoleProgressBar.WriteProgressBar(1.0f, false);
        ConsoleProgressBar.TrySetCursorVisibility(true);
        
        return _hashAlgorithm.Hash
               ?? throw new InvalidOperationException("Unable to compute hash.");
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and, optionally, managed resources.
    /// </summary>
    /// <param name="disposing">If true, managed resources are freed.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _hashAlgorithm.Dispose();
        }

        _disposed = true;
    }
}