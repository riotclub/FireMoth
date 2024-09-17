// <copyright file="ConfigurationBuilderExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

using System.CommandLine.Parsing;
using RiotClub.FireMoth.Console;

internal static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddCommandLineConfiguration(
        this IConfigurationBuilder builder, ParseResult parseResult)
    {
        return builder.Add(new CommandLineConfigurationSource(parseResult));
    }
}