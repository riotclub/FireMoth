// <copyright file="FileFingerprint.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Repository;

using System;
using Extensions;

/// <summary>
/// Contains data that uniquely identifies a file and its data.
/// </summary>
public class FileFingerprint : IFileFingerprint, IEquatable<FileFingerprint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileFingerprint"/> class.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="directoryName">The full path of the directory containing the file.</param>
    /// <param name="fileSize">The size of the file in bytes.</param>
    /// <param name="base64Hash">A <see cref="string"/> containing a valid base 64 hash for the
    /// specified file.</param>
    public FileFingerprint(
        string fileName, string directoryName, long fileSize, string base64Hash)
    {
        DirectoryName = directoryName
                        ?? throw new ArgumentNullException(nameof(directoryName));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        FileSize = fileSize;

        ThrowIfHashInvalid(base64Hash);
        Base64Hash = base64Hash;
    }

    /// <inheritdoc/>
    public string DirectoryName { get; }

    /// <inheritdoc/>
    public string FileName { get; }

    /// <inheritdoc/>
    public long FileSize { get; }

    /// <inheritdoc/>
    public string Base64Hash { get; }

    /// <inheritdoc/>
    public string FullPath => System.IO.Path.Combine(DirectoryName, FileName);

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
    public static bool operator !=(FileFingerprint? left, FileFingerprint? right) =>
        !(left == right);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as FileFingerprint);

    /// <inheritdoc/>
    public bool Equals(FileFingerprint? other) =>
        other is FileFingerprint fingerprint
        && FileName == fingerprint.FileName
        && DirectoryName == fingerprint.DirectoryName
        && FileSize == fingerprint.FileSize
        && Base64Hash == fingerprint.Base64Hash;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        FileName, DirectoryName, FileSize, Base64Hash);

    /// <inheritdoc/>
    public override string ToString()
    {
        return
            "File:" + System.IO.Path.Combine(DirectoryName, FileName)
                    + ",FileSize:" + FileSize
                    + ",Hash:" + Base64Hash;
    }

    private static void ThrowIfHashInvalid(string base64Hash)
    {
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
    }
}