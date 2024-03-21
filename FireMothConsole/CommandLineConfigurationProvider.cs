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
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Handles configuration of command line parameters, converting kebab-cased parameters to
/// PascalCase so they can be bound to options classes via
/// <see cref="OptionsConfigurationServiceCollectionExtensions.Configure{TOptions}(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration)"/> 
/// </summary>
internal class CommandLineConfigurationProvider : ConfigurationProvider
{
    private readonly ParseResult _parseResult;

    private const string CommandLineOptionPrefix = "CommandLine:";
    private const string MoveDuplicateFilesToDirectoryKey =
        CommandLineOptionPrefix + "MoveDuplicateFilesToDirectory";
    private const string ScanDirectoryKey = CommandLineOptionPrefix + "Directory";
    public const string ScanDirectoryToken = "<scan_directory>";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineConfigurationProvider"/> class.
    /// </summary>
    /// <param name="parseResult">A <see cref="ParseResult"/> containing command line parse results.
    /// </param>
    public CommandLineConfigurationProvider(ParseResult parseResult) =>
        _parseResult = parseResult;

    /// <inheritdoc/>
    public override void Load() =>
        Data = MapConfigurationFromParseResult(_parseResult);

    private static Dictionary<string, string?> MapConfigurationFromParseResult(
        ParseResult parseResult)
    {
        var optionResults = parseResult.CommandResult.Children
            .Where(symbolResult => symbolResult is OptionResult)
            .ToList();

        var result = optionResults.ToDictionary(
            optionResult =>
                CommandLineOptionPrefix + KebabCaseToPascalCase(optionResult.Symbol.Name),
            optionResult =>
                parseResult.GetValueForOption(((OptionResult)optionResult).Option)!.ToString(),
            StringComparer.OrdinalIgnoreCase
        );
        
        ReplaceScanDirectoryToken(result);

        return result;
    }

    private static void ReplaceScanDirectoryToken(
        Dictionary<string, string?> optionResultsDictionary)
    {
        if (!optionResultsDictionary.TryGetValue(MoveDuplicateFilesToDirectoryKey, out var value))
            return;

        if (value is default(string?)
            || !optionResultsDictionary.TryGetValue(ScanDirectoryKey, out var scanDirectory))
            return;
        
        optionResultsDictionary.Remove(MoveDuplicateFilesToDirectoryKey);
        optionResultsDictionary.Add(MoveDuplicateFilesToDirectoryKey,
            value.Replace(ScanDirectoryToken, scanDirectory));
    }
    
    private static string KebabCaseToPascalCase(string original)
    {
        var tokens = original.Split('-');
        var upperTokens = new string[tokens.Length];
        for (var index = 0; index < tokens.Length; index++)
        {
            upperTokens[index] =
                string.Concat(tokens[index][0].ToString().ToUpper(), tokens[index].AsSpan(1));
        }
       
        return string.Concat(upperTokens);
    }
}