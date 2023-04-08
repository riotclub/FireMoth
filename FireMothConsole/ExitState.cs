// <copyright file="ExitState.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console;

/// <summary>
/// Specifies the cause of program termination.
/// </summary>
public enum ExitState
{
    /// <summary>
    /// Inidicates nominal program shutdown.
    /// </summary>
    Normal,

    /// <summary>
    /// Inidicates an error occurred during program initialization.
    /// </summary>
    StartupError,

    /// <summary>
    /// Indicates an error occurred after program initialization.
    /// </summary>
    RuntimeError,
}