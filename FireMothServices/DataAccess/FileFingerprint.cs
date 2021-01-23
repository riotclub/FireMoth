// <copyright file="FileFingerprint.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace FireMothServices.DataAccess
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
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            this.DirectoryName = fileInfo.DirectoryName;
            this.Name = fileInfo.Name;
            this.Length = fileInfo.Length;
            this.Base64Hash = base64Hash;
        }

        /// <summary>
        /// Gets the directory's full path.
        /// </summary>
        [Index(0)]
        public string DirectoryName { get; private set; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        [Index(1)]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the size, in bytes, of the file.
        /// </summary>
        [Index(2)]
        public long Length { get; private set; }

        /// <summary>
        /// Gets or sets the base-64 hash of the file's data.
        /// </summary>
        [Index(3)]
        public string Base64Hash
        {
            get => this.base64Hash;

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Hash string cannot be empty.");
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
