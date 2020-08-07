// <copyright file="IDataAccessProvider.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    //using Microsoft.Extensions.FileProviders;
    using System.IO.Abstractions;

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
    }
}
