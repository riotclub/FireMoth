﻿// <copyright file="MemoryDataAccessLayerTests.cs" company="Riot Club">
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
using RiotClub.FireMoth.Services.DataAccess.InMemory;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Tests.Common.AutoFixture.SpecimenBuilders;
using Xunit;

/// <summary>
/// Ctor
/// - If null ILogger is passed, an ArgumentNullException is thrown.
///
/// GetAsync
/// - If null filter and null orderBy expressions are passed, an unfiltered and unordered collection
///   of records is returned.
/// - If non-null filter and null orderBy expressions are passed, a filtered and unordered
///   collection of records is returned.
/// - If null filter and non-null orderBy expressions are passed, an unfiltered and ordered
///   collection of records is returned.
/// - If non-null filter and non-null orderBy expressions are passed, a filtered and ordered
///   collection of records is returned.
///
/// AddAsync
/// - If null FileFingerprint is passed, an ArgumentNullException is thrown.
/// - If non-null FileFingerprint is passed, a record is added to the data access layer.
///
/// AddManyAsync
/// - If null IEnumerable{FileFingerprint} is passed, an ArgumentNullException is thrown.
/// - If non-null IEnumerable{FileFingerprint} is passed, the records are added to the data access
///   layer.
///
/// DeleteAsync
/// - If null FileFingerprint is passed, an ArgumentNullException is thrown.
/// - If non-null FileFingerprint that matches a record in the data access layer is passed, the
///   record is deleted.
/// - If non-null FileFingerprint that matches a record in the data access layer is passed, true is
///   returned.
/// - If non-null FileFingerprint that does not match a record in the data access layer is passed,
///   the data access layer's existing records are not modified.
/// - If non-null FileFingerprint that does not match a record in the data access layer is passed,
///   false is returned.
///
/// DeleteAllAsync
/// - Deletes all records from the data access layer.
/// - Returns the number of records that were deleted from the data access layer.
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
    /// <summary>Ctor: If null ILogger is passed, an ArgumentNullException is thrown.</summary>
    [Fact]
    public void Ctor_NullILogger_ThrowsArgumentNullException()
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
    /// <summary>GetAsync: If null filter and null orderBy expressions are passed, an unfiltered and
    /// unordered collection of records is returned.</summary>
    [Fact]
    public async void GetAsync_NullFilterNullOrderBy_ReturnsUnfilteredUnorderedCollection()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var expected = await AddFileFingerprintsAsync(sut);

        // Act
        var result = await sut.GetAsync();

        // Assert
        result.Should().Equal(expected);
    }

    /// <summary>GetAsync: If non-null filter and null orderBy expressions are passed, a filtered
    /// and unordered collection of records is returned.</summary>
    [Fact]
    public async void GetAsync_NonNullFilterNullOrderBy_ReturnsFilteredUnorderedCollection()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var fingerprints = await AddFileFingerprintsAsync(sut);
        bool FilterFunction(IFileFingerprint fingerprint) => fingerprint.FileName.Contains('X');
        var expected = fingerprints.Where(FilterFunction);
        
        // Act
        var result = await sut.GetAsync(filter: FilterFunction);

        // Assert
        result.Should().Equal(expected);
    }
    
    /// <summary>GetAsync: If null filter and non-null orderBy expressions are passed, an unfiltered
    /// and ordered collection of records is returned.</summary>
    [Fact]
    public async void GetAsync_NullFilterNonNullOrderBy_ReturnsUnfilteredOrderedCollection()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var fingerprints = await AddFileFingerprintsAsync(sut);
        string OrderByFunction(IFileFingerprint fingerprint) => fingerprint.FileName;
        var expected = fingerprints.OrderBy(OrderByFunction);
        
        // Act
        var result = await sut.GetAsync(orderBy: OrderByFunction);

        // Assert
        result.Should().Equal(expected);
    }
    
    /// <summary>GetAsync: If non-null filter and non-null orderBy expressions are passed, a
    /// filtered and ordered collection of records is returned.</summary>
    [Fact]
    public async void GetAsync_NonNullFilterNonNullOrderBy_ReturnsFilteredOrderedCollection()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var fingerprints = await AddFileFingerprintsAsync(sut);
        bool FilterFunction(IFileFingerprint fingerprint) => fingerprint.FileName.Contains('X');        
        var filtered = fingerprints.Where(FilterFunction);
        string OrderByFunction(IFileFingerprint fingerprint) => fingerprint.FileName;
        var expected = filtered.OrderBy(OrderByFunction);
        
        // Act
        var result = await sut.GetAsync(FilterFunction, OrderByFunction);

        // Assert
        result.Should().Equal(expected);
    }
#endregion

#region AddAsync
    /// <summary>AddAsync: If null FileFingerprint is passed, an ArgumentNullException is thrown.
    /// </summary>
    [Fact]
    public void AddAsync_NullFileFingerprint_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action addAsyncAction = () => sut.AddAsync(null!);

        // Assert
        addAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }
    
    /// <summary>AddAsync: If non-null FileFingerprint is passed, a record is added to the data
    /// access layer.</summary>
    [Fact]
    public async Task AddAsync_NonNullFileFingerprint_AddsRecord()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        await AddFileFingerprintsAsync(sut);
        var testFileFingerprint = _fixture.Create<FileFingerprint>();
        var expected = new List<FileFingerprint> { testFileFingerprint };

        // Act
        await sut.AddAsync(testFileFingerprint);
        var result = await sut.GetAsync(fingerprint => fingerprint.Equals(testFileFingerprint));

        // Assert
        result.Should().Equal(expected);
    }
#endregion
    
#region AddManyAsync
    /// <summary>AddManyAsync: If null IEnumerable{FileFingerprint} is passed, an
    /// ArgumentNullException is thrown.</summary>
    [Fact]
    public void AddManyAsync_NullIEnumerable_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action addManyAsyncAction = () => sut.AddManyAsync(null!);

        // Assert
        addManyAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>If non-null IEnumerable{FileFingerprint} is passed, the records are added to the
    /// data access layer.</summary>
    [Fact]
    public async void AddManyAsync_NonNullIEnumerable_AddsRecords()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        await AddFileFingerprintsAsync(sut);
        var existingFileFingerprints = (await sut.GetAsync()).ToList();
        var testFileFingerprints = _fixture.CreateMany<FileFingerprint>(3).ToList();
        var expected = existingFileFingerprints.Concat(testFileFingerprints).ToList();

        // Act
        await sut.AddManyAsync(testFileFingerprints);

        // Assert
        var result = await sut.GetAsync();
        result.Should().Equal(expected);
    }
#endregion

#region DeleteAsync
    /// <summary>DeleteAsync: If null FileFingerprint is passed, an ArgumentNullException is thrown.
    /// </summary>
    [Fact]
    public void DeleteAsync_NullFileFingerprint_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action deleteAsyncAction = () => sut.DeleteAsync(null!);

        // Assert
        deleteAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>DeleteAsync: If non-null FileFingerprint that matches a record in the data access
    /// layer is passed, the record is deleted.</summary>
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

    /// <summary>DeleteAsync: If non-null FileFingerprint that matches a record in the data access
    /// layer is passed, true is returned.</summary>
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
    
    /// <summary>DeleteAsync: If non-null FileFingerprint that does not match a record in the data
    /// access layer is passed, the data access layer's existing records are not modified.</summary>
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

    /// <summary>DeleteAsync: If non-null FileFingerprint that does not match a record in the data
    /// access layer is passed, false is returned.</summary>
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

#region DeleteAllAsync
    /// <summary>DeleteAllAsync: Deletes all records from the data access layer.</summary>
    [Fact]
    public async void DeleteAllAsync_MethodCalled_DeletesAllRecords()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        await AddFileFingerprintsAsync(sut);
            
        // Act
        await sut.DeleteAllAsync();
        var result = await sut.GetAsync();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>DeleteAllAsync: Returns the number of records that were deleted from the data
    /// access layer.</summary>
    [Fact]
    public async void DeleteAllAsync_MethodCalled_ReturnsDeletedRecordCount()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var addedFileFingerprints = (await AddFileFingerprintsAsync(sut)).ToList();
        var expected = addedFileFingerprints.Count;
            
        // Act
        var result = await sut.DeleteAllAsync();

        // Assert
        result.Should().Be(expected);
    }
#endregion

    private async Task<IEnumerable<FileFingerprint>> AddFileFingerprintsAsync(
        MemoryDataAccessLayer dataAccessLayer)
    {
        var fileFingerprints = _fixture.CreateMany<FileFingerprint>(50);
        var fileFingerprintList = fileFingerprints.ToList();
        await dataAccessLayer.AddManyAsync(fileFingerprintList);
        
        return fileFingerprintList;
    }
}