// <copyright file="IFileFingerprint.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace FireMothServices.DataAccess
{
    public interface IFileFingerprint
    {
        string DirectoryName { get; set; }

        string Name { get; set; }

        long Length { get; set; }

        string Base64Hash { get; set; }
    }
}
