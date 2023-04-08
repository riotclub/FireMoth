// <copyright file="FileNameSpecimenBuilder.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace RiotClub.FireMoth.Services.Tests.Helpers;

using System;
using System.Reflection;
using AutoFixture.Kernel;

public class FileNameSpecimenBuilder : ISpecimenBuilder
{
    private static readonly Random _random = new();
    
    private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int NameLength = 16;
    
    public object Create(object request, ISpecimenContext context)
    {
        var pi = request as ParameterInfo;
        if (pi == null)
        {
            return new NoSpecimen();
        }
        if (pi.ParameterType != typeof(string) || pi.Name != "fileName")
        {
            return new NoSpecimen();
        }

        return RandomString(NameLength);
    }
    
    private static string RandomString(int length)
    {
        return new string(
            Enumerable.Repeat(AllowedChars, length)
                      .Select(s => s[_random.Next(s.Length)])
                      .ToArray());
    }
}