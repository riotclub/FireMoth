// <copyright file="FileScanResultWriter.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;
using System.Collections.Generic;
using RiotClub.FireMoth.Services.Repository;
using System;

namespace RiotClub.FireMoth.Services.Output;

public class FileScanResultWriter : IFileFingerprintWriter
{
    public Task WriteFileFingerprintsAsync(IEnumerable<IFileFingerprint> fileFingerprints)
    {
        throw new NotImplementedException();
    }
}