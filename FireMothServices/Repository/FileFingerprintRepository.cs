// <copyright file="FileFingerprintRepository.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.DataAccess;

namespace RiotClub.FireMoth.Services.Repository
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A repository of <see cref="IFileFingerprint"/>s utilizing Entity Framework as a backing
    /// store.
    /// </summary>
    public class FileFingerprintRepository : IFileFingerprintRepository
    {
        private readonly IDataAccessLayer<IFileFingerprint> _dataAccessLayer;
        private readonly ILogger<FileFingerprintRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileFingerprintRepository"/> class.
        /// </summary>
        /// <param name="dataAccessLayer">A <see cref="IDataAccessLayer{IFileFingerprint}"/> implementation used to
        /// persist data.</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation used to log pertinent information.</param>
        /// <exception cref="ArgumentNullException">If any of the provided services are <c>null</c>.</exception>
        public FileFingerprintRepository(
            IDataAccessLayer<IFileFingerprint> dataAccessLayer,
            ILogger<FileFingerprintRepository> logger)
        {
            _dataAccessLayer = dataAccessLayer ?? throw new ArgumentNullException(nameof(dataAccessLayer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public IEnumerable<IFileFingerprint> Get(
            Func<IFileFingerprint, bool>? filter = null, Func<IFileFingerprint, string>? orderBy = null)
        {
            return _dataAccessLayer.Get(filter, orderBy);
        }

        /// <inheritdoc/>
        public bool Delete(IFileFingerprint fileFingerprint)
        {
            return _dataAccessLayer.Delete(fileFingerprint);
        }

        /// <inheritdoc/>
        public void Add(IFileFingerprint fileFingerprint)
        {
            _dataAccessLayer.Add(fileFingerprint);
        }

        /// <inheritdoc/>
        public bool Update(IFileFingerprint fileFingerprint)
        {
            return _dataAccessLayer.Update(fileFingerprint);
        }
    }
}
