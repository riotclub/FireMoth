// <copyright file="FileFingerprintMap.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using CsvHelper.Configuration;
    using RiotClub.FireMoth.Services.Repository;

    /// <summary>
    /// A mapping between an <see cref="IFileFingerprint"/> and CSV headers.
    /// </summary>
    /// <seealso cref="ClassMap"/>
    internal class FileFingerprintMap : ClassMap<IFileFingerprint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileFingerprintMap"/> class.
        /// </summary>
        public FileFingerprintMap()
        {
            this.Map(fingerprint => fingerprint.FileName)
                .Name("Name")
                .Index(0);
            this.Map(fingerprint => fingerprint.DirectoryName)
                .Name("DirectoryName")
                .Index(1);
            this.Map(fingerprint => fingerprint.FileSize)
                .Name("Length")
                .Index(2);
            this.Map(fingerprint => fingerprint.Base64Hash)
                .Name("Base64Hash")
                .Index(3);
        }
    }
}
