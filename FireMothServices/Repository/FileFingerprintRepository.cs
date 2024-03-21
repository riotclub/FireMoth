// <copyright file="FileFingerprintRepository.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Repository;

using System.Threading.Tasks;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A repository that provides access to <see cref="IFileFingerprint"/>s.
/// </summary>
public class FileFingerprintRepository : IFileFingerprintRepository
{
    private readonly IDataAccessLayer<FileFingerprint> _dataAccessLayer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileFingerprintRepository"/> class.
    /// </summary>
    /// <param name="dataAccessLayer">A <see cref="IDataAccessLayer{IFileFingerprint}"/>
    /// implementation used to persist data.</param>
    /// <exception cref="ArgumentNullException">If any of the provided services are <c>null</c>.
    /// </exception>
    public FileFingerprintRepository(IDataAccessLayer<FileFingerprint> dataAccessLayer)
    {
        _dataAccessLayer = dataAccessLayer 
                           ?? throw new ArgumentNullException(nameof(dataAccessLayer));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<FileFingerprint>> GetAsync(
        Func<FileFingerprint, bool>? filter = null,
        Func<FileFingerprint, string>? orderBy = null) =>
        await _dataAccessLayer.GetAsync(filter, orderBy);

    /// <inheritdoc/>
    public async Task<IEnumerable<FileFingerprint>> GetRecordsWithDuplicateHashesAsync()
    {
        return (await GetGroupingsWithDuplicateHashesAsync())
            .SelectMany(duplicateGroup => duplicateGroup);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IGrouping<string, FileFingerprint>>> 
        GetGroupingsWithDuplicateHashesAsync()
    {
        var allFingerprints = await _dataAccessLayer.GetAsync();
        return allFingerprints.GroupBy(fp => fp.Base64Hash)
            .Where(group => group.Count() > 1);
    }
    
    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(FileFingerprint fileFingerprint) =>
        await _dataAccessLayer.DeleteAsync(fileFingerprint);

    /// <inheritdoc/>
    public async Task<int> DeleteAllAsync() =>
        await _dataAccessLayer.DeleteAllAsync();

    /// <inheritdoc/>
    public async Task AddAsync(FileFingerprint fileFingerprint) =>
        await _dataAccessLayer.AddAsync(fileFingerprint);
}