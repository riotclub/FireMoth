// <copyright file="MockExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.Extensions;

using System;
using Microsoft.Extensions.Logging;
using Moq;

public static class MockExtensions
{
    /// <summary>
    /// Verifies that a <see cref="LoggerExtensions.LogInformation(ILogger, string, object[])"/>
    /// invocation with the specified message was performed on the mock.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ILogger"/>.</typeparam>
    /// <param name="logger">The <see cref="Mock{ILogger}"/> on which the verificaation will
    /// be performed.</param>
    /// <param name="expectedMessage">The expected message to verify an invocation of
    /// <see cref="LoggerExtensions.LogInformation(ILogger, string, object[])"/> with.</param>
    /// <param name="logLevel">The expected log level to verify an invocation of
    /// <see cref="LoggerExtensions.LogInformation(ILogger, string, object[])"/> with.</param>
    /// <returns>The <see cref="Mock{ILogger}"/> on which this method was called.</returns>
    public static Mock<ILogger<T>> VerifyLogCalled<T>(
        this Mock<ILogger<T>> logger, string expectedMessage, LogLevel logLevel)
    {
        Func<object, Type?, bool> state = (v, t) => v?.ToString()?.CompareTo(expectedMessage) == 0;

        logger.Verify(x =>
            x.Log(
                It.Is<LogLevel>(l => l == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return logger;
    }
}