//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Text.RegularExpressions;
using SethCS.Basic;
using SethCS.Exceptions;

namespace Chaskis.Cli
{
    /// <summary>
    /// Parses the arguments for this program.
    /// </summary>
    public class ArgumentParser
    {
        // -------- Fields --------

        /// <summary>
        /// The argument for the chaskis root.
        /// </summary>
        private const string rootArg = "--chaskisroot=";

        /// <summary>
        /// The regex to search for a configuration regex.
        /// </summary>
        private const string rootRegex = rootArg + "(?<path>.+)";

        /// <summary>
        /// The argument to turn on bootstrapping.
        /// </summary>
        private const string bootStrapArg = "--bootstrap";

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="args">Arguments to parse</param>
        /// <param name="defaultRootDir">Where the DEFAULT root of the config directory is.</param>
        public ArgumentParser( IEnumerable<string> args, string defaultRootDir )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );
            ArgumentChecker.StringIsNotNullOrEmpty( defaultRootDir, nameof( defaultRootDir ) );

            this.ChaskisRoot = defaultRootDir;
            this.IsValid = true;

            foreach( string arg in args )
            {
                if(
                    ( arg == "--help" ) ||
                    ( arg == "-h" ) ||
                    ( arg == "/?" ) )
                {
                    this.PrintHelp = true;
                }
                else if( arg == "--version" )
                {
                    this.PrintVersion = true;
                }
                else if( arg == bootStrapArg )
                {
                    this.BootStrap = true;
                }
                else if( arg.StartsWith( rootArg ) )
                {
                    Match configMatch = Regex.Match( arg, rootRegex );
                    if( configMatch.Success )
                    {
                        this.ChaskisRoot = configMatch.Groups["path"].Value;
                    }
                    else
                    {
                        this.IsValid = false;
                    }
                }
                else
                {
                    this.IsValid = false;
                    StaticLogger.Log.ErrorWriteLine( "Unknown Argument: " + arg );
                }
            }
        }

        // -------- Properties --------

        /// <summary>
        /// Where we will treat our Chaskis config root.
        /// Defaulted to AppData/Chaskis.
        /// </summary>
        public string ChaskisRoot { get; private set; }

        /// <summary>
        /// Whether or not to print the help.
        /// </summary>
        public bool PrintHelp { get; private set; }

        /// <summary>
        /// Whether or not to print the version information.
        /// </summary>
        public bool PrintVersion { get; private set; }

        /// <summary>
        /// Whether or not the args were parsed correctly.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Should we bootstrap or not.
        /// </summary>
        public bool BootStrap { get; private set; }
    }
}