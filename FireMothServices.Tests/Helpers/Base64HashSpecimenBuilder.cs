// <copyright file="Base64HashSpecimenBuilder.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Helpers;

using System;
using System.Reflection;
using AutoFixture.Kernel;

public class Base64HashSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        var pi = request as ParameterInfo;
        if (pi == null)
        {
            return new NoSpecimen();
        }
        if (pi.ParameterType != typeof(string) || pi.Name != "base64Hash")
        {
            return new NoSpecimen();
        }

        var rand = new Random();
        var bytes = new byte[32];
        rand.NextBytes(bytes);
        
        return Convert.ToBase64String(bytes);
    }
}