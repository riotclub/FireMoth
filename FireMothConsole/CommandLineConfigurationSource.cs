// <copyright file="CommandLineConfigurationSource.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console;

using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;

internal class CommandLineConfigurationSource : IConfigurationSource
{
    private readonly ParseResult _parseResult;

    public CommandLineConfigurationSource(ParseResult parseResult) =>
        _parseResult = parseResult;

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new CommandLineConfigurationProvider(_parseResult);
}