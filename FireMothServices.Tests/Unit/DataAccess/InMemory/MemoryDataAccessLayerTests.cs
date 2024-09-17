// <copyright file="MemoryDataAccessLayerTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
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
/// <p>
/// Ctor<br/>
/// - Passing [ILogger{MemoryDataAccessLayer}:null] throws an ArgumentNullException.<br/>
/// </p>
/// <p>
/// GetAsync<br/>
/// - Passing [filter:null, orderBy:null] returns an unfiltered, unordered collection.<br/>
/// - Passing [filter:non-null, orderBy:null] returns a filtered, unordered collection.<br/>
/// - Passing [filter:null, orderBy:non-null] returns an unfiltered, ordered collection.<br/>
/// - Passing [filter:non-null, orderBy:non-null] returns a filtered and ordered collection.<br/>
/// </p>
/// <p>
/// AddAsync<br/>
/// - Passing [FileFingerprint:null] throws an ArgumentNullException.<br/>
/// - Passing [FileFingerprint:non-null] adds a record to the data access layer.<br/>
/// </p>
/// <p>
/// AddManyAsync<br/>
/// - Passing [IEnumerable{FileFingerprint}:null] throws an ArgumentNullException.<br/>
/// - Passing [IEnumerable{FileFingerprint}:non-null] adds the records to the data access layer.<br/>
/// </p>
/// <p>
/// DeleteAsync<br/>
/// - Passing [FileFingerprint:null] throws an ArgumentNullException.<br/>
/// - Passing [FileFingerprint:matches a record in the data access layer] deletes the record.<br/>
/// - Passing [FileFingerprint:matches a record in the data access layer] returns true.<br/>
/// - Passing [FileFingerprint:does not match a record in the data access layer] does not modify
///   existing records.<br/>
/// - Passing [FileFingerprint:does not match a record in the data access layer] returns false.<br/>
/// </p>
/// <p>
/// DeleteAllAsync<br/>
/// - Deletes all records from the data access layer.<br/>
/// - Returns the number of records that were deleted from the data access layer.<br/>
/// </p>
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
    /// <summary>Ctor:Passing [ILogger{MemoryDataAccessLayer}: null] throws an
    /// ArgumentNullException.</summary>
    [Fact]
    public void Ctor_ILoggerNull_ThrowsArgumentNullException()
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
    /// <summary>GetAsync: Passing [filter:null, orderBy:null] returns an unfiltered, unordered
    /// collection.</summary>
    [Fact]
    public async void GetAsync_FilterNullOrderByNull_ReturnsUnfilteredUnorderedCollection()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();
        var expected = await AddFileFingerprintsAsync(sut);

        // Act
        var result = await sut.GetAsync();

        // Assert
        result.Should().Equal(expected);
    }

    /// <summary>GetAsync: Passing [filter:non-null, orderBy:null] returns a filtered, unordered
    /// collection.</summary>
    [Fact]
    public async void GetAsync_FilterNonNullOrderByNull_ReturnsFilteredUnorderedCollection()
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
    
    /// <summary>GetAsync: Passing [filter:null, orderBy:non-null] returns an unfiltered, ordered
    /// collection.</summary>
    [Fact]
    public async void GetAsync_FilterNullOrderByNonNull_ReturnsUnfilteredOrderedCollection()
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
    
    /// <summary>GetAsync: Passing [filter:non-null, orderBy:non-null] returns a filtered and
    /// ordered collection.</summary>
    [Fact]
    public async void GetAsync_FilterNonNullOrderByNonNull_ReturnsFilteredOrderedCollection()
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
    /// <summary>AddAsync: Passing [FileFingerprint:null] throws an ArgumentNullException.</summary>
    [Fact]
    public void AddAsync_FileFingerprintNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action addAsyncAction = () => sut.AddAsync(null!);

        // Assert
        addAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }
    
    /// <summary>AddAsync: Passing [FileFingerprint:non-null] adds a record to the data access
    /// layer.</summary>
    [Fact]
    public async Task AddAsync_FileFingerprintNonNull_AddsRecord()
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
    /// <summary>AddManyAsync: Passing [IEnumerable{FileFingerprint}:null] throws an
    /// ArgumentNullException.</summary>
    [Fact]
    public void AddManyAsync_IEnumerableNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action addManyAsyncAction = () => sut.AddManyAsync(null!);

        // Assert
        addManyAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>AddManyAsync: Passing [IEnumerable{FileFingerprint}:non-null] adds the records to
    /// the data access layer.</summary>
    [Fact]
    public async void AddManyAsync_IEnumerableNonNull_AddsRecords()
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
    /// <summary>DeleteAsync: Passing [FileFingerprint:null] throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void DeleteAsync_FileFingerprintNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = _mocker.CreateInstance<MemoryDataAccessLayer>();

        // Act
        Action deleteAsyncAction = () => sut.DeleteAsync(null!);

        // Assert
        deleteAsyncAction.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>DeleteAsync: Passing [FileFingerprint:matches a record in the data access layer]
    /// deletes the record.</summary>
    [Fact]
    public async void DeleteAsync_FileFingerprintMatchExists_MatchingValueIsDeleted()
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

    /// <summary>DeleteAsync: Passing [FileFingerprint:matches a record in the data access layer]
    /// returns true.</summary>
    [Fact]
    public async void DeleteAsync_FileFingerprintMatchExists_ReturnsTrue()
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
    
    /// <summary>DeleteAsync: Passing [FileFingerprint:does not match a record in the data access
    /// layer] does not modify existing records.</summary>
    [Fact]
    public async void DeleteAsync_FileFingerprintMatchDoesNotExist_NoChangesMade()
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

    /// <summary>DeleteAsync: Passing [FileFingerprint:does not match a record in the data access
    /// layer] returns false.</summary>
    [Fact]
    public async void DeleteAsync_FileFingerprintMatchDoesNotExist_ReturnsFalse()
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