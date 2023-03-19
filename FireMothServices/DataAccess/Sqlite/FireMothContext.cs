// <copyright file="FireMothContext.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.EntityFrameworkSqlite
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using RiotClub.FireMoth.Services.Repository;

    internal class FireMothContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FireMothContext"/> class.
        /// </summary>
        public FireMothContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            this.DbPath = System.IO.Path.Join(path, "FireMoth.db");
        }

        public DbSet<FileFingerprint>? FileFingerprints { get; set; }

        public string DbPath { get; }

        /// <inheritdoc/>
        /// <seealso cref="FileFingerprintTypeConfiguration"/>
        protected override void OnModelCreating(ModelBuilder builder) =>
            new FileFingerprintTypeConfiguration().Configure(builder.Entity<FileFingerprint>());

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite($"Data Source={this.DbPath}");
    }
}
