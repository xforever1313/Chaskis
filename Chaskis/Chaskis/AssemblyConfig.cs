//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using SethCS.Exceptions;

namespace Chaskis.Cli
{
    /// <summary>
    /// Contains information about an assembly.
    /// </summary>
    public class AssemblyConfig
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Absolute Path to the assembly</param>
        /// <param name="blackListedChannels">Channels that will ignore this assembly's handlers.</param>
        public AssemblyConfig( string path, IList<string> blackListedChannels )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( path, nameof( path ) );
            ArgumentChecker.IsNotNull( blackListedChannels, nameof( blackListedChannels ) );

            if( blackListedChannels.Any( c => string.IsNullOrEmpty( c ) || string.IsNullOrWhiteSpace( c ) ) )
            {
                throw new ArgumentException(
                    "black listed channels can not be null, empty, or whitespace.",
                    nameof( blackListedChannels )
                );
            }

            this.AssemblyPath = path;
            this.BlackListedChannels = new List<string>( blackListedChannels ).AsReadOnly();
        }

        // -------- Properties --------

        /// <summary>
        /// Absolute Path to the assembly.
        /// </summary>
        public string AssemblyPath { get; set; }

        /// <summary>
        /// Read-only list of channels where this plugin will ignore commands from.
        /// </summary>
        public IList<string> BlackListedChannels { get; private set; }
    }
}