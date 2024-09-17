// <copyright file="FireMothContext.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.Sqlite;

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using RiotClub.FireMoth.Services.Repository;

public class FireMothContext : DbContext
{
    public virtual DbSet<FileFingerprint> FileFingerprints { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FireMothContext"/> class.
    /// </summary>
    public FireMothContext(DbContextOptions<FireMothContext> options) : base(options)
    {
    }
    
    public FireMothContext()
    { }

    /// <inheritdoc/>
    /// <seealso cref="FileFingerprintTypeConfiguration"/>
    protected override void OnModelCreating(ModelBuilder builder) =>
        new FileFingerprintTypeConfiguration().Configure(builder.Entity<FileFingerprint>());
}