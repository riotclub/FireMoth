// <copyright file="ScanResult.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    /// <summary>
    /// Specifies the result of a file scan operation.
    /// </summary>
    public enum ScanResult
    {
        /// <summary>
        /// Inidicates the file scan operation was successful.
        /// </summary>
        ScanSuccess,

        /// <summary>
        /// Indicates the file scan operation encountered a problem that prevented it from completing successfully.
        /// </summary>
        ScanFailure,
    }
}
