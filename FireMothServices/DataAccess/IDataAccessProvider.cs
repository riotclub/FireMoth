// <copyright file="IDataAccessProvider.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System.IO.Abstractions;
    using FireMothServices.DataAccess;

    /// <summary>
    /// Defines the public interface for a class that implements data access and persistence operations.
    /// </summary>
    public interface IDataAccessProvider
    {
        /// <summary>
        /// Adds a file and its hash value to the backing store.
        /// </summary>
        /// <param name="fileInfo">A <see cref="IFileInfo"/> containing the properties of the file to store.</param>
        /// <param name="base64Hash">The hash value of the file represented by <paramref name="fileInfo"/> in base 64.
        /// </param>
        public void AddFileRecord(IFileInfo fileInfo, string base64Hash);

        /// <summary>
        /// Adds a file and its hash value to the backing store.
        /// </summary>
        /// <param name="fileFingerprint">An <see cref="IFileFingerprint"/> containing the properties of the file to
        /// store.</param>
        public void AddFileRecord(IFileFingerprint fileFingerprint);

        /*
        /// <summary>
        /// Retrieves a <see cref="FileFingerprint"/> record from the currently available collection of file records.
        /// </summary>
        /// <param name="name">The name of the file to retrieve.</param>
        /// <param name="directory">The path of the file to retrieve.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> collection of <see cref="FileFingerprint"/> records that match the
        /// provided search criteria. If no matching records are found, an empty <see cref="IEnumerable{T}"/> is
        /// returned.</returns>
        public IEnumerable<FileFingerprint> GetFileRecord(string name, string directory);
        */
    }
}
