// <copyright file="FileFingerprint.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System;
    using System.IO.Abstractions;
    using CsvHelper.Configuration.Attributes;

    /// <summary>
    /// Conatins data that uniquely identifies a file and its data.
    /// </summary>
    public class FileFingerprint : IFileFingerprint
    {
        private string base64Hash;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileFingerprint"/> class.
        /// </summary>
        /// <param name="fileInfo">A <see cref="IFileInfo"/> containing information about the file.
        /// </param>
        /// <param name="base64Hash">A <see cref="string"/> containing a valid base 64 hash for the
        /// specified file.</param>
        public FileFingerprint(IFileInfo fileInfo, string base64Hash)
        {
            this.Base64Hash = base64Hash ?? throw new ArgumentNullException(nameof(base64Hash));

            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            this.FileInfo = fileInfo;
        }

        /// <inheritdoc/>
        public IFileInfo FileInfo { get; }

        /// <summary>
        /// Gets the base-64 hash of the file's data.
        /// </summary>
        public string Base64Hash
        {
            get => this.base64Hash;

            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Hash string cannot be null or empty.");
                }

                if (!IsBase64String(value))
                {
                    throw new ArgumentException(
                        "Hash string is not a valid base 64 string.", nameof(value));
                }

                this.base64Hash = value;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the provided string is a valid base 64 string.
        /// </summary>
        /// <param name="base64">The string to check.</param>
        /// <returns><c>true</c> if the provided string is a valid base 64 string.</returns>
        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}
