﻿// <copyright file="DuplicateFileHandlingOptions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

/// <summary>Contains options pertaining to how duplicate files are handled by
/// <see cref="ITaskHandler"/> impementations.</summary>
public class DuplicateFileHandlingOptions
{
    /// <summary>A <see cref="DuplicateFileHandlingMethod"/> used to indicate how duplicate files
    /// should be handled.</summary>
    public DuplicateFileHandlingMethod DuplicateFileHandlingMethod { get; init; }
    
    /// <summary>A flag indicating whether duplicate file deletion or moving should be
    /// user-interactive or not.</summary>
    public bool Interactive { get; init; }
    
    /// <summary>The full path where duplicate files will be moved to when
    /// <see cref="DuplicateFileHandlingMethod"/> is <see cref="DuplicateFileHandlingMethod.Move"/>.
    /// </summary>
    public string? MoveDuplicateFilesToDirectory { get; init; }
}