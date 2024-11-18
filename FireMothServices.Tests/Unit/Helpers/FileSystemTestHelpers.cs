// <copyright file="FileSystemTestHelpers.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.Helpers;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

public static class FileSystemTestHelpers
{
    public static MockFileSystem BuildMockFileSystem()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { "/RootDirFile", new MockFileData(Guid.NewGuid().ToString()) },
            { "/RootDirFile2", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/TestFile.txt", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/AnotherFile.dat", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/YetAnotherFile.xml", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/beep", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/meep.ext", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/subdirwithfiles/SubdirFileA.1", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/subdirwithfiles/SubdirFileB.2", new MockFileData(Guid.NewGuid().ToString()) },
            { "/dirwithfiles/subdirwithfiles/Creep.ext", new MockFileData(Guid.NewGuid().ToString()) },
        };
        
        var mockFileSystem = new MockFileSystem(files);
        mockFileSystem.AddDirectory("/emptydir");
        mockFileSystem.AddDirectory("/dirwithfiles/emptysubdir");

        return mockFileSystem;
    } 
}