// <copyright file="OutputOption.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Output
{
    /// <summary>
    /// Options that control what file information is included in the output from a file scan
    /// operation.
    /// </summary>
    public enum OutputDuplicateFileFingerprintsOption
    {
        /// <summary>
        /// Output information for all scanned files.
        /// </summary>
        All,

        /// <summary>
        /// Output information for only duplicate files.
        /// </summary>
        Duplicates,
    }
}
