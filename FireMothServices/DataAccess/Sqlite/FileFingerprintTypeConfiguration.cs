// <copyright file="FileFingerprintTypeConfiguration.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess.EntityFrameworkSqlite;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiotClub.FireMoth.Services.Repository;
using System.IO.Abstractions;

internal class FileFingerprintTypeConfiguration : IEntityTypeConfiguration<FileFingerprint>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileFingerprint> builder)
    {
        builder.HasKey(f => new { f.DirectoryName, f.FileName, f.Base64Hash });

        //builder.Property(f => f.FileInfo.DirectoryName)
        //       .HasColumnName("Directory");

        //builder.Property(f => f.FileInfo.Name)
        //       .HasColumnName("FileName");

        //builder.Property(f => f.FileInfo.Length)
        //       .HasColumnName("FileSize");

        //builder.Property(f => f.Base64Hash)
        //       .HasColumnName("Base64Hash");

        builder.Property("DirectoryName")
            .HasColumnType("varchar(500)")
            .HasColumnName("DirectoryName");

        builder.Property("FileName")
            .HasColumnName("FileName");

        builder.Property("FileSize")
            .HasColumnName("FileSize");

        builder.Property("Base64Hash")
            .HasColumnName("Base64Hash");
    }
}