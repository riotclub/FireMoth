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
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// Implementation of a data access layer that persists data to a SQLite database.
/// </summary>
public class SqliteDataAccessLayer : IDataAccessLayer<FileFingerprint>
{
    private readonly ILogger<SqliteDataAccessLayer> _logger;
    private readonly FireMothContext _fireMothContext;
    
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
    public Task AddAsync(FileFingerprint fileFingerprint)
    {
        Guard.IsNotNull(fileFingerprint);

        var fullPath = Path.Combine(fileFingerprint.DirectoryName, fileFingerprint.FileName);
        _logger.LogDebug(
            "Writing fingerprint for file {FileName} with hash {HashString}.",
            fullPath,
            fileFingerprint.Base64Hash);

        _fireMothContext.FileFingerprints.Add(fileFingerprint);
        _fireMothContext.SaveChanges();
        
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public Task AddManyAsync(IEnumerable<FileFingerprint> fileFingerprints)
    {
        Guard.IsNotNull(fileFingerprints);

        var fileFingerprintList = fileFingerprints.ToList();
        _logger.LogDebug("Writing {FileFingerprintCount} fingerprint(s).", fileFingerprintList.Count);
        _fireMothContext.AddRange(fileFingerprintList);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(FileFingerprint value)
    {
        _logger.LogInformation("SqliteDataAccessLayer: DELETE");

        return Task.FromResult(false);
    }
}