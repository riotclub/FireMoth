// <copyright file="FileFingerprintMap.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using CsvHelper.Configuration;
    using RiotClub.FireMoth.Services.DataAccess.Model;

    /// <summary>
    /// A mapping between an <see cref="System.IO.Abstractions.IFileInfo"/> and CSV headers.
    /// </summary>
    /// <seealso cref="ClassMap"/>
    public class FileFingerprintMap : ClassMap<IFileFingerprint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileFingerprintMap"/> class.
        /// </summary>
        public FileFingerprintMap()
        {
            this.Map(fingerprint => fingerprint.FileInfo.DirectoryName)
                .Name("DirectoryName")
                .Index(0);
            this.Map(fingerprint => fingerprint.FileInfo.Name)
                .Name("Name")
                .Index(1);
            this.Map(fingerprint => fingerprint.FileInfo.Length)
                .Name("Length")
                .Index(2);
            this.Map(fingerprint => fingerprint.Base64Hash)
                .Name("Base64Hash")
                .Index(3);
        }
    }
}
