// <copyright file="FileFingerprintRepository.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Repository;

using System.Threading.Tasks;
using DataAccess;
using System;
using System.Collections.Generic;

/// <summary>
/// A repository that provides access to <see cref="IFileFingerprint"/>s.
/// </summary>
public class FileFingerprintRepository : IFileFingerprintRepository
{
    private readonly IDataAccessLayer<IFileFingerprint> _dataAccessLayer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileFingerprintRepository"/> class.
    /// </summary>
    /// <param name="dataAccessLayer">A <see cref="IDataAccessLayer{IFileFingerprint}"/> implementation used to
    /// persist data.</param>
    /// <exception cref="ArgumentNullException">If any of the provided services are <c>null</c>.</exception>
    public FileFingerprintRepository(IDataAccessLayer<IFileFingerprint> dataAccessLayer)
    {
        _dataAccessLayer = dataAccessLayer ?? throw new ArgumentNullException(nameof(dataAccessLayer));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IFileFingerprint>> GetAsync(
        Func<IFileFingerprint, bool>? filter = null,
        Func<IFileFingerprint, string>? orderBy = null) =>
        await _dataAccessLayer.GetAsync(filter, orderBy);

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(IFileFingerprint fileFingerprint) =>
        await _dataAccessLayer.DeleteAsync(fileFingerprint);

    /// <inheritdoc/>
    public async Task AddAsync(IFileFingerprint fileFingerprint) =>
        await _dataAccessLayer.AddAsync(fileFingerprint);

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(IFileFingerprint fileFingerprint) =>
        await _dataAccessLayer.UpdateAsync(fileFingerprint);
}