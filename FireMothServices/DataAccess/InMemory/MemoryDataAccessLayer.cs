// <copyright file="MemoryDataAccessLayer.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.InMemory;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// Implementation of a data access layer that persists data to memory.
/// </summary>
public class MemoryDataAccessLayer : IDataAccessLayer<FileFingerprint>
{
    private readonly List<FileFingerprint> _fileFingerprints;
    private readonly ILogger<MemoryDataAccessLayer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryDataAccessLayer"/> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> used to log information.</param>
    public MemoryDataAccessLayer(ILogger<MemoryDataAccessLayer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _fileFingerprints = new List<FileFingerprint>();
    }

    /// <summary>
    /// Retrieves file fingerprints from the data access layer.
    /// </summary>
    /// <param name="filter">A lambda expression that specifies a filter condition.</param>
    /// <param name="orderBy">A lambda expression that specifies an ordering.</param>
    /// <returns>IEnumerable collection of file fingerprints.</returns>
    public Task<IEnumerable<FileFingerprint>> GetAsync(
        Func<FileFingerprint, bool>? filter = null,
        Func<FileFingerprint, string>? orderBy = null)
    {
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
    /// <exception cref="ArgumentNullException">Thrown when provided <see cref="IFileFingerprint"/>
    /// reference is null.</exception>
    public Task AddAsync(FileFingerprint fileFingerprint)
    {
        ThrowIfArgumentNull(fileFingerprint, nameof(fileFingerprint));
        _logger.LogDebug(
            "MemoryDataAccessLayer: Writing fingerprint {FileFingerprint}.", fileFingerprint);
        _fileFingerprints.Add(fileFingerprint);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds the provided collection of <see cref="IFileFingerprint"/>s to the data access layer.
    /// </summary>
    /// <param name="fileFingerprints">An <see cref="IEnumerable{IFileFingerprint}"/> containing
    /// items to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when provided collection is null.</exception>
    public Task AddManyAsync(IEnumerable<FileFingerprint> fileFingerprints)
    {
        ThrowIfArgumentNull(fileFingerprints, nameof(fileFingerprints));
        var fileFingerprintList = fileFingerprints.ToList();
        _logger.LogDebug(
            "MemoryDataAccessLayer: Writing {FileFingerprintCount} fingerprint(s).",
            fileFingerprintList.Count);
        _fileFingerprints.AddRange(fileFingerprintList);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes the provided <see cref="IFileFingerprint"/> from the data access layer.
    /// </summary>
    /// <param name="fileFingerprint">A <see cref="IFileFingerprint"/> to delete.</param>
    /// <returns><c>true</c> if a file matching the provided <see cref="IFileFingerprint"/>'s was
    /// deleted, <c>false</c> if no match could be found.</returns>
    public Task<bool> DeleteAsync(FileFingerprint fileFingerprint)
    {
        ThrowIfArgumentNull(fileFingerprint, nameof(fileFingerprint));
        _logger.LogDebug(
            "MemoryDataAccessLayer: Deleting fingerprint {FileFingerprint}", fileFingerprint);
        
        return Task.FromResult(_fileFingerprints.Remove(fileFingerprint));
    }

    private static void ThrowIfArgumentNull(object testArgument, string argumentName)
    {
        if (testArgument is null)
            throw new ArgumentNullException(argumentName);
    }
}