// <copyright file="FileNameSpecimenBuilder.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Tests.Common.AutoFixture.SpecimenBuilders;

using System;
using System.Linq;
using System.Reflection;
using global::AutoFixture.Kernel;

public class FileNameSpecimenBuilder : ISpecimenBuilder
{
    private static readonly Random RandomInstance = new();
    
    private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int NameLength = 16;
    
    public object Create(object request, ISpecimenContext context)
    {
        if (request is not ParameterInfo pi)
            return new NoSpecimen();
        
        if (pi.ParameterType != typeof(string) || pi.Name != "fileName")
            return new NoSpecimen();

        return RandomString(NameLength);
    }
    
    private static string RandomString(int length)
    {
        return new string(
            Enumerable.Repeat(AllowedChars, length)
                      .Select(s => s[RandomInstance.Next(s.Length)])
                      .ToArray());
    }
}