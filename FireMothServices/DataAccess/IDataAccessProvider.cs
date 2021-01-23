// <copyright file="IDataAccessProvider.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using FireMothServices.DataAccess;

    /// <summary>
    /// Defines the public interface for a class that implements data access and persistence operations.
    /// </summary>
    public interface IDataAccessProvider
    {
        /// <summary>
        /// Adds a file and its hash value to the backing store.
        /// </summary>
        /// <param name="fileFingerprint">An <see cref="IFileFingerprint"/> containing the properties of the file to
        /// store.</param>
        public void AddFileRecord(IFileFingerprint fileFingerprint);
    }
}
