// <copyright file="FileFingerprint.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace FireMothServices.DataAccess
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Runtime.CompilerServices;
    using CsvHelper.Configuration.Attributes;

    /// <summary>
    /// Conatins data that uniquely identifies a file and its data.
    /// </summary>
    public class FileFingerprint : IFileFingerprint
    {
        private string base64Hash;
        private long length;

        /// <summary>
        /// Gets or sets a string representing the directory's full path.
        /// </summary>
        [Index(0)]
        public string DirectoryName { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        [Index(1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the size, in bytes, of the file.
        /// </summary>
        [Index(2)]
        public long Length
        {
            get => this.length;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), "Length cannot be less than zero.");
                }

                this.length = value;
            }
        }

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
        /// Builds a <see cref="FileFingerprint"/> object from the provided <see cref="IFileInfo"/>
        /// and base 64 hash string.
        /// </summary>
        /// <param name="fileInfo">The <see cref="IFileInfo"/> implementation containing the file
        /// information to use for the returned <see cref="FileFingerprint"/>.</param>
        /// <param name="base64Hash">A <c>string</c> containing the base 64 hash string to use in
        /// the returned <see cref="FileFingerprint"/>.</param>
        /// <returns>A new <see cref="FileFingerprint"/> object containing the provided
        /// information.</returns>
        public static FileFingerprint CreateFileFingerprint(IFileInfo fileInfo, string base64Hash)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            return new FileFingerprint
            {
                DirectoryName = new FileSystem().Path.GetDirectoryName(fileInfo.FullName),
                Name = fileInfo.Name,
                Length = fileInfo.Length,
                Base64Hash = base64Hash,
            };
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
