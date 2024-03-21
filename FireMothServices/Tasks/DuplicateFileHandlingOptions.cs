// <copyright file="DuplicateFileHandlingOptions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

/// <summary>
/// Contains options pertaining to how duplicate files are handled by
/// <see cref="DuplicateFileHandler"/>s.
/// </summary>
public class DuplicateFileHandlingOptions
{
    /// <summary>
    /// A <see cref="DuplicateFileHandlingMethod"/> used to indicate how duplicate files should be
    /// handled.
    /// </summary>
    public DuplicateFileHandlingMethod DuplicateFileHandlingMethod { get; set; }
    
    /// <summary>
    /// A flag indicating whether or not duplicate file deletion or moving should be user-
    /// interactive or not.
    /// </summary>
    public bool Interactive { get; set; }
    
    /// <summary>
    /// The full path where duplicate files will be moved to when
    /// <see cref="DuplicateFileHandlingMethod"/> is <see cref="DuplicateFileHandlingMethod.Move"/>.
    /// </summary>
    public string? MoveDuplicateFilesToDirectory { get; set; }
}