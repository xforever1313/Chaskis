
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using SethCS.Exceptions;
using System.IO;
using System.Text.RegularExpressions;

namespace Chaskis
{
    /// <summary>
    /// Parses the arguments for this program.
    /// </summary>
    public class ArgumentParser
    {
        // -------- Fields --------

        /// <summary>
        /// The argument for the irc config.
        /// </summary>
        private const string configArg = "--configPath=";

        /// <summary>
        /// The regex to search for a configuration regex.
        /// </summary>
        private const string configRegex = configArg + "(?<path>.+)";

        /// <summary>
        /// The argument for the irc plugin config.
        /// </summary>
        private const string pluginArg = "--pluginConfigPath=";

        /// <summary>
        /// The regex to search for a plugin regex.
        /// </summary>
        private const string pluginRegex = pluginArg + "(?<path>.+)";

        /// <summary>
        /// The argument for the irc plugin config.
        /// </summary>
        private const string failPluginArg = "--failOnBadPlugin=";

        /// <summary>
        /// The regex to search for a plugin regex.
        /// </summary>
        private const string failPluginRegex = failPluginArg + "(?<yesOrNo>(yes|no))";

        /// <summary>
        /// Default irc config location.
        /// </summary>
        public readonly string DefaultIrcConfigLocation;

        /// <summary>
        /// Default plugin config location;
        /// </summary>
        public readonly string DefaultIrcPluginConfigLocation;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="args">Arguments to parse</param>
        /// <param name="rootDir">Where the DEFAULT root of the config directory is.</param>
        public ArgumentParser( string[] args, string rootDir )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );
            ArgumentChecker.StringIsNotNullOrEmpty( rootDir, nameof( rootDir ) );

            this.DefaultIrcConfigLocation = Path.Combine( rootDir, "IrcConfig.xml" );
            this.DefaultIrcPluginConfigLocation = Path.Combine( rootDir, "PluginConfig.xml" );

            this.IrcConfigLocation = this.DefaultIrcConfigLocation;
            this.IrcPluginConfigLocation = this.DefaultIrcPluginConfigLocation;
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
                else if( arg.StartsWith( configArg ) )
                {
                    Match configMatch = Regex.Match( arg, configRegex );
                    if( configMatch.Success )
                    {
                        this.IrcConfigLocation = configMatch.Groups["path"].Value;
                    }
                    else
                    {
                        this.IsValid = false;
                    }
                }
                else if( arg.StartsWith( pluginArg ) )
                {
                    Match pluginMatch = Regex.Match( arg, pluginRegex );
                    if( pluginMatch.Success )
                    {
                        this.IrcPluginConfigLocation = pluginMatch.Groups["path"].Value;
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
        /// Where the irc config XML file is located.
        /// Defaulted to rootDir\IrcConfig.xml.
        /// </summary>
        public string IrcConfigLocation{ get; private set; }

        /// <summary>
        /// Where the irc plugin XML file is located.
        /// Defaulted to rootDir\PluginConfig.xml.
        /// </summary>
        public string IrcPluginConfigLocation{ get; private set; }

        /// <summary>
        /// Whether or not to print the help.
        /// </summary>
        public bool PrintHelp{ get; private set; }

        /// <summary>
        /// Whether or not to print the version information.
        /// </summary>
        public bool PrintVersion{ get; private set; }

        /// <summary>
        /// Whether or not to fail if the a plug loads, or attempt to continue.
        /// Defaulted to false (will not fail).
        /// </summary>
        public bool FailOnPluginFailure { get; private set; }

        /// <summary>
        /// Whether or not the args were parsed correctly.
        /// </summary>
        public bool IsValid{ get; private set; }
    }
}

