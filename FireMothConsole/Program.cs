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
        /// <returns>An <c>int</c> return code indicating invocation result.</returns>
        public static int Main(string[] args)
        {
            var initializer = new Initializer(args, Console.Out);
            bool initResult = initializer.Initialize();

            if (initResult)
            {
                ExitState exitState = initializer.Start();
                Console.WriteLine("Process completed with exit state: {0}.", exitState);
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}