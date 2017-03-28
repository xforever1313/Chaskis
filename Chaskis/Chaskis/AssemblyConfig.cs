//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using SethCS.Exceptions;

namespace Chaskis
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
        public AssemblyConfig( string path )
            : this()
        {
            ArgumentChecker.StringIsNotNullOrEmpty( path, nameof( path ) );

            this.AssemblyPath = path;
        }

        // -------- Properties --------

        /// <summary>
        /// Absolute Path to the assembly.
        /// </summary>
        public string AssemblyPath { get; set; }
    }
}