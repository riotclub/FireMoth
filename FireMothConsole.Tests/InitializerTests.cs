// <copyright file="InitializerTests.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console
{
    using System;
    using System.IO;
    using System.Text;
    using Xunit;

    /*
     * x Init with valid command line arguments.
     *      x Initialize_ValidDirectoryArgument_ReturnsTrue
     *      x Initialize_ValidDirectoryArgument_SetsDirectoryOption
     *
     * x Init without directory argument.
     *      x Initialize_NoArguments_ReturnsFalse
     *      x Initialize_NoArguments_OutputsError
     *
     * x Init with unknown argument.
     *      x Initialize_UnknownArgument_ReturnsFalse
     *      x Initialize_UnknownArgument_OutputsError
     *
     * Init with invalid directory argument displays error (don't check for directory existence).
     *      Initialize_InvalidDirectoryArgument_ReturnsFalse
     *      Initialize_InvalidDirectoryArgument_OutputsError
     *
     * Start when initialized runs file scanner.
     *      Start_Initialized_StartsDirectoryScan
     *
     * Start when not initialized throws exception.
     *      Start_NotInitialized_ThrowsIllegalStateException
     */
    public class InitializerTests : IDisposable
    {
        private readonly StringWriter outputWriter;
        private bool disposed = false;

        public InitializerTests()
        {
            this.outputWriter = new StringWriter(new StringBuilder());
        }

        [Fact]
        public void Initialize_ValidDirectoryArgument_ReturnsTrue()
        {
            // Arrange
            string[] arguments = { "--directory", @"C:\testdir" };
            Initializer initializer = new Initializer(arguments, this.outputWriter);

            // Act
            var result = initializer.Initialize();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Initialize_ValidDirectoryArgument_SetsDirectoryOption()
        {
            // Arrange
            var testOption = "--directory";
            var testValue = @"C:\testdir";
            string[] arguments = { testOption, testValue };
            Initializer initializer = new Initializer(arguments, this.outputWriter);

            // Act
            initializer.Initialize();

            // Assert
            Assert.NotNull(initializer.CommandLineOptions);
            Assert.Equal(testValue, initializer.CommandLineOptions.ScanDirectory);
        }

        [Fact]
        public void Initialize_NoArguments_ReturnsFalse()
        {
            // Arrange
            var arguments = Array.Empty<string>();
            Initializer initializer = new Initializer(arguments, this.outputWriter);

            // Act
            var result = initializer.Initialize();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Initialize_NoArguments_OutputsError()
        {
            // Arrange
            var arguments = Array.Empty<string>();
            Initializer initializer = new Initializer(arguments, this.outputWriter);

            // Act
            initializer.Initialize();

            // Assert
            Assert.Contains(
                "Required option 'd, directory' is missing.",
                this.outputWriter.ToString(),
                StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("--badoption")]
        [InlineData("--badoption", "C:\\")]
        [InlineData("--directory", "C:\\", "-x")]
        public void Initialize_UnknownArgument_ReturnsFalse(params string[] arguments)
        {
            // Arrange
            var initializer = new Initializer(arguments, this.outputWriter);

            // Act
            var result = initializer.Initialize();

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("--badoption")]
        [InlineData("--badoption", "C:\\")]
        [InlineData("--directory", "C:\\", "-x")]
        public void Initialize_UnknownArgument_OutputsError(params string[] arguments)
        {
            // Arrange
            Initializer initializer = new Initializer(arguments, this.outputWriter);

            // Act
            initializer.Initialize();

            // Assert
            Assert.Matches("Option '[a-zA-Z]*' is unknown", this.outputWriter.ToString());
        }

        /*
        [Theory]
        [InlineData("-d", "C:\\")]
        public void Initialize_InvalidDirectoryArgument_ReturnsFalse(params string[] arguments)
        {

        }
        */

        [Fact]
        public void Start_ValidArguments_ReturnNormalExitState()
        {
            // Arrange
            var arguments = new string[] { "--directory", "C:\\" };
            var initializer = new Initializer(arguments, this.outputWriter);
            initializer.Initialize();

            // Act
            ExitState result = initializer.Start();

            // Assert
            Assert.Equal(ExitState.Normal, result);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.outputWriter.Dispose();
            }

            this.disposed = true;
        }
    }
}
