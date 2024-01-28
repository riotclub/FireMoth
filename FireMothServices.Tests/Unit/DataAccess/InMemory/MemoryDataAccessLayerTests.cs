// <copyright file="MemoryDataAccessLayerTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.DataAccess.InMemory;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.AutoMock;
using RiotClub.FireMoth.Services.DataAccess;
using RiotClub.FireMoth.Services.DataAccess.InMemory;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Tests.Common.AutoFixture.SpecimenBuilders;
using Xunit;

/// <summary>
/// Ctor
///     - If ILogger is null, an ArgumentNullException is thrown.
///
/// GetAsync
///     - Call without filter or orderBy parameters returns all FileFingerprints.
///     - Call with filter returns filtered results.
///     - Call with orderBy returns ordered results.
///     - Call with both filter and orderBy parameters returns filtered and ordered results.
///
/// AddAsync
///     - If null FileFingerprint is provided, throw ArgumentNullException.
///     - After call, provided FileFingerprint has been added to the data access layer.
///
/// AddManyAsync
///     - If null IEnumerable is provided, throw ArgumentNullException.
///     - After call, provided FileFingerprints have been added to the data access layer.
///
/// DeleteAsync
///     - If null FileFingerprint is provided, throw ArgumentNullException.
///     - When data access layer contains a matching FileFingerprint, matching value is deleted.
///     - When data access layer contains a matching FileFingerprint, true is returned.
///     - When data access layer does not contain a matching FileFingerprint, no changes are made to
///       the data access layer.
///     - When data access layer does not contain a matching FileFingerprint, false is returned.
/// </summary>
public class MemoryDataAccessLayerTests
{
    private readonly AutoMocker _mocker = new();
    private readonly Fixture _fixture = new();
    
    public MemoryDataAccessLayerTests()
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
        // Arrange, Act
        // ReSharper disable once ObjectCreationAsStatement
#pragma warning disable CA1806
        Action ctorAction = () => new MemoryDataAccessLayer(null!);
#pragma warning restore CA1806

        // Assert
        ctorAction.Should().ThrowExactly<ArgumentNullException>();
    }
#endregion

#region GetAsync
    /// <summary>
    /// GetAsync: Call without filter or orderBy parameters returns all FileFingerprints
    /// </summary>
    [Fact]
    public async void GetAsync_WithoutFilterOrOrderBy_ReturnsAllFileFingerprints()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var expectedResult = await AddFileFingerprintsAsync(testObject);

        // Act
        var result = await testObject.GetAsync();

        // Assert
        result.Should().Equal(expectedResult);
    }

    /// <summary>
    /// GetAsync: Call with filter returns filtered results
    /// </summary>
    [Fact]
    public async void GetAsync_WithFilter_ReturnsFilteredResults()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var fingerprints = await AddFileFingerprintsAsync(testObject);
        bool FilterFunction(IFileFingerprint fingerprint) => fingerprint.FileName.Contains('X');
        var expectedResult = fingerprints.Where(FilterFunction);
        
        // Act
        var result = await testObject.GetAsync(filter: FilterFunction);

        // Assert
        result.Should().Equal(expectedResult);
    }
    
    /// <summary>
    /// GetAsync: Call with orderBy returns ordered results.
    /// </summary>
    [Fact]
    public async void GetAsync_WithOrderBy_ReturnsOrderedResults()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var fingerprints = await AddFileFingerprintsAsync(testObject);
        string OrderByFunction(IFileFingerprint fingerprint) => fingerprint.FileName;
        var expectedResult = fingerprints.OrderBy(OrderByFunction);
        
        // Act
        var result = await testObject.GetAsync(orderBy: OrderByFunction);

        // Assert
        result.Should().Equal(expectedResult);
    }
    
    /// <summary>
    /// GetAsync: Call with both filter and orderBy parameters returns filtered and ordered results.
    /// </summary>
    [Fact]
    public async void GetAsync_WithFilterAndOrderBy_ReturnsFilteredAndOrderedResults()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var fingerprints = await AddFileFingerprintsAsync(testObject);
        bool FilterFunction(IFileFingerprint fingerprint) => fingerprint.FileName.Contains('X');        
        var filteredResult = fingerprints.Where(FilterFunction);
        string OrderByFunction(IFileFingerprint fingerprint) => fingerprint.FileName;
        var expectedResult = filteredResult.OrderBy(OrderByFunction);
        
        // Act
        var result = await testObject.GetAsync(FilterFunction, OrderByFunction);

        // Assert
        result.Should().Equal(expectedResult);
    }
#endregion

#region AddAsync
    /// <summary>
    /// AddAsync: If null FileFingerprint is provided, throw ArgumentNullException.
    /// </summary>
    [Fact]
    public void AddAsync_WithNullFileFingerprint_ThrowsArgumentNullException()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action addAsyncAction = () => testObject.AddAsync(null!);

        // Assert
        addAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }
    
    /// <summary>
    /// AddAsync: After call, provided FileFingerprint has been added to the data access layer.
    /// </summary>
    [Fact]
    public async Task AddAsync_WithValidFileFingerprint_AddsFileFingerprint()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();
        await AddFileFingerprintsAsync(testObject);
        var testFileFingerprint = _fixture.Create<FileFingerprint>();
        var expectedResult = new List<FileFingerprint> { testFileFingerprint };

        // Act
        await testObject.AddAsync(testFileFingerprint);
        var result = await testObject.GetAsync(fingerprint =>
            fingerprint.Equals(testFileFingerprint));

        // Assert
        result.Should().NotBeNull().And.HaveCount(1).And.Equal(expectedResult);
    }
#endregion
    
#region AddManyAsync
    /// <summary>
    /// AddManyAsync: If null IEnumerable is provided, throw ArgumentNullException.
    /// </summary>
    [Fact]
    public void AddManyAsync_WithNullIEnumerable_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action addManyAsyncAction = () => sut.AddManyAsync(null!);

        // Assert
        addManyAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>
    /// After call, provided FileFingerprints have been added to the data access layer.
    /// </summary>
    [Fact]
    public async void AddManyAsync_WithValidFileFingerprints_AddsFileFingerprints()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        await AddFileFingerprintsAsync(sut);
        var existingFileFingerprints = (await sut.GetAsync()).ToList();
        var testFileFingerprints = _fixture.CreateMany<FileFingerprint>(3).ToList();
        var expectedResult = existingFileFingerprints.Concat(testFileFingerprints).ToList();

        // Act
        await sut.AddManyAsync(testFileFingerprints);

        // Assert
        var result = await sut.GetAsync();
        result.Should().Equal(expectedResult);
    }
#endregion

#region DeleteAsync
    /// <summary>
    /// DeleteAsync: If null FileFingerprint is provided, throw ArgumentNullException.
    /// </summary>
    [Fact]
    public void DeleteAsync_WithNullFileFingerprint_ThrowsArgumentNullException()
    {
        // Arrange
        var testObject = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action deleteAsyncAction = () => testObject.DeleteAsync(null!);

        // Assert
        deleteAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>
    /// DeleteAsync: When data access layer contains a matching FileFingerprint, matching value is
    /// deleted.
    /// </summary>
    [Fact]
    public async void DeleteAsync_MatchingFileFingerprintExists_MatchingValueIsDeleted()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var expected = (await AddFileFingerprintsAsync(sut)).ToList();
        var itemToDelete = expected[10];
        expected.Remove(itemToDelete);

        // Act
        await sut.DeleteAsync(itemToDelete);

        // Assert
        var result = await sut.GetAsync();
        result.Should().Equal(expected);
    }

    /// <summary>
    /// DeleteAsync: When data access layer contains a matching FileFingerprint, true is returned.
    /// </summary>
    [Fact]
    public async void DeleteAsync_MatchingFileFingerprintExists_ReturnsTrue()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var originalItems = (await AddFileFingerprintsAsync(sut)).ToList();
        var itemToDelete = originalItems[0];

        // Act
        var result = await sut.DeleteAsync(itemToDelete);

        // Assert
        result.Should().BeTrue();
    }
    
    /// <summary>
    /// DeleteAsync: When data access layer does not contain a matching FileFingerprint, no changes
    /// are made to the data access layer.
    /// </summary>
    [Fact]
    public async void DeleteAsync_MatchingFileFingerprintDoesNotExist_NoChangesMade()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var expected = (await AddFileFingerprintsAsync(sut)).ToList();
        var randomFingerprint = _fixture.Create<FileFingerprint>();

        // Act
        await sut.DeleteAsync(randomFingerprint);
        var result = await sut.GetAsync();
        
        // Assert
        result.Should().Equal(expected);
    }

    /// <summary>
    /// DeleteAsync: When data access layer does not contain a matching FileFingerprint, false is
    /// returned.
    /// </summary>
    [Fact]
    public async void DeleteAsync_MatchingFileFingerprintDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var randomFingerprint = _fixture.Create<FileFingerprint>();

        // Act
        var result = await sut.DeleteAsync(randomFingerprint);
        
        // Assert
        result.Should().BeFalse();
    }
#endregion

    private async Task<IEnumerable<FileFingerprint>> AddFileFingerprintsAsync(
        IDataAccessLayer<FileFingerprint> dataAccessLayer)
    {
        var fileFingerprints = _fixture.CreateMany<FileFingerprint>(50);
        var fileFingerprintList = fileFingerprints.ToList();
        await dataAccessLayer.AddManyAsync(fileFingerprintList);
        
        return fileFingerprintList;
    }
}