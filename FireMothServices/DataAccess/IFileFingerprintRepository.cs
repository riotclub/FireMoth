// <copyright file="IFileFingerprintRepository.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FireMoth.Services.Tests")]

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Defines the public interface for a class that implements a repository of
    /// <see cref="IFileFingerprint"/>s.
    /// </summary>
    public interface IFileFingerprintRepository
    {
        /// <summary>
        /// Adds a file fingerprint to the repository.
        /// </summary>
        /// <param name="fileFingerprint">A <see cref="IFileFingerprint"/> to add to the repository.
        /// </param>
        void Add(IFileFingerprint fileFingerprint);

        /// <summary>
        /// Retrieves all file fingerprints with paths matching the specified path.
        /// </summary>
        /// <param name="filter">Filter.</param>
        /// <param name="orderBy">OrderBy.</param>
        /// <returns>IEnumerable collection of file fingerprints.</returns>
        IEnumerable<IFileFingerprint> Get(
            Expression<Func<IFileFingerprint, bool>>? filter = null,
            Func<IQueryable<IFileFingerprint>, IOrderedQueryable<IFileFingerprint>>? orderBy = null);
    }
}
