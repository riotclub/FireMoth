// <copyright file="IDataAccessProvider.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using RiotClub.FireMoth.Services.DataAccess.Model;

    /// <summary>
    /// Defines the public interface for a class that implements data access and persistence
    /// operations.
    /// </summary>
    public interface IDataAccessProvider
    {
        /// <summary>
        /// Adds a file and its hash value to the backing store.
        /// </summary>
        /// <param name="fileFingerprint">An <see cref="IFileFingerprint"/> containing the
        /// properties of the file to store.</param>
        public void AddFileRecord(IFileFingerprint fileFingerprint);
    }
}
