// <copyright file="MemoryDataAccessLayer.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RiotClub.FireMoth.Services.DataAccess.Csv;

/// <summary>
/// Implementation of a data access layer that persists data to memory.
/// </summary>
public class MemoryDataAccessLayer : IDataAccessLayer<IFileFingerprint>, IDisposable
{
    private readonly IList<IFileFingerprint> _fileFingerprints;
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
            
        var filteredResult = _fileFingerprints.AsEnumerable();

        if (filter is not null) filteredResult = _fileFingerprints.Where(filter);
        if (orderBy is not null) filteredResult = _fileFingerprints.OrderBy(orderBy);

        return Task.FromResult(filteredResult);
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

        if (fileFingerprint == null) throw new ArgumentNullException(nameof(fileFingerprint));

        _fileFingerprints.Add(fileFingerprint);

        var fullPath = Path.Combine(fileFingerprint.DirectoryName, fileFingerprint.FileName);
        _logger.LogDebug(
            "Writing fingerprint for file {FileName} with hash {HashString}.",
            fullPath,
            fileFingerprint.Base64Hash);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the provided <see cref="IFileFingerprint"/> in the data access layer.
    /// </summary>
    /// <param name="fileFingerprint">A <see cref="IFileFingerprint"/> to update.</param>
    /// <returns><c>true</c> if a file matching the provided <see cref="IFileFingerprint"/>'s full path was updated,
    /// <c>false</c> if no file with a matching path could be found.</returns>
    public Task<bool> UpdateAsync(IFileFingerprint fileFingerprint)
    {
        ThrowIfDisposed();

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
    /// <param name="fileFingerprint"></param>
    /// <returns></returns>
    public Task<bool> DeleteAsync(IFileFingerprint fileFingerprint)
    {
        ThrowIfDisposed();

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

    private void ThrowIfDisposed([CallerMemberName] string methodName = "")
    {
        if (!_disposed) return;

        _logger.LogCritical($"Tried to call {methodName} on disposed object.");
        throw new ObjectDisposedException(GetType().FullName);
    }
}