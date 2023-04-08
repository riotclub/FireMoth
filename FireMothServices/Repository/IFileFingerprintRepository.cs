// <copyright file="IFileFingerprintRepository.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;

namespace RiotClub.FireMoth.Services.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
    public Task AddAsync(IFileFingerprint fileFingerprint);

    /// <summary>
    /// Retrieves file fingerprints from the repository.
    /// </summary>
    /// <param name="filter">A lambda expression that specifies a filter condition.</param>
    /// <param name="orderBy">A lambda expression that specifies an ordering.</param>
    /// <returns>IEnumerable collection of file fingerprints.</returns>
    public Task<IEnumerable<IFileFingerprint>> GetAsync(
        Func<IFileFingerprint, bool>? filter = null,
        Func<IFileFingerprint, string>? orderBy = null);

    /// <summary>
    /// Updates a file fingerprint in the repository.
    /// </summary>
    /// <param name="fileFingerprint">The <see cref="IFileFingerprint"/> to update.</param>
    public Task<bool> UpdateAsync(IFileFingerprint fileFingerprint);

    /// <summary>
    /// Deletes a file fingerprint from the repository.
    /// </summary>
    /// <param name="fileFingerprint">The <see cref="IFileFingerprint"/> to delete.</param>
    public Task<bool> DeleteAsync(IFileFingerprint fileFingerprint);
}