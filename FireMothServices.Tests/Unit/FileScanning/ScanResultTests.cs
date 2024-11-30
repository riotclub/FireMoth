// <copyright file="ScanResultTests.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tests.Unit.FileScanning;

using System;
using System.Collections.Generic;
using System.Linq;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Tests.Common.AutoFixture.SpecimenBuilders;
using AutoFixture;
using FluentAssertions;
using Services.Orchestration;
using Xunit;

/// <summary>
/// <p>
/// Ctor<br/>
/// - Initializes all collection properties.<br/>
/// </p>
/// <p>
/// GetScannedFiles (Property)<br/>
/// - Returns the proper value of the ScannedFiles property.<br/>
/// </p>
/// <p>
/// GetSkippedFiles (Property)<br/>
/// - Returns the proper value of the SkippedFiles property.<br/>
/// </p>
/// <p>
/// GetErrors (Property)<br/>
/// - Returns the proper value of the Errors property.<br/>
/// </p>
/// <p>
/// OperatorAddition<br/>
/// - Passing [a:null, b:null] throws ArgumentNullException.<br/>
/// - Passing [a:null, b:non-null] returns new ScanResult with values from b.<br/>
/// - Passing [a:non-null, b:null] returns new ScanResult with values from a.<br/>
/// - Passing [a:non-null, b:non-null] returns new ScanResult with combined values from both a and
///   b.<br/>
/// </p>
/// </summary>
public class ScanResultTests
{
    private readonly Fixture _fixture = new();

    public ScanResultTests()
    {
        _fixture.Customizations.Add(new Base64HashSpecimenBuilder());
    }
    
#region Ctor
    /// <summary>Ctor: Initializes all collection properties.</summary>
    [Fact]
    public void Ctor_MethodCalled_InitializesCollectionProperties()
    {
        // Arrange, Act
        var sut = new ScanResult();

        // Assert
        sut.ScannedFiles.Should().NotBeNull().And.BeEmpty();
        sut.SkippedFiles.Should().NotBeNull().And.BeEmpty();
        sut.Errors.Should().NotBeNull().And.BeEmpty();
    }
#endregion

#region GetScannedFiles
    /// <summary>GetScannedFiles: Returns the proper value of the ScannedFiles property.</summary>
    [Fact]
    public void GetScannedFiles_MethodCalled_ReturnsCorrectValue()
    {
        // Arrange
        var expected = _fixture.CreateMany<FileFingerprint>(10).ToList();
        var sut = new ScanResult();
        sut.ScannedFiles.AddRange(expected);
        
        // Act
        var result = sut.ScannedFiles;

        // Assert
        result.Should().Equal(expected);
    }
#endregion

#region GetSkippedFiles
    /// <summary>GetSkippedFiles: Returns the proper value of the SkippedFiles property.</summary>
    [Fact]
    public void GetSkilledFiles_MethodCalled_ReturnsCorrectValue()
    {
        // Arrange
        var expected = _fixture.CreateMany<KeyValuePair<string, string>>(10).ToList();
        var sut = new ScanResult();
        foreach (var pair in expected)
            sut.SkippedFiles.Add(pair.Key, pair.Value);
        
        // Act
        var result = sut.SkippedFiles;

        // Assert
        result.Should().Equal(expected);
    }
#endregion

#region GetErrors
    /// <summary>GetErrors: Returns the proper value of the Errors property.</summary>
    [Fact]
    public void GetErrors_MethodCalled_ReturnsCorrectValue()
    {
        // Arrange
        var expected = _fixture.CreateMany<ScanError>(10).ToList();
        var sut = new ScanResult();
        sut.Errors.AddRange(expected);
            
        // Act
        var result = sut.Errors;

        // Assert
        result.Should().Equal(expected);
    }
#endregion

#region OperatorAddition
    /// <summary>OperatorAddition: Passing [a:null, b:null] throws ArgumentNullException.</summary>
    [Fact]
    public void OperatorAddition_ANullBNull_ThrowsArgumentNullException()
    {
        // Arrange
        ScanResult sutA = null!;
        ScanResult sutB = null!;
        
        // Act
        var additionFunc = () => sutA + sutB;

        // Assert
        additionFunc.Should().ThrowExactly<ArgumentNullException>();
    }
    
    /// <summary>OperatorAddition: Passing [a:null, b:non-null] returns new ScanResult with values
    /// from b.</summary>
    [Fact]
    public void OperatorAddition_ANullBNonNull_ReturnsCorrectScanResult()
    {
        // Arrange
        var expectedScannedFiles = _fixture.CreateMany<FileFingerprint>(10).ToList();
        var expectedErrors = _fixture.CreateMany<ScanError>(10).ToList();
        var expectedSkippedFiles = _fixture.CreateMany<KeyValuePair<string, string>>(10).ToList();
        var sutB = new ScanResult();
        sutB.ScannedFiles.AddRange(expectedScannedFiles);
        sutB.Errors.AddRange(expectedErrors);
        foreach (var pair in expectedSkippedFiles)
            sutB.SkippedFiles.Add(pair.Key, pair.Value);
        
        ScanResult sutA = null!;
        
        // Act
        var result = sutA + sutB;

        // Assert
        result.ScannedFiles.Should().Equal(expectedScannedFiles);
        result.SkippedFiles.Should().Equal(expectedSkippedFiles);
        result.Errors.Should().Equal(expectedErrors);
    }
      
    /// <summary>OperatorAddition: Passing [a:non-null, b:null] returns new ScanResult with values
    /// from a.</summary>
    [Fact]
    public void OperatorAddition_ANonNullBNull_ReturnsCorrectScanResult()
    {
        // Arrange
        var expectedScannedFiles = _fixture.CreateMany<FileFingerprint>(10).ToList();
        var expectedErrors = _fixture.CreateMany<ScanError>(10).ToList();
        var expectedSkippedFiles = 
            _fixture.CreateMany<KeyValuePair<string, string>>(10).ToDictionary();
        var sutA = new ScanResult();
        sutA.ScannedFiles.AddRange(expectedScannedFiles);
        sutA.Errors.AddRange(expectedErrors);
        foreach (var pair in expectedSkippedFiles)
            sutA.SkippedFiles.Add(pair.Key, pair.Value);
        
        ScanResult sutB = null!;
        
        // Act
        var result = sutA + sutB;

        // Assert
        result.ScannedFiles.Should().Equal(expectedScannedFiles);
        result.SkippedFiles.Should().Equal(expectedSkippedFiles);
        result.Errors.Should().Equal(expectedErrors);
    }
    
    /// <summary>OperatorAddition: Passing [a:non-null, b:non-null] returns new ScanResult with
    /// combined values from both a and b.</summary>
    [Fact]
    public void OperatorAddition_ANonNullBNonNull_ReturnsCorrectScanResult()
    {
        // Arrange
        var testScannedFilesA = _fixture.CreateMany<FileFingerprint>(10).ToList();
        var testScannedFilesB = _fixture.CreateMany<FileFingerprint>(10).ToList();
        var testErrorsA = _fixture.CreateMany<ScanError>(10).ToList();
        var testErrorsB = _fixture.CreateMany<ScanError>(10).ToList();
        var testSkippedFilesA =
            _fixture.CreateMany<KeyValuePair<string, string>>(10).ToDictionary();
        var testSkippedFilesB =
            _fixture.CreateMany<KeyValuePair<string, string>>(10).ToDictionary();
        
        var sutA = new ScanResult();
        sutA.ScannedFiles.AddRange(testScannedFilesA);
        sutA.Errors.AddRange(testErrorsA);
        foreach (var pair in testSkippedFilesA)
            sutA.SkippedFiles.Add(pair.Key, pair.Value);
        
        var sutB = new ScanResult();
        sutB.ScannedFiles.AddRange(testScannedFilesB);
        sutB.Errors.AddRange(testErrorsB);
        foreach (var pair in testSkippedFilesB)
            sutB.SkippedFiles.Add(pair.Key, pair.Value);

        testScannedFilesA.AddRange(testScannedFilesB);
        var expectedScannedFiles = testScannedFilesA.ToList();
        testErrorsA.AddRange(testErrorsB);
        var expectedErrors = testErrorsA.ToList();
        
        foreach (var pair in testSkippedFilesB)
            testSkippedFilesA.TryAdd(pair.Key, pair.Value);

        // Act
        var result = sutA + sutB;

        // Assert
        result.ScannedFiles.Should().Equal(expectedScannedFiles);
        result.SkippedFiles.Should().Equal(testSkippedFilesA);
        result.Errors.Should().Equal(expectedErrors);
    }
#endregion
}