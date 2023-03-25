// <copyright file="CsvFileFingerprintWriter.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using RiotClub.FireMoth.Services.Repository;

namespace RiotClub.FireMoth.Services.Output;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CsvFileFingerprintWriter : IFileFingerprintWriter
{
    public Task WriteFileFingerprintsAsync(IEnumerable<IFileFingerprint> fileFingerprints)
    {
        throw new NotImplementedException();
    }
}