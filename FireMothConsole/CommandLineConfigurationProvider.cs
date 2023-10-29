// <copyright file="CommandLineConfigurationProvider.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console;

using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using Microsoft.Extensions.Configuration;

internal class CommandLineConfigurationProvider : ConfigurationProvider
{
    private readonly ParseResult _parseResult;

    public CommandLineConfigurationProvider(ParseResult parseResult) =>
        _parseResult = parseResult;

    public override void Load() =>
        Data = MapConfigurationFromParseResult(_parseResult);

    private static IDictionary<string, string?> MapConfigurationFromParseResult(
        ParseResult parseResult)
    {
        var optionResults =
            parseResult.CommandResult.Children
                .Where(symbolResult => symbolResult is OptionResult)
                .ToList();

        var result = optionResults.ToDictionary(
            optionResult => "CommandLine:" + optionResult.Symbol.Name,
            optionResult => 
                parseResult.GetValueForOption(((OptionResult)optionResult).Option)!.ToString(),
            StringComparer.OrdinalIgnoreCase
        );

        return result;
    }
}