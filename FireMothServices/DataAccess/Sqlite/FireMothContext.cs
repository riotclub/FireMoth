// <copyright file="FireMothContext.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.Sqlite;

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using RiotClub.FireMoth.Services.DataAccess.EntityFrameworkSqlite;
using RiotClub.FireMoth.Services.Repository;

internal class FireMothContext : DbContext
{
    public DbSet<FileFingerprint> FileFingerprints { get; set; }

    /// <summary>
    /// Subfolder within system application data folder where the database file is located.
    /// </summary>
    /// <seealso cref="Environment.SpecialFolder.LocalApplicationData"/>
    public const string DbAppdataSubfolder = "FireMoth";

    public const string DbFileName = "FireMoth_FileFingerprints.db";
    
    public string DbPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FireMothContext"/> class.
    /// </summary>
    public FireMothContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Path.Combine(Environment.GetFolderPath(folder), DbAppdataSubfolder);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        DbPath = Path.Join(path, DbFileName);
    }

    /// <inheritdoc/>
    /// <seealso cref="FileFingerprintTypeConfiguration"/>
    protected override void OnModelCreating(ModelBuilder builder) =>
        new FileFingerprintTypeConfiguration().Configure(builder.Entity<FileFingerprint>());

    // TODO: Move DB connection string to configuration 
    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlite($"Data Source={DbPath}");
}