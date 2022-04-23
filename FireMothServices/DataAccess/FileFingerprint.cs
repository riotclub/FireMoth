﻿// <copyright file="FileFingerprint.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System;
    using System.IO.Abstractions;
    using RiotClub.FireMoth.Services.Extensions;

    /// <summary>
    /// Conatins data that uniquely identifies a file and its data.
    /// </summary>
    public class FileFingerprint : IFileFingerprint, IEquatable<FileFingerprint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileFingerprint"/> class.
        /// </summary>
        /// <param name="fileInfo">A <see cref="IFileInfo"/> containing information about the file.
        /// </param>
        /// <param name="base64Hash">A <see cref="string"/> containing a valid base 64 hash for the
        /// specified file.</param>
        public FileFingerprint(IFileInfo fileInfo, string base64Hash)
        {
            this.FileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));

            if (base64Hash == null)
            {
                throw new ArgumentNullException(nameof(base64Hash));
            }

            if (base64Hash.IsEmptyOrWhiteSpace())
            {
                throw new ArgumentException("Hash string cannot be empty.");
            }

            if (!base64Hash.IsBase64String())
            {
                throw new ArgumentException(
                    "Hash string is not a valid base 64 string.", nameof(base64Hash));
            }

            this.Base64Hash = base64Hash;
        }

        /// <inheritdoc/>
        public IFileInfo FileInfo { get; }

        /// <summary>
        /// Gets the base-64 hash of the file's data.
        /// </summary>
        public string Base64Hash { get; }

        /// <summary>
        /// Gets the UTC <see cref="DateTime"/> at which this <see cref="FileFingerprint"/> was
        /// created.
        /// </summary>
        public DateTime CreatedDateTime { get; } = DateTime.UtcNow;

        /// <summary>
        /// Implements the equality operator.
        /// </summary>
        /// <param name="left">An instance of <see cref="FileFingerprint"/> to test for equality.
        /// </param>
        /// <param name="right">A second instance of <see cref="FileFingerprint"/> to test for
        /// equality.</param>
        /// <returns><c>true</c> if the two <see cref="FileFingerprint"/>s are equal; false
        /// otherwise.</returns>
        public static bool operator ==(FileFingerprint? left, FileFingerprint? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Implements the inequality operator.
        /// </summary>
        /// <param name="left">An instance of <see cref="FileFingerprint"/> to test for inequality.
        /// </param>
        /// <param name="right">A second instance of <see cref="FileFingerprint"/> to test for
        /// inequality.</param>
        /// <returns><c>true</c> if the two <see cref="FileFingerprint"/>s are not equal; false
        /// otherwise.</returns>
        public static bool operator !=(FileFingerprint? left, FileFingerprint? right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return this.Equals(obj as FileFingerprint);
        }

        /// <inheritdoc/>
        public bool Equals(FileFingerprint? other)
        {
            return other is FileFingerprint fingerprint
                && this.FileInfo.FullName == fingerprint.FileInfo.FullName
                && this.Base64Hash == fingerprint.Base64Hash;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Base64Hash);
        }
    }
}
