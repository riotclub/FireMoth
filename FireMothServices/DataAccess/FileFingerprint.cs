// <copyright file="FileFingerprint.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace FireMothServices.DataAccess
{
    using CsvHelper.Configuration.Attributes;

    /// <summary>
    /// Conatins data that uniquely identifies a file and its data.
    /// </summary>
    public class FileFingerprint
    {
        /// <summary>
        /// Gets or sets the path to the file.
        /// </summary>
        [Index(0)]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        [Index(1)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the base-64 hash of the file's data.
        /// </summary>
        [Index(2)]
        public string Base64Hash { get; set; }
    }
}
