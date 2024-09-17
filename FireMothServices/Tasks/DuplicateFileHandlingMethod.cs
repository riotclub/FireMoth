// <copyright file="DuplicateHandlingMethod.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

/// <summary>
/// Specifies the method for handling duplicates at the end of the scanning process.
/// </summary>
public enum DuplicateFileHandlingMethod
{
    /// <summary>
    /// Specifies that nothing should be done to duplicate files after scanning.
    /// </summary>
    NoAction,
    
    /// <summary>
    /// Specifies that duplicate files are deleted after scanning. 
    /// </summary>
    Delete,
    
    /// <summary>
    /// Specifies that duplicate files should be moved to a separate directory after scanning.
    /// </summary>
    Move
}