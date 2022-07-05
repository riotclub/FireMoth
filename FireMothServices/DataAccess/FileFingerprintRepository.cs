// <copyright file="FileFingerprintRepository.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A repository of <see cref="IFileFingerprint"/>s utilizing Entity Framework as a backing
    /// store.
    /// </summary>
    public class FileFingerprintRepository : IFileFingerprintRepository
    {
        /// <inheritdoc/>
        public IEnumerable<IFileFingerprint> Get(
            Expression<Func<IFileFingerprint, bool>>? filter = null,
            Func<IQueryable<IFileFingerprint>, IOrderedQueryable<IFileFingerprint>>? orderBy = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        void IFileFingerprintRepository.Delete(IFileFingerprint fileFingerprint)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        void IFileFingerprintRepository.Insert(IFileFingerprint fileFingerprint)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        void IFileFingerprintRepository.Update(IFileFingerprint fileFingerprint)
        {
            throw new NotImplementedException();
        }
    }
}
