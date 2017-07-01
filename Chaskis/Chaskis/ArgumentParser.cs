//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis
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
        /// The argument for the irc plugin config.
        /// </summary>
        private const string failPluginArg = "--failOnBadPlugin=";

        /// <summary>
        /// The regex to search for a plugin regex.
        /// </summary>
        private const string failPluginRegex = failPluginArg + "(?<yesOrNo>(yes|no))";

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
        public ArgumentParser( string[] args, string defaultRootDir )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );
            ArgumentChecker.StringIsNotNullOrEmpty( defaultRootDir, nameof( defaultRootDir ) );

            this.ChaskisRoot = defaultRootDir;
            this.FailOnPluginFailure = false;
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
                else if( arg == "--bootstrap" )
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
                else if( arg.StartsWith( failPluginArg ) )
                {
                    Match failMatch = Regex.Match( arg, failPluginRegex );
                    if( failMatch.Success )
                    {
                        if( failMatch.Groups["yesOrNo"].Value == "yes" )
                        {
                            this.FailOnPluginFailure = true;
                        }
                        else
                        {
                            this.FailOnPluginFailure = false;
                        }
                    }
                    else
                    {
                        this.IsValid = false;
                    }
                }
                else
                {
                    this.IsValid = false;
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
        /// Whether or not to fail if the a plug loads, or attempt to continue.
        /// Defaulted to false (will not fail).
        /// </summary>
        public bool FailOnPluginFailure { get; private set; }

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