// <copyright file="IDataAccessLayer.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RiotClub.FireMoth.Services.Repository;

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
    public Task<IEnumerable<TValue>> GetAsync(
        Func<TValue, bool>? filter = null,
        Func<TValue, string>? orderBy = null);

    /// <summary>
    /// Adds a value to the data layer.
    /// </summary>
    /// <param name="value">A value of type <typeparamref name="TValue"/> to add to the data layer.</param>
    public Task AddAsync(TValue value);

    /// <summary>
    /// Adds a collection of values to the data layer.
    /// </summary>
    /// <param name="values">An <see cref="IEnumerable{TValue}"/> collection to add to the data layer.</param>
    public Task AddManyAsync(IEnumerable<TValue> values); 

    /// <summary>
    /// Deletes a value from the repository.
    /// </summary>
    /// <param name="value">A value of type <typeparamref name="TValue"/> to delete from the data layer.</param>
    public Task<bool> DeleteAsync(TValue value);
}