// <copyright file="IFileFingerprint.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace FireMothServices.DataAccess
{
    /// <summary>
    /// Defines the public interface for a class that implements properties that define a file and
    /// its hash value.
    /// </summary>
    public interface IFileFingerprint
    {
        /// <summary>
        /// Gets the directory that the file resides in.
        /// </summary>
        string DirectoryName { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the size, in bytes, of the file.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Gets a base 64 string representation of the file's hash.
        /// </summary>
        string Base64Hash { get; }
    }
}
