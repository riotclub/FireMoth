// <copyright file="FireMothContext.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.Sqlite;

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using RiotClub.FireMoth.Services.Repository;

/// <summary>An implementation of <see cref="DbContext"/> that represents a session with the
/// FireMoth database backing store.</summary>
/// <remarks>Generally, this class should not be used for direct database access. Instead, use a
/// repository abstraction that implements <see cref="IFileFingerprintRepository"/>.</remarks>
/// <seealso cref="FileFingerprintRepository"/>
public class FireMothContext : DbContext
{
    /// <summary>A <see cref="DbSet{FileFingerprint}"/> used to query the available set of
    /// <see cref="FileFingerprint"/>s in the database.</summary>
    public virtual DbSet<FileFingerprint> FileFingerprints { get; set; }

    /// <summary>Initializes a new instance of the <see cref="FireMothContext"/> class.</summary>
    /// <param name="options">A <see cref="DbContextOptions{FireMothContext}"/> containing
    /// <see cref="DbContext"/> options for this context.</param>
    public FireMothContext(DbContextOptions<FireMothContext> options) : base(options)
    { }
    
    /// <summary>Initializes a new instance of the <see cref="FireMothContext"/> class.</summary>
    public FireMothContext()
    { }

    /// <inheritdoc/>
    /// <seealso cref="FileFingerprintTypeConfiguration"/>
    protected override void OnModelCreating(ModelBuilder builder) =>
        new FileFingerprintTypeConfiguration().Configure(builder.Entity<FileFingerprint>());
}