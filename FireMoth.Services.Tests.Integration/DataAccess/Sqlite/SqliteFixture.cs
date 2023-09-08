// <copyright file="SqliteFixture.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace FireMoth.Services.Tests.Integration.DataAccess.Sqlite;

using AutoFixture;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RiotClub.FireMoth.Services.DataAccess.Sqlite;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Tests.Common.AutoFixture.SpecimenBuilders;

public class SqliteFixture : IDisposable
{
    private readonly Fixture _autoFixture = new();
    public FireMothContext DbContext { get; }

    private const string TestDbFileName = "FireMoth_Integration_Testing.sqlite";

    public List<FileFingerprint> TestFileFingerprints { get; } = new();

    public SqliteFixture()
    {
        _autoFixture.Customizations.Add(new Base64HashSpecimenBuilder());
        _autoFixture.Customizations.Add(new FileNameSpecimenBuilder());

        var sqliteConnectionString = GetSqliteConnectionString();

        var dbContextOptions = new DbContextOptionsBuilder<FireMothContext>()
            .UseSqlite(sqliteConnectionString)
            .Options;

        DbContext = new FireMothContext(dbContextOptions);
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();
        
        GenerateTestData();
    }

    private static string GetSqliteConnectionString()
    {
        var tempDirectory = Path.GetTempPath();

        var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Join(tempDirectory, TestDbFileName)
        };
        
        return sqliteConnectionStringBuilder.ConnectionString;
    }

    private void GenerateTestData()
    {
        // Randomly generated test data
        TestFileFingerprints.AddRange(_autoFixture.CreateMany<FileFingerprint>(10));
        
        // Pre-defined test data
        TestFileFingerprints.AddRange(new List<FileFingerprint>
        {
            new("TestFileB.dat", "/TestDir/TestSubdirB", 250, "KsmmdGrKVDr43/OYlM/oFzr7oh6wHG+uM9UpRyIoVe8="),
            new("TestFileC.dat", "/TestDir/TestSubdirB", 251, "lD31FWzYbiOCwbhDN4KMZ6+p5F+Vf2TIqaIiQBV8EIg="),
            new("TestFileA.dat", "/TestDir/TestSubdirA", 252, "bTCaN71zwpnwavzYamAwzI+ST0fVhQ6kOJkJcI/JKdI=")
        });
        
        // Some more randomly generated test data
        TestFileFingerprints.AddRange(_autoFixture.CreateMany<FileFingerprint>(5));
    }
    
    public void InsertTestData()
    {
        DbContext.FileFingerprints.AddRange(TestFileFingerprints);
        DbContext.SaveChanges();
    }
    
    public void DeleteTestData()
    {
        DbContext.Database.ExecuteSqlRaw("DELETE FROM FileFingerprints");
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}