// <copyright file="SqliteDataAccessLayerTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.DataAccess.Sqlite;

using System;
using Xunit;
using FluentAssertions;
using Moq.AutoMock;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using RiotClub.FireMoth.Services.Tests.Helpers;
using RiotClub.FireMoth.Services.DataAccess.Sqlite;

public class SqliteDataAccessLayerTests
{
    /*
     * Ctor
     *  - If ILogger is null, an ArgumentNullException is thrown.
     *  - If FireMothContext is null, an ArgumentNullException is thrown.
     *
     * GetAsync
     *  - Call without filter or orderBy parameters returns all FileFingerprints.
     *  - Call with filter returns filtered results.
     *  - Call with orderBy returns ordered results.
     *  - Call with both filter and orderBy parameters returns filtered and ordered results.
     *
     * AddAsync
     *  - If null FileFingerprint is provided, throw ArgumentNullException.
     *  - After call, provided FileFingerprint has been added to the data access layer.
     * 
     * AddManyAsync
     *  - If null IEnumerable is provided, throw ArgumentNullException.
     *  - After call, provided FileFingerprints have been added to the data access layer. 
     *
     * UpdateAsync
     *  - If null FileFingerprint is provided, throw ArgumentNullException.
     *  - After call when data access layer contains a FileFingerprint with a matching full path, matching value is
     *    updated.
     *  - After call when data access layer does not contain a FileFingerprint with a matching full path, no changes are
     *    made to the data access layer.
     *
     * DeleteAsync
     *  - If null FileFingerprint is provided, throw ArgumentNullException.
     *  - After call when data access layer contains an equal FileFingerprint, matching value is deleted.
     *  - After call when data access layer does not contain a FileFingerprint with a matching full path, no changes are
     *    made to the data access layer.
     */
    private readonly AutoMocker _mocker = new();
    private readonly Fixture _fixture = new();
    
    public SqliteDataAccessLayerTests()
    {
        _fixture.Customizations.Add(new Base64HashSpecimenBuilder());
        _fixture.Customizations.Add(new FileNameSpecimenBuilder());
    }
    
#region Ctor
    /// <summary>
    /// Ctor: If ILogger is null, an ArgumentNullException is thrown.
    /// </summary>
    [Fact]
    public void Ctor_ILoggerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockDbContext = _mocker.CreateInstance<FireMothContext>();
        
        // Act
        // ReSharper disable once ObjectCreationAsStatement
#pragma warning disable CA1806
        Action ctorAction = () => new SqliteDataAccessLayer(null!, mockDbContext);
#pragma warning restore CA1806
        
        // Assert
        ctorAction.Should().ThrowExactly<ArgumentNullException>();
    }
    
    /// <summary>
    /// Ctor: If FireMothContext is null, an ArgumentNullException is thrown.
    /// </summary>
    [Fact]
    public void Ctor_FireMothContextIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SqliteDataAccessLayer>>();
        
        // Act
        // ReSharper disable once ObjectCreationAsStatement
#pragma warning disable CA1806
        Action ctorAction = () => new SqliteDataAccessLayer(mockLogger.Object, null!);
#pragma warning restore CA1806
        
        // Assert
        ctorAction.Should().ThrowExactly<ArgumentNullException>();
    }
#endregion

#region GetAsync

    // * GetAsync
    // *  - Call without filter or orderBy parameters returns all FileFingerprints.
   
    // *  - Call with filter returns filtered results.
    // *  - Call with orderBy returns ordered results.
    // *  - Call with both filter and orderBy parameters returns filtered and ordered results.

#endregion

}