// <copyright file="MemoryDataAccessLayer.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.InMemory;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// Implementation of a data access layer that persists data to memory.
/// </summary>
public class MemoryDataAccessLayer : IDataAccessLayer<IFileFingerprint>, IDisposable
{
    private readonly List<IFileFingerprint> _fileFingerprints;
    private readonly ILogger<MemoryDataAccessLayer> _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryDataAccessLayer"/> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> used to log information.</param>
    public MemoryDataAccessLayer(ILogger<MemoryDataAccessLayer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
        _fileFingerprints = new List<IFileFingerprint>();
    }

    /// <summary>
    /// Retrieves file fingerprints from the data access layer.
    /// </summary>
    /// <param name="filter">A lambda expression that specifies a filter condition.</param>
    /// <param name="orderBy">A lambda expression that specifies an ordering.</param>
    /// <returns>IEnumerable collection of file fingerprints.</returns>
    public Task<IEnumerable<IFileFingerprint>> GetAsync(
        Func<IFileFingerprint, bool>? filter = null,
        Func<IFileFingerprint, string>? orderBy = null)
    {
        ThrowIfDisposed();
            
        var result = _fileFingerprints.AsEnumerable();

        if (filter is not null)
            result = result.Where(filter);

        if (orderBy is not null)
            result = result.OrderBy(orderBy);

        return Task.FromResult(result);
    }

    /// <summary>
    /// Adds the provided <see cref="IFileFingerprint"/> to the data access layer.
    /// </summary>
    /// <param name="fileFingerprint">A <see cref="IFileFingerprint"/> to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when provided <see cref="IFileFingerprint"/> reference is
    /// null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when object is in a disposed state.</exception>
    public Task AddAsync(IFileFingerprint fileFingerprint)
    {
        ThrowIfDisposed();
        ThrowIfArgumentNull(fileFingerprint, nameof(fileFingerprint));

        _fileFingerprints.Add(fileFingerprint);

        var fullPath = Path.Combine(fileFingerprint.DirectoryName, fileFingerprint.FileName);
        _logger.LogDebug(
            "Writing fingerprint for file {FileName} with hash {HashString}.",
            fullPath,
            fileFingerprint.Base64Hash);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds the provided collection of <see cref="IFileFingerprint"/>s to the data access layer.
    /// </summary>
    /// <param name="fileFingerprints">An <see cref="IEnumerable{IFileFingerprint}"/> containing items to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when provided collection is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when object is in a disposed state.</exception>
    public Task AddManyAsync(IEnumerable<IFileFingerprint> fileFingerprints)
    {
        ThrowIfDisposed();
        ThrowIfArgumentNull(fileFingerprints, nameof(fileFingerprints));

        var fileFingerprintList = fileFingerprints.ToList();
        _fileFingerprints.AddRange(fileFingerprintList);
        _logger.LogDebug("Writing {FileFingerprintCount} fingerprint(s).", fileFingerprintList.Count);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the provided <see cref="IFileFingerprint"/> in the data access layer.
    /// </summary>
    /// <param name="fileFingerprint">A <see cref="IFileFingerprint"/> to update.</param>
    /// <returns><c>true</c> if a file matching the provided <see cref="IFileFingerprint"/>'s was updated, <c>false</c>
    /// if no match could be found.</returns>
    public Task<bool> UpdateAsync(IFileFingerprint fileFingerprint)
    {
        ThrowIfDisposed();
        ThrowIfArgumentNull(fileFingerprint, nameof(fileFingerprint));

        var match = _fileFingerprints.FirstOrDefault(fingerprint =>
            fingerprint.FullPath == fileFingerprint.FullPath);
            
        if (match is null) return Task.FromResult(false);

        _fileFingerprints.Remove(match);
        _fileFingerprints.Add(fileFingerprint);
            
        return Task.FromResult(true);
    }

    /// <summary>
    /// Deletes the provided <see cref="IFileFingerprint"/> from the data access layer.
    /// </summary>
    /// <param name="fileFingerprint">A <see cref="IFileFingerprint"/> to delete.</param>
    /// <returns><c>true</c> if a file matching the provided <see cref="IFileFingerprint"/>'s was deleted, <c>false</c>
    /// if no match could be found.</returns>
    public Task<bool> DeleteAsync(IFileFingerprint fileFingerprint)
    {
        ThrowIfDisposed();
        ThrowIfArgumentNull(fileFingerprint, nameof(fileFingerprint));
        
        return Task.FromResult(_fileFingerprints.Remove(fileFingerprint));
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
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _logger.LogDebug("Flushing CsvWriter buffer and disposing.");
        }

        _disposed = true;
    }

    private static void ThrowIfArgumentNull(object testArgument, string argumentName)
    {
        if (testArgument is null)
            throw new ArgumentNullException(argumentName);
    }

    private void ThrowIfDisposed([CallerMemberName] string methodName = "")
    {
        if (!_disposed) return;

        _logger.LogCritical("Tried to call {MethodName} on disposed object.", methodName);
        throw new ObjectDisposedException(GetType().FullName);
    }
}