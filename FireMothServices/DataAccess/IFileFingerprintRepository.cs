// <copyright file="IFileFingerprintRepository.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System.Collections.Generic;

    internal interface IFileFingerprintRepository
    {
        void Add(IFileFingerprint fileFingerprint);

        IFileFingerprint GetByPath(string filePath);

        IEnumerable<IFileFingerprint> GetByHash(string hash);
    }
}
