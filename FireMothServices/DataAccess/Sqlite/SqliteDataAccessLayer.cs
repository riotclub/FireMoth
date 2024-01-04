// <copyright file="SqliteDataAccessLayer.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.Sqlite;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// Implementation of a data access layer that persists data to a SQLite database.
/// </summary>
public class SqliteDataAccessLayer : IDataAccessLayer<FileFingerprint>
{
    private readonly ILogger<SqliteDataAccessLayer> _logger;
    private readonly FireMothContext _fireMothContext;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDataAccessLayer"/> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> used to log information.</param>
    /// <param name="context">A <see cref="FireMothContext"/> instance representing the database
    /// connection.</param>
    public SqliteDataAccessLayer(ILogger<SqliteDataAccessLayer> logger, FireMothContext context)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(context);

        _logger = logger;
        _fireMothContext = context;
    }
    
    /// <inheritdoc/>
    public Task<IEnumerable<FileFingerprint>> GetAsync(
        Func<FileFingerprint, bool>? filter = null,
        Func<FileFingerprint, string>? orderBy = null)
    {
        IEnumerable<FileFingerprint> result = _fireMothContext.FileFingerprints;
        
        if (filter is not null)
            result = result.Where(filter);

        if (orderBy is not null)
            result = result.OrderBy(orderBy);

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task AddAsync(FileFingerprint fileFingerprint)
    {
        Guard.IsNotNull(fileFingerprint);
        _logger.LogDebug(
            "SqliteDataAccessLayer: Writing fingerprint {FileFingerprint}.", fileFingerprint);
        
        _fireMothContext.FileFingerprints.Add(fileFingerprint);
        await _fireMothContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task AddManyAsync(IEnumerable<FileFingerprint> fileFingerprints)
    {
        Guard.IsNotNull(fileFingerprints);
        var fileFingerprintList = fileFingerprints.ToList();
        _logger.LogDebug(
            "SqliteDataAccessLayer: Writing {FileFingerprintCount} fingerprint(s).",
            fileFingerprintList.Count);
        
        _fireMothContext.AddRange(fileFingerprintList);
        await _fireMothContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(FileFingerprint fileFingerprint)
    {
        Guard.IsNotNull(fileFingerprint);
        _logger.LogDebug(
            "SqliteDataAccessLayer: Deleting fingerprint {FileFingerprint}", fileFingerprint);
        
        // TODO: Figure out a better way to do this. Ideally, we'd do a simple Equals comparison
        //       (i.e., fp => fp.Equals(...)) to keep the code DRY and prevent future maintenance
        //       issues. However, EntityFramework translates the Equals call to a primary key
        //       comparison. Because of this, we're doing a memberwise comparison here as a
        //       workaround.
        var fingerprintToDelete = await _fireMothContext.FileFingerprints
            .FirstOrDefaultAsync(fp =>
                fp.FileName == fileFingerprint.FileName
                && fp.DirectoryName == fileFingerprint.DirectoryName
                && fp.FileSize == fileFingerprint.FileSize
                && fp.Base64Hash == fileFingerprint.Base64Hash);
        if (fingerprintToDelete is null)
            return false;
        
        _fireMothContext.Remove(fingerprintToDelete);
        await _fireMothContext.SaveChangesAsync();
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<int> DeleteAllAsync()
    {
        return await _fireMothContext.FileFingerprints.ExecuteDeleteAsync();
    }
}