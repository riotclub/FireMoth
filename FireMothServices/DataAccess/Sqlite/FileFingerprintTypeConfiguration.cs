// <copyright file="FileFingerprintTypeConfiguration.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.Sqlite;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiotClub.FireMoth.Services.Repository;

internal class FileFingerprintTypeConfiguration : IEntityTypeConfiguration<FileFingerprint>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileFingerprint> builder)
    {
        builder.Property<int>("Id")
               .ValueGeneratedOnAdd();
        builder.HasKey("Id");
        builder.Property("FileName")
               .HasColumnType("varchar(256)");
        builder.Property("DirectoryName")
               .HasColumnType("varchar(1000)");
        builder.Property("FileSize");
        builder.Property("Base64Hash")
               .HasColumnType("char(44)");
    }
}