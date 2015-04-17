// <copyright file="Program.cs" company="Bauer Consumer Media">
//     Copyright (c) 2015 Bauer Consumer Media. All rights reserved.
// </copyright>

namespace ResponseTimeMonitor
{
    /// <summary>
    /// Program startup class
    /// </summary>
    public sealed class Program
    {
        #region Methods

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var runner = new RequestRunner();
            runner.Execute(args);
        }

        #endregion
    }
}
