// <copyright file="SqliteDataAccessLayer.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.Sqlite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// Implementation of a data access layer that persists data to a SQLite database.
/// </summary>
public class SqliteDataAccessLayer : IDataAccessLayer<IFileFingerprint>
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
    public Task<IEnumerable<IFileFingerprint>> GetAsync(
        Func<IFileFingerprint, bool>? filter = null,
        Func<IFileFingerprint, string>? orderBy = null)
    {
        _logger.LogInformation("SqliteDataAccessLayer: GET");

        var result = new List<IFileFingerprint>();
        
        return Task.FromResult(result.Where(filter));
    }

    /// <inheritdoc/>
    public Task AddAsync(IFileFingerprint value)
    {
        _logger.LogInformation("SqliteDataAccessLayer: ADD");

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task AddManyAsync(IEnumerable<IFileFingerprint> values)
    {
        _logger.LogInformation("SqliteDataAccessLayer: ADDMANY");

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> UpdateAsync(IFileFingerprint value)
    {
        _logger.LogInformation("SqliteDataAccessLayer: UPDATE");

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(IFileFingerprint value)
    {
        _logger.LogInformation("SqliteDataAccessLayer: DELETE");

        return Task.FromResult(false);
    }
}