
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using SethCS.Exceptions;

namespace TestBot
{
    /// <summary>
    /// Contains information about an assembly.
    /// </summary>
    public struct AssemblyConfig
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Absolute Path to the assembly</param>
        /// <param name="className">The class name inside the assembly to call to get the IRC handlers.</param>
        public AssemblyConfig( string path, string className )
            : this()
        {
            ArgumentChecker.StringIsNotNullOrEmpty( path, nameof( path ) );
            ArgumentChecker.StringIsNotNullOrEmpty( className, nameof( className ) );

            this.AssemblyPath = path;
            this.ClassName = className;
        }

        // -------- Properties --------

        /// <summary>
        /// Absolute Path to the assembly.
        /// </summary>
        public string AssemblyPath{ get; set; }

        /// <summary>
        /// The class name inside the assembly to call
        /// to get the IRC handlers.
        /// </summary>
        public string ClassName{ get; set; }
    }
}

