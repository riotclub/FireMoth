// <copyright file="FileFingerprintMap.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks.Output.Csv;

using CsvHelper.Configuration;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// A mapping between an <see cref="IFileFingerprint"/> and CSV headers.
/// </summary>
/// <seealso cref="ClassMap"/>
// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class FileFingerprintMap : ClassMap<FileFingerprint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileFingerprintMap"/> class.
    /// </summary>
    public FileFingerprintMap()
    {
        Map(fingerprint => fingerprint.FileName)
            .Name("FileName")
            .Index(0);
        Map(fingerprint => fingerprint.DirectoryName)
            .Name("DirectoryName")
            .Index(1);
        Map(fingerprint => fingerprint.FileSize)
            .Name("FileSize")
            .Index(2);
        Map(fingerprint => fingerprint.Base64Hash)
            .Name("Base64Hash")
            .Index(3);
    }
}