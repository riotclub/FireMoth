// <copyright file="InteractiveDuplicateHandlerOptions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console.Tasks;

/// <summary>Specifies options used to define the behavior of
/// <see cref="InteractiveDuplicateHandler"/>.</summary>
public class InteractiveDuplicateHandlerOptions
{
    /// <summary>Gets or sets a value indicating whether to use the operating system shell to start
    /// the "open files" operation.</summary>
    public bool UseShellExecute { get; init; }
    
    /// <summary>Gets or sets a value specifying the application to use when using the "open files"
    /// operation.</summary>
    public string? Application { get; init; }
    
    /// <summary>Gets or sets a value specifying arguments to the application specified in
    /// <see cref="Application"/>.</summary>
    public string? Arguments { get; init; }
}