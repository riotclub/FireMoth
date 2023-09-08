// <copyright file="SqliteDataAccessLayerTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace FireMoth.Services.Tests.Integration.DataAccess.Sqlite;

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq.AutoMock;
using RiotClub.FireMoth.Services.DataAccess.Sqlite;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Tests.Common.AutoFixture.SpecimenBuilders;

/// <summary>
/// SqliteDataAccessLayer Integration Tests
///
/// Ctor
///     - If ILogger is null, an ArgumentNullException is thrown.
///     - If FireMothContext is null, an ArgumentNullException is thrown.
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
public class SqliteDataAccessLayerTests : IClassFixture<SqliteFixture>, IDisposable
{
    private readonly AutoMocker _mocker = new();
    private readonly Fixture _autoFixture = new();
    private readonly SqliteFixture _sqliteFixture;
    private readonly NullLogger<SqliteDataAccessLayer> _nullLogger =
        NullLogger<SqliteDataAccessLayer>.Instance; 
    
    public SqliteDataAccessLayerTests(SqliteFixture sqliteFixture)
    {
        _sqliteFixture = sqliteFixture;
        _sqliteFixture.InsertTestData();

        _autoFixture.Customizations.Add(new Base64HashSpecimenBuilder());
        _autoFixture.Customizations.Add(new FileNameSpecimenBuilder());
    }

    /// <inheritdoc cref="IDisposable"/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _sqliteFixture.DeleteTestData();
    }

#region Ctor
    /// <summary>
    /// Ctor: If ILogger is null, an ArgumentNullException is thrown.
    /// </summary>
    [Fact]
    public void Ctor_ILoggerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var ctorFunc = () => new SqliteDataAccessLayer(null!, _sqliteFixture.DbContext);

        // Act, Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Ctor: If FireMothContext is null, an ArgumentNullException is thrown.
    /// </summary>
    [Fact]
    public void Ctor_FireMothContextIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var ctorFunc = () => new SqliteDataAccessLayer(_nullLogger, null!);

        // Act, Assert
        ctorFunc.Should().ThrowExactly<ArgumentNullException>();
    }
#endregion
    
#region GetAsync
    /// <summary>
    /// GetAsync: Call without filter or orderBy parameters returns all FileFingerprints.
    /// </summary>
    [Fact]
    public async void GetAsync_WithoutFilterOrOrderBy_ReturnsAllFileFingerprints()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        var expected = _sqliteFixture.TestFileFingerprints;

        // Act
        var result = (await sut.GetAsync()).ToList();

        // Assert
        result.Should().Equal(expected);
    }

    /// <summary>
    /// GetAsync: Call with filter returns filtered results.
    /// </summary>
    [Fact]
    public async void GetAsync_WithFilter_ReturnsFilteredResults()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        // ReSharper disable once ConvertToLocalFunction
        Func<FileFingerprint, bool> filter = fingerprint =>
            fingerprint.FileName.StartsWith("TestFile"); 
        var expected = _sqliteFixture.TestFileFingerprints.Where(filter);

        // Act
        var result = (await sut.GetAsync(filter: filter)).ToList();

        // Assert
        result.Should().Equal(expected);
    }

    /// <summary>
    /// GetAsync: Call with orderBy returns ordered results.
    /// </summary>
    [Fact]
    public async void GetAsync_WithOrderBy_ReturnsOrderedResults()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        // ReSharper disable once ConvertToLocalFunction
        Func<FileFingerprint, string> orderBy = fingerprint => fingerprint.FileName; 
        var expected = _sqliteFixture.TestFileFingerprints.OrderBy(orderBy);

        // Act
        var result = (await sut.GetAsync(orderBy: orderBy)).ToList();

        // Assert
        result.Should().Equal(expected);
    }

    /// <summary>
    /// GetAsync: Call with both filter and orderBy parameters returns filtered and ordered results. 
    /// </summary>
    [Fact]
    public async void GetAsync_WithFilterAndOrderBy_ReturnsFilteredAndOrderedResults()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        Func<FileFingerprint, bool> filter = fingerprint =>
            fingerprint.FileName.StartsWith("TestFile"); 
        Func<FileFingerprint, string> orderBy = fingerprint => fingerprint.FileName;
        var expected = _sqliteFixture.TestFileFingerprints.Where(filter).OrderBy(orderBy);

        // Act
        var result = (await sut.GetAsync(filter: filter, orderBy: orderBy)).ToList();

        // Assert
        result.Should().Equal(expected);
    }
#endregion GetAsync

#region AddAsync
    /// <summary>
    /// AddAsync: If null FileFingerprint is provided, throw ArgumentNullException.
    /// </summary>
    [Fact]
    public async void AddAsync_WithNullFileFingerprint_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);

        // Act
        var addAsyncFunc = () => sut.AddAsync(null!);

        // Assert
        await addAsyncFunc.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// AddAsync: After call, provided FileFingerprint has been added to the data access layer.
    /// </summary>
    [Fact]
    public async void AddAsync_WithValidFileFingerprint_AddsFileFingerprintToDataAccessLayer()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        var testFileFingerprint = _autoFixture.Create<FileFingerprint>();
        var expected = _sqliteFixture.TestFileFingerprints.ToList();
        expected.Add(testFileFingerprint);

        // Act
        await sut.AddAsync(testFileFingerprint);
        var result = await sut.GetAsync();

        // Assert
        result.Should().Equal(expected);
    }
#endregion

#region AddManyAsync
    /// <summary>
    /// AddManyAsync: If null IEnumerable is provided, throw ArgumentNullException.
    /// </summary>
    [Fact]
    public async void AddManyAsync_WithNullIEnumerable_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);

        // Act
        var addManyAsyncFunc = () => sut.AddManyAsync(null!);

        // Assert
        await addManyAsyncFunc.Should().ThrowAsync<ArgumentNullException>();
    }
    
    /// <summary>
    /// AddManyAsync: After call, provided FileFingerprints have been added to the data access
    ///               layer.
    /// </summary>
    [Fact]
    public async void AddManyAsync_WithValidIEnumerable_AddsFileFingerprintsToDataAccessLayer()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        var testFileFingerprints = _autoFixture.CreateMany<FileFingerprint>(5).ToList();
        var expected = _sqliteFixture.TestFileFingerprints.ToList();
        expected.AddRange(testFileFingerprints);

        // Act
        await sut.AddManyAsync(testFileFingerprints);
        var result = await sut.GetAsync();

        // Assert
        result.Should().Equal(expected);
    }
#endregion

#region DeleteAsync
    /// <summary>
    /// DeleteAsync: If null FileFingerprint is provided, throw ArgumentNullException.
    /// </summary>
    [Fact]
    public async void DeleteAsync_WithNullFileFingerprint_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);

        // Act
        var deleteAsyncFunc = () => sut.DeleteAsync(null!);

        // Assert
        await deleteAsyncFunc.Should().ThrowAsync<ArgumentNullException>();        
    }

    /// <summary>
    /// DeleteAsync: When data access layer contains a matching FileFingerprint, matching value is
    ///              deleted.
    /// </summary>
    [Fact]
    public async void DeleteAsync_WithMatchingFileFingerprint_MatchingValueIsDeleted()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        var fingerprintToDelete = _sqliteFixture.DbContext.FileFingerprints
            .First(fp => fp.FileName == "TestFileA.dat");
        var expected = _sqliteFixture.TestFileFingerprints.ToList();
        expected.Remove(fingerprintToDelete);
    
        // Act
        await sut.DeleteAsync(fingerprintToDelete);
        var result = await sut.GetAsync();
    
        // Assert
        result.Should().Equal(expected);
    }
    
    /// <summary>
    /// DeleteAsync: When data access layer contains a matching FileFingerprint, true is returned.
    /// </summary>
    [Fact]
    public async void DeleteAsync_WithMatchingFileFingerprint_ReturnsTrue()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        var fingerprintToDelete = _sqliteFixture.DbContext.FileFingerprints
            .First(fp => fp.FileName == "TestFileA.dat");
    
        // Act
        var result = await sut.DeleteAsync(fingerprintToDelete);
    
        // Assert
        result.Should().BeTrue();
    }
    
    /// <summary>
    /// DeleteAsync: When data access layer does not contain a matching FileFingerprint, no changes
    ///              are made to the data access layer. 
    /// </summary>
    [Fact]
    public async void DeleteAsync_WithoutMatchingFileFingerprint_NoChangesMade()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        var fingerprintToDelete = new FileFingerprint(
            "File", "NotFound", 0, "kCHjHcukmK55NvgizMPfN4Icwk9xEpcpqrm6SiC1nkw=");
        var expected = _sqliteFixture.TestFileFingerprints.ToList();

        // Act
        await sut.DeleteAsync(fingerprintToDelete);
        var result = await sut.GetAsync();

        // Assert
        result.Should().Equal(expected);
    }
    
    // DeleteAsync: When data access layer does not contain a matching FileFingerprint, false is
    //              returned.
    [Fact]
    public async void DeleteAsync_WithoutMatchingFileFingerprint_ReturnsFalse()
    {
        // Arrange
        var sut = new SqliteDataAccessLayer(_nullLogger, _sqliteFixture.DbContext);
        var fingerprintToDelete = new FileFingerprint(
            "File", "NotFound", 0, "kCHjHcukmK55NvgizMPfN4Icwk9xEpcpqrm6SiC1nkw=");
        var expected = _sqliteFixture.TestFileFingerprints.ToList();

        // Act
        await sut.DeleteAsync(fingerprintToDelete);
        var result = await sut.GetAsync();

        // Assert
        result.Should().Equal(expected);
    }
#endregion
}