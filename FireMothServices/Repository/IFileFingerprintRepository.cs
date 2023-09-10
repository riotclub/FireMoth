// <copyright file="IFileFingerprintRepository.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;

namespace RiotClub.FireMoth.Services.Repository;

using System;
using System.Collections.Generic;

/// <summary>
/// Defines the public interface for a class that implements a repository of
/// <see cref="IFileFingerprint"/>s.
/// </summary>
public interface IFileFingerprintRepository
{
    /// <summary>
    /// Adds a file fingerprint to the repository.
    /// </summary>
    /// <param name="fileFingerprint">A <see cref="IFileFingerprint"/> to add to the repository.
    /// </param>
    public Task AddAsync(FileFingerprint fileFingerprint);

    /// <summary>
    /// Retrieves file fingerprints from the repository.
    /// </summary>
    /// <param name="filter">A lambda expression that specifies a filter condition.</param>
    /// <param name="orderBy">A lambda expression that specifies an ordering.</param>
    /// <returns>IEnumerable collection of file fingerprints.</returns>
    public Task<IEnumerable<FileFingerprint>> GetAsync(
        Func<FileFingerprint, bool>? filter = null,
        Func<FileFingerprint, string>? orderBy = null);

    /// <summary>
    /// Retrieves a collection of file fingerprints that have hash values that match other file
    /// fingerprints in the repository.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{FileFingerprint}"/> containing the file fingerprints.
    /// </returns>
    public Task<IEnumerable<FileFingerprint>> GetFileFingerprintsWithDuplicateHashesAsync();
    
    /// <summary>
    /// Deletes a file fingerprint from the repository.
    /// </summary>
    /// <param name="fileFingerprint">The <see cref="IFileFingerprint"/> to delete.</param>
    public Task<bool> DeleteAsync(FileFingerprint fileFingerprint);

    /// <summary>
    /// Deletes all file fingerprints from the repository.
    /// </summary>
    public Task<int> DeleteAllAsync();
}