// <copyright file="ITaskHandler.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

using System.Threading.Tasks;

/// <summary>
/// Defines the public interface for a class that runs a task.
/// </summary>
public interface ITaskHandler
{
    /// <summary>
    /// Performs the task for this task handler.
    /// </summary>
    /// <returns>An asynchronous <see cref="Task"/> with a <see cref="bool"/> result indicating
    /// whether the task completed successfully (<c>true</c>) or failed (<c>false</c>).</returns>
    public Task<bool> RunTaskAsync();
}