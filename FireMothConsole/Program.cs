// <copyright file="Program.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console
{
    using System;

    /// <summary>
    /// Application entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Class and application entry point. Validates command-line arguments, performs startup
        /// configuration, and invokes the directory scanning process.
        /// </summary>
        /// <param name="args">Command-line arguments. A single argument, --directory, is currently
        /// supported and required. This value must be a well-formed and existing directory path.
        /// </param>
        public static void Main(string[] args)
        {
            var initializer = new Initializer(args, Console.Out);
            ExitState exitState = initializer.Start();

#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("Process completed with exit state: {0}.", exitState);
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        }
    }
}
