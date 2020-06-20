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
     * Start:
     * - Application startup arguments must be valid
     *      - There must be one and only one argument.
     *          - Start_InvalidArgumentCount_ReturnsExitStateStartupError
     * - Valid application startup arguments returns ScanSuccess result.
     *      - Start_ValidPathArgument_ReturnsScanSuccess
     */
    public class InitializerTests : IDisposable
    {
        private const string UsageText = "Usage: FireMoth.Console.exe --directory [ScanDirectory]";
        private readonly StringWriter outputWriter;
        private bool disposed = false;

        public InitializerTests()
        {
            this.outputWriter = new StringWriter(new StringBuilder());
        }

        [Fact]
        public void Initialize_ValidArguments_ReturnsTrue()
        {
            // Arrange
            string[] arguments = { "--directory", @"C:\testdir" };
            Initializer initializer = new Initializer(arguments, this.outputWriter);

            // Act
            var result = initializer.Initialize();

            // Assert
            Assert.True(result);
        }

        /*
        [Fact]
        public void Initialize_ValidArguments_OptionsSet()
        {
            var testOption = "--directory";
            var testValue = @"C:\testdir";
            string[] arguments = { testOption, testValue };

            Initializer initializer = new Initializer(arguments, this.outputWriter);
            var initResult = initializer.Initialize();

            var option = initializer.GetOption("directory");

            Assert.NotNull(option);
            Assert.Equal(testValue, option);
        }
        */

        [Theory]
        [InlineData("--badoption")]
        [InlineData("--badoption", "C:\\")]
        [InlineData("--directory", "C:\\", "extraoption")]
        public void Start_InvalidArguments_DisplaysUsageAndReturnStartupErrorExitState(
            params string[] arguments)
        {
            // Arrange
            var initializer = new Initializer(arguments, this.outputWriter);

            // Act
            ExitState result = initializer.Start();

            // Assert
            Assert.StartsWith(
                "Invalid option: ", this.outputWriter.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(
                UsageText + Environment.NewLine,
                this.outputWriter.ToString(),
                StringComparison.OrdinalIgnoreCase);
            Assert.Equal(ExitState.StartupError, result);
        }

        [Fact]
        public void Start_ZeroLengthArgumentsArray_DisplaysUsageAndReturnsStartupErrorExitState()
        {
            // Arrange
            var arguments = Array.Empty<string>();
            var initializer = new Initializer(arguments, this.outputWriter);

            // Act
            ExitState result = initializer.Start();

            // Assert
            Assert.EndsWith(
                UsageText + Environment.NewLine,
                this.outputWriter.ToString(),
                StringComparison.OrdinalIgnoreCase);
            Assert.Equal(ExitState.StartupError, result);
        }

        [Fact]
        public void Start_ValidArguments_ReturnNormalExitState()
        {
            // Arrange
            var arguments = new string[] { "--directory", "C:\\" };
            var initializer = new Initializer(arguments, this.outputWriter);

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
