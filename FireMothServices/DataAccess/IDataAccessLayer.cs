// <copyright file="IDataAccessLayer.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using RiotClub.FireMoth.Services.Repository;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Defines the public interface for a class that implements data access and persistence operations.
    /// </summary>
    /// <typeparam name="TValue">The type of domain objects this data access layer manipulates.</typeparam>
    public interface IDataAccessLayer<TValue>
    {
        /// <summary>
        /// Retrieves values from the data layer.
        /// </summary>
        /// <param name="filter">A lambda expression that specifies a filter condition.</param>
        /// <param name="orderBy">A lambda expression that specifies an ordering.</param>
        /// <returns>A collection of values matching the specified filter with the specified ordering.</returns>
        public IEnumerable<TValue> Get(
            Func<IFileFingerprint, bool>? filter = null,
            Func<IFileFingerprint, string>? orderBy = null);

        /// <summary>
        /// Adds a value to the data layer.
        /// </summary>
        /// <param name="value">A value of type <typeparamref name="TValue"/> to add to the data layer.</param>
        public void Add(TValue value);

        /// <summary>
        /// Updates a value in the repository.
        /// </summary>
        /// <param name="value">A value of type <typeparamref name="TValue"/> to update in the data layer.</param>
        public bool Update(TValue value);

        /// <summary>
        /// Deletes a value from the repository.
        /// </summary>
        /// <param name="value">A value of type <typeparamref name="TValue"/> to delete from the data layer.</param>
        public bool Delete(TValue value);
    }
}
