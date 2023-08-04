// <copyright file="MemoryDataAccessLayerTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.DataAccess.Csv;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.AutoMock;
using Xunit;
using RiotClub.FireMoth.Services.DataAccess;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Services.Tests.Helpers;
using RiotClub.FireMoth.Services.DataAccess.InMemory;

public class MemoryDataAccessLayerTests
{
    /*
     * Ctor
     *  - If ILogger is null, an ArgumentNullException is thrown.
     *
     * GetAsync
     *  - If object is disposed, an ObjectDisposedException is thrown.
     *  - Call without filter or orderBy parameters returns all FileFingerprints.
     *  - Call with filter returns filtered results.
     *  - Call with orderBy returns ordered results.
     *  - Call with both filter and orderBy parameters returns filtered and ordered results.
     *
     * AddAsync
     *  - If object is disposed, an ObjectDisposedException is thrown.
     *  - If null FileFingerprint is provided, throw ArgumentNullException.
     *  - After call, provided FileFingerprint has been added to the data access layer.
     * 
     * AddManyAsync
     *  - If object is disposed, an ObjectDisposedException is thrown.
     *  - If null IEnumerable is provided, throw ArgumentNullException.
     *  - After call, provided FileFingerprints have been added to the data access layer. 
     *
     * UpdateAsync
     *  - If object is disposed, an ObjectDisposedException is thrown.
     *  - If null FileFingerprint is provided, throw ArgumentNullException.
     *  - After call when data access layer contains a FileFingerprint with a matching full path, matching value is
     *    updated.
     *  - After call when data access layer does not contain a FileFingerprint with a matching full path, no changes are
     *    made to the data access layer.
     *
     * DeleteAsync
     *  - If object is disposed, an ObjectDisposedException is thrown.
     *  - If null FileFingerprint is provided, throw ArgumentNullException.
     *  - After call when data access layer contains an equal FileFingerprint, matching value is deleted.
     *  - After call when data access layer does not contain a FileFingerprint with a matching full path, no changes are
     *    made to the data access layer.
     */

    private readonly AutoMocker _mocker = new();
    private readonly Fixture _fixture = new();

    public MemoryDataAccessLayerTests()
    {
        _fixture.Customizations.Add(new Base64HashSpecimenBuilder());
        _fixture.Customizations.Add(new FileNameSpecimenBuilder());
    }
    
    // Ctor: If ILogger is null, an ArgumentNullException is thrown
    [Fact]
    public void Ctor_ILoggerIsNull_ThrowsArgumentNullException()
    {
        // Arrange, Act
        // ReSharper disable once ObjectCreationAsStatement
#pragma warning disable CA1806
        Action ctorAction = () => new MemoryDataAccessLayer(null!);
#pragma warning restore CA1806

        // Assert
        ctorAction.Should().ThrowExactly<ArgumentNullException>();
    }
    
    // GetAsync: If object is disposed, an ObjectDisposedException is thrown
    [Fact]
    public void GetAsync_ObjectDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        testObject.Dispose();

        // Act
        Action getAsyncAction = () => testObject.GetAsync();

        // Assert
        getAsyncAction.Should().ThrowExactly<ObjectDisposedException>();
    }
    
    // GetAsync: Call without filter or orderBy parameters returns all FileFingerprints
    [Fact]
    public async void GetAsync_NoFilterOrOrderByParameters_ReturnsAllFileFingerprints()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var expectedResult = await AddFileFingerprints(testObject);

        // Act
        var result = await testObject.GetAsync();

        // Assert
        result.Should().Equal(expectedResult);
    }

    // GetAsync: Call with filter returns filtered results.
    [Fact]
    public async void GetAsync_WithFilter_ReturnsFilteredResults()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var fingerprints = await AddFileFingerprints(testObject);
        bool FilterFunction(IFileFingerprint fingerprint) => fingerprint.FileName.Contains('X');
        var expectedResult = fingerprints.Where(FilterFunction);
        
        // Act
        var result = await testObject.GetAsync(filter: FilterFunction);

        // Assert
        result.Should().Equal(expectedResult);
    }
    
    // GetAsync: Call with orderBy returns ordered results.
    [Fact]
    public async void GetAsync_WithOrderBy_ReturnsOrderedResults()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var fingerprints = await AddFileFingerprints(testObject);
        string OrderByFunction(IFileFingerprint fingerprint) => fingerprint.FileName;
        var expectedResult = fingerprints.OrderBy(OrderByFunction);
        
        // Act
        var result = await testObject.GetAsync(orderBy: OrderByFunction);

        // Assert
        result.Should().Equal(expectedResult);
    }
    
    // GetAsync: Call with both filter and orderBy parameters returns filtered and ordered results.
    [Fact]
    public async void GetAsync_WithFilterAndOrderBy_ReturnsFilteredAndOrderedResults()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var fingerprints = await AddFileFingerprints(testObject);
        bool FilterFunction(IFileFingerprint fingerprint) => fingerprint.FileName.Contains('X');        
        string OrderByFunction(IFileFingerprint fingerprint) => fingerprint.FileName;
        var filteredResult = fingerprints.Where(FilterFunction);
        var expectedResult = filteredResult.OrderBy(OrderByFunction);
        
        // Act
        var result = await testObject.GetAsync(FilterFunction, OrderByFunction);

        // Assert
        result.Should().Equal(expectedResult);
    }
    
    // AddAsync: If object is disposed, an ObjectDisposedException is thrown.
    [Fact]
    public void AddAsync_ObjectDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var testFileFingerprint = _fixture.Create<FileFingerprint>();
        testObject.Dispose();

        // Act
        Action getAsyncAction = () => testObject.AddAsync(testFileFingerprint);

        // Assert
        getAsyncAction.Should().ThrowExactly<ObjectDisposedException>();
    }
    
    // AddAsync: If null FileFingerprint is provided, throw ArgumentNullException.
    [Fact]
    public void AddAsync_WithNullFileFingerprint_ThrowsArgumentNullException()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action getAsyncAction = () => testObject.AddAsync(null!);

        // Assert
        getAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }
    
    // AddAsync: After call, provided FileFingerprint has been added to the data access layer.
    [Fact]
    public async Task AddAsync_WithValidFileFingerprint_AddsFileFingerprint()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        await AddFileFingerprints(testObject);
        var testFileFingerprint = _fixture.Create<FileFingerprint>();
        var expectedResult = new List<IFileFingerprint> { testFileFingerprint };

        // Act
        await testObject.AddAsync(testFileFingerprint);
        var result = await testObject.GetAsync(fingerprint => fingerprint.Equals(testFileFingerprint));

        // Assert
        result.Should().NotBeNull().And.HaveCount(1).And.Equal(expectedResult);
    }
    
    private async Task<IEnumerable<IFileFingerprint>> AddFileFingerprints(
        IDataAccessLayer<IFileFingerprint> dataAccessLayer)
    {
        var fileFingerprints = _fixture.CreateMany<FileFingerprint>(50);
        var fileFingerprintList = fileFingerprints.ToList();
        await dataAccessLayer.AddManyAsync(fileFingerprintList);
        
        return fileFingerprintList;
    }
}