// <copyright file="SqliteDataAccessLayerTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.DataAccess.Sqlite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RiotClub.FireMoth.Services.DataAccess.Sqlite;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Tests.Common.AutoFixture.SpecimenBuilders;
using Xunit;

/// <summary>
/// <p>
/// Ctor<br/>
/// - Passing [ILogger{MemoryDataAccessLayer}:null] throws an ArgumentNullException.<br/>
/// - Passing [FireMothContext:null] throws an ArgumentNullException.<br/>
/// </p>
/// <p>
/// GetAsync<br/>
/// - Passing [filter:null, orderBy:null] returns an unfiltered, unordered collection.<br/>
/// - Passing [filter:non-null, orderBy:null] returns a filtered, unordered collection.<br/>
/// - Passing [filter:null, orderBy:non-null] returns an unfiltered, ordered collection.<br/>
/// - Passing [filter:non-null, orderBy:non-null] returns a filtered, ordered collection.<br/>
/// </p>
/// <p>
/// AddAsync<br/>
/// - Passing [FileFingerprint:null] throws an ArgumentNullException.<br/>
/// - Passing [FileFingerprint:non-null] adds a record to the data access layer.<br/>
/// </p>
/// <p>
/// AddManyAsync<br/>
/// - Passing [IEnumerable{FileFingerprint}:null] throws an ArgumentNullException.<br/>
/// - Passing [IEnumerable{FileFingerprint}:non-null] adds the records to the data access layer.
/// <br/>
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
/// </summary>
public class SqliteDataAccessLayerTests
{
    private readonly Fixture _fixture = new();
    private readonly ILogger<SqliteDataAccessLayer> _nullLogger =
        NullLogger<SqliteDataAccessLayer>.Instance;
    
    public SqliteDataAccessLayerTests()
    {
        _fixture.Customizations.Add(new Base64HashSpecimenBuilder());
        _fixture.Customizations.Add(new FileNameSpecimenBuilder());
    }
    
#region Ctor
    /// <summary>Ctor: Passing [ILogger{MemoryDataAccessLayer}:null] throws an
    /// ArgumentNullException.</summary>
    [Fact]
    public void Ctor_ILoggerNull_ThrowsArgumentNullException()
    {
        // Arrange, Act
        var mockFireMothContext = new Mock<FireMothContext>();
    #pragma warning disable CA1806
        // ReSharper disable once ObjectCreationAsStatement
        Action ctorAction = () => new SqliteDataAccessLayer(null!, mockFireMothContext.Object);
    #pragma warning restore CA1806

        // Assert
        ctorAction.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>Ctor: Passing [FireMothContext:null] throws an ArgumentNullException.</summary>
    [Fact]
    public void Ctor_FireMothContextNull_ThrowsArgumentNullException()
    {
        // Arrange, Act
#pragma warning disable CA1806
        // ReSharper disable once ObjectCreationAsStatement
        Action ctorAction = () => new SqliteDataAccessLayer(_nullLogger, null!);
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
        var mockFireMothContext = new Mock<FireMothContext>();
        var mockDbSet = GetMockDbSet();
        mockFireMothContext.Setup(m => m.FileFingerprints).Returns(mockDbSet.Object);
        var expected = mockDbSet.Object.ToList();
        var sut = new SqliteDataAccessLayer(_nullLogger, mockFireMothContext.Object);
        
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
        var mockFireMothContext = new Mock<FireMothContext>();
        var mockDbSet = GetMockDbSet();
        mockFireMothContext.Setup(m => m.FileFingerprints).Returns(mockDbSet.Object);
        bool FilterFunction(IFileFingerprint fingerprint) => fingerprint.FileName.Contains('X');
        var expected = mockDbSet.Object.Where(FilterFunction).ToList();
        var sut = new SqliteDataAccessLayer(_nullLogger, mockFireMothContext.Object);
        
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
        var mockFireMothContext = new Mock<FireMothContext>();
        var mockDbSet = GetMockDbSet();
        mockFireMothContext.Setup(m => m.FileFingerprints).Returns(mockDbSet.Object);        
        string OrderByFunction(IFileFingerprint fingerprint) => fingerprint.FileName;
        var expected = mockDbSet.Object.OrderBy(OrderByFunction).ToList();
        var sut = new SqliteDataAccessLayer(_nullLogger, mockFireMothContext.Object);
        
        // Act
        var result = await sut.GetAsync(orderBy: OrderByFunction);

        // Assert
        result.Should().Equal(expected);
    }
    
    /// <summary>GetAsync: Passing [filter:non-null, orderBy:non-null] returns a filtered, ordered
    /// collection.</summary>
    [Fact]
    public async void GetAsync_FilterNonNullOrderByNonNull_ReturnsFilteredOrderedCollection()
    {
        // Arrange
        var mockFireMothContext = new Mock<FireMothContext>();
        var mockDbSet = GetMockDbSet();
        mockFireMothContext.Setup(m => m.FileFingerprints).Returns(mockDbSet.Object);        
        bool FilterFunction(IFileFingerprint fingerprint) => fingerprint.FileName.Contains('X');        
        string OrderByFunction(IFileFingerprint fingerprint) => fingerprint.FileName;
        var expected = mockDbSet.Object.Where(FilterFunction).OrderBy(OrderByFunction).ToList();
        var sut = new SqliteDataAccessLayer(_nullLogger, mockFireMothContext.Object);        
        
        // Act
        var result = await sut.GetAsync(FilterFunction, OrderByFunction);

        // Assert
        result.Should().Equal(expected);
    }
#endregion

#region AddAsync
    /// <summary>AddAsync: Passing [FileFingerprint:null] throws an ArgumentNullException.</summary>
    [Fact]
    public async void AddAsync_FileFingerprintNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockFireMothContext = new Mock<FireMothContext>();
        var sut = new SqliteDataAccessLayer(_nullLogger, mockFireMothContext.Object);

        // Act
        var addAsyncAction = async () => await sut.AddAsync(null!);

        // Assert
        await addAsyncAction.Should().ThrowExactlyAsync<ArgumentNullException>();
    }
    
    /// <summary>AddAsync: Passing [FileFingerprint:non-null] adds a record to the data access
    /// layer.</summary>
    [Fact]
    public async Task AddAsync_FileFingerprintNonNull_AddsRecord()
    {
        // Arrange
        var mockDbSet = new Mock<DbSet<FileFingerprint>>();
        var mockContext = new Mock<FireMothContext>();
        mockContext.Setup(context => context.FileFingerprints).Returns(mockDbSet.Object);
        var sut = new SqliteDataAccessLayer(_nullLogger, mockContext.Object);
        var testFileFingerprint = _fixture.Create<FileFingerprint>();

        // Act
        await sut.AddAsync(testFileFingerprint);

        // Assert
        mockDbSet.Verify(set => set.Add(It.IsAny<FileFingerprint>()), Times.Once());
        mockContext.Verify(context => context.SaveChanges(), Times.Once());
    }
#endregion
    
#region AddManyAsync
    /// <summary>AddManyAsync: Passing [IEnumerable{FileFingerprint}:null] throws an
    /// ArgumentNullException.</summary>
    [Fact]
    public async void AddManyAsync_IEnumerableNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockContext = new Mock<FireMothContext>();
        var sut = new SqliteDataAccessLayer(_nullLogger, mockContext.Object);

        // Act
        var addManyAsyncAction = async () => await sut.AddManyAsync(null!);

        // Assert
        await addManyAsyncAction.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    /// <summary>AddManyAsync: Passing [IEnumerable{FileFingerprint}:non-null] adds the records to
    /// the data access layer.</summary>
    [Fact]
    public async void AddManyAsync_IEnumerableNonNull_AddsRecords()
    {
        // Arrange
        var mockDbSet = new Mock<DbSet<FileFingerprint>>();
        var mockContext = new Mock<FireMothContext>();
        mockContext.Setup(context => context.FileFingerprints).Returns(mockDbSet.Object);
        var sut = new SqliteDataAccessLayer(_nullLogger, mockContext.Object);
        
        // Act
        await sut.AddManyAsync(_fixture.CreateMany<FileFingerprint>(2));

        // Assert
        mockDbSet.Verify(set =>
            set.AddRange(It.IsAny<IEnumerable<FileFingerprint>>()), Times.Once());
        mockContext.Verify(context => context.SaveChanges(), Times.Once());
    }
#endregion

#region DeleteAsync
    /// <summary>DeleteAsync: Passing [FileFingerprint:null] throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void DeleteAsync_FileFingerprintNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mockContext = new Mock<FireMothContext>();
        var sut = new SqliteDataAccessLayer(_nullLogger, mockContext.Object);

        // Act
        Func<Task> deleteAsyncAction = async () => await sut.DeleteAsync(null!);

        // Assert
        deleteAsyncAction.Should().ThrowExactlyAsync<ArgumentNullException>();
    }

    /// <summary>DeleteAsync: Passing [FileFingerprint:matches a record in the data access layer]
    /// deletes the record.</summary>
    [Fact]
    public async void DeleteAsync_FileFingerprintMatchExists_MatchingValueIsDeleted()
    {
        // Arrange
        var mockFireMothContext = new Mock<FireMothContext>();
        var mockDbSet = GetMockDbSet();
        mockFireMothContext.Setup(m => m.FileFingerprints).Returns(mockDbSet.Object);        
        var itemToDelete = mockDbSet.Object.ElementAt(10);
        var sut = new SqliteDataAccessLayer(_nullLogger, mockFireMothContext.Object);         
        
        // Act
        await sut.DeleteAsync(itemToDelete);
        
        // Assert
        mockFireMothContext.Verify(set => set.Remove(itemToDelete), Times.Once());
        mockFireMothContext.Verify(context => context.SaveChanges(), Times.Once());
    }

    /// <summary>DeleteAsync: Passing [FileFingerprint:matches a record in the data access layer]
    /// returns true.</summary>
    [Fact]
    public async void DeleteAsync_FileFingerprintMatchExists_ReturnsTrue()
    {
        // Arrange
        var mockFireMothContext = new Mock<FireMothContext>();
        var mockDbSet = GetMockDbSet();
        mockFireMothContext.Setup(m => m.FileFingerprints).Returns(mockDbSet.Object);        
        var itemToDelete = mockDbSet.Object.ElementAt(10);
        var sut = new SqliteDataAccessLayer(_nullLogger, mockFireMothContext.Object);         
        
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
        var mockFireMothContext = new Mock<FireMothContext>();
        var mockDbSet = GetMockDbSet();
        mockFireMothContext.Setup(m => m.FileFingerprints).Returns(mockDbSet.Object);        
        var itemToDelete = _fixture.Create<FileFingerprint>();
        var sut = new SqliteDataAccessLayer(_nullLogger, mockFireMothContext.Object);         
        
        // Act
        await sut.DeleteAsync(itemToDelete);
        
        // Assert
        mockFireMothContext.Verify(set => set.Remove(itemToDelete), Times.Never());
        mockFireMothContext.Verify(context => context.SaveChanges(), Times.Never());
    }

    /// <summary>DeleteAsync: Passing [FileFingerprint:does not match a record in the data access
    /// layer] returns false.</summary>
    [Fact]
    public async void DeleteAsync_FileFingerprintMatchDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var mockFireMothContext = new Mock<FireMothContext>();
        var mockDbSet = GetMockDbSet();
        mockFireMothContext.Setup(m => m.FileFingerprints).Returns(mockDbSet.Object);        
        var itemToDelete = _fixture.Create<FileFingerprint>();
        var sut = new SqliteDataAccessLayer(_nullLogger, mockFireMothContext.Object);         
        
        // Act
        var result = await sut.DeleteAsync(itemToDelete);
        
        // Assert
        result.Should().BeFalse();
    }
#endregion

    private Mock<DbSet<FileFingerprint>> GetMockDbSet()
    {
        var testFingerprints = _fixture.CreateMany<FileFingerprint>(50).AsQueryable();

        var mockSet = new Mock<DbSet<FileFingerprint>>();
        mockSet.As<IQueryable<FileFingerprint>>()
               .Setup(m => m.Provider)
               .Returns(testFingerprints.Provider);
        mockSet.As<IQueryable<FileFingerprint>>()
               .Setup(m => m.Expression)
               .Returns(testFingerprints.Expression);
        mockSet.As<IQueryable<FileFingerprint>>()
               .Setup(m => m.ElementType)
               .Returns(testFingerprints.ElementType);
        mockSet.As<IQueryable<FileFingerprint>>()
               .Setup(m => m.GetEnumerator())
               .Returns(() => testFingerprints.GetEnumerator());

        return mockSet;
    }
}