// <copyright file="SHA256FileHasher.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAnalysis;

using System;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// Provides file hashing services using the SHA256 algorithm.
/// </summary>
public class SHA256FileHasher : IFileHasher, IDisposable
{
    private readonly HashAlgorithm _hashAlgorithm;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SHA256FileHasher"/> class.
    /// </summary>
    public SHA256FileHasher()
    {
        _hashAlgorithm = SHA256.Create();
    }

    /// <inheritdoc/>
    public byte[] ComputeHashFromStream(Stream inputStream) =>
        _hashAlgorithm.ComputeHash(inputStream);

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
    protected virtual void Dispose(bool disposing)
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