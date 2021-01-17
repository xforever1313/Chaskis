//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chaskis.Core;
using SethCS.Extensions;

namespace Chaskis.Cli
{
    /// <summary>
    /// Loads XML files and returns objects based on the contents.
    /// </summary>
    public static class XmlLoader
    {
        /// <summary>
        /// What the root node name of the irc config should be.
        /// </summary>
        private const string ircConfigRootNodeName = "ircbotconfig";

        /// <summary>
        /// What the root node of the plugin config should be.
        /// </summary>
        private const string pluginConfigRootNodeName = "pluginconfig";

        public static IReadOnlyIrcConfig ParseIrcConfigFromString( string str )
        {
            using( StringReader reader = new StringReader( str ) )
            {
                return ParseIrcConfigFromTextReader( reader );
            }
        }

        /// <summary>
        /// Parses the given XML file and returns the IRC Config settings.
        /// </summary>
        /// <param name="fileName">The file name to parse.</param>
        /// <exception cref="FileNotFoundException">If the given filename does not exist.</exception>
        /// <exception cref="FormatException">If the port in the XML file is invalid.</exception>
        /// <exception cref="ApplicationException">If the irc config isn't valid.</exception>
        /// <returns>The IrcConfig objected based on the XML.</returns>
        public static IReadOnlyIrcConfig ParseIrcConfigFromFile( string fileName )
        {
            if( File.Exists( fileName ) == false )
            {
                throw new FileNotFoundException( "Could not find IRC Config file " + fileName );
            }

            using( TextReader reader = File.OpenText( fileName ) )
            {
                return ParseIrcConfigFromTextReader( reader );
            }
        }

        private static IReadOnlyIrcConfig ParseIrcConfigFromTextReader( TextReader reader )
        {
            XDocument doc = XDocument.Load( reader );

            if( ircConfigRootNodeName.EqualsIgnoreCase( doc.Root.Name.LocalName ) == false )
            {
                throw new XmlException(
                    $"Root XML node should be named '{ircConfigRootNodeName}'.  Got: '{doc.Root.Name.LocalName}'"
                );
            }

            IrcConfig config = new IrcConfig();

            foreach( XElement childNode in doc.Root.Elements() )
            {
                if( "server".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.Server = childNode.Value;
                }
                else if( "port".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.Port = short.Parse( childNode.Value );
                }
                else if( "usessl".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.UseSsl = bool.Parse( childNode.Value );
                }
                else if( "username".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.UserName = childNode.Value;
                }
                else if( "nick".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.Nick = childNode.Value;
                }
                else if( "realname".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.RealName = childNode.Value;
                }
                else if( "quitmessage".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.QuitMessage = childNode.Value;
                }
                else if( "serverpassword".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.ServerPassword = childNode.Value;
                    foreach( XAttribute attr in childNode.Attributes() )
                    {
                        if( attr.Name.LocalName.EqualsIgnoreCase( "method" ) )
                        {
                            config.ServerPasswordMethod = ParsePasswordMethod( attr.Value );
                        }
                    }
                }
                else if( "nickservpassword".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.NickServPassword = childNode.Value;
                    foreach( XAttribute attr in childNode.Attributes() )
                    {
                        if( attr.Name.LocalName.EqualsIgnoreCase( "method" ) )
                        {
                            config.NickServPasswordMethod = ParsePasswordMethod( attr.Value );
                        }
                    }
                }
                else if( "nickservnick".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.NickServNick = childNode.Value;
                }
                else if( "nickservmessage".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.NickServMessage = childNode.Value;
                }
                else if( "ratelimit".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.RateLimit = TimeSpan.FromMilliseconds( int.Parse( childNode.Value ) );
                }
                else if( "watchdogtimeout".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.WatchdogTimeout = TimeSpan.FromSeconds( int.Parse( childNode.Value ) );
                }
                else if( "channels".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    foreach( XElement channelNode in childNode.Elements() )
                    {
                        if( "channel".EqualsIgnoreCase( channelNode.Name.LocalName ) )
                        {
                            config.Channels.Add( channelNode.Value );
                        }
                    }
                }
                else if( "bridgebots".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    foreach( XElement bridgeBotNode in childNode.Elements() )
                    {
                        if( "bridgebot".EqualsIgnoreCase( bridgeBotNode.Name.LocalName ) )
                        {
                            string botName = string.Empty;
                            string botRegex = string.Empty;

                            foreach( XElement bridgeBotChild in bridgeBotNode.Elements() )
                            {
                                if( "botname".EqualsIgnoreCase( bridgeBotChild.Name.LocalName ) )
                                {
                                    botName = bridgeBotChild.Value;
                                }
                                else if( "botregex".EqualsIgnoreCase( bridgeBotChild.Name.LocalName ) )
                                {
                                    botRegex = bridgeBotChild.Value;
                                }
                            }

                            config.BridgeBots.Add( botName, botRegex );
                        }
                    }
                }
                else if( "admins".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    foreach( XElement adminNode in childNode.Elements() )
                    {
                        if( "admin".EqualsIgnoreCase( adminNode.Name.LocalName ) )
                        {
                            config.Admins.Add( adminNode.Value.ToLower() );
                        }
                    }
                }
            }

            config.Validate();

            return config;
        }

        private static PasswordMethod ParsePasswordMethod( string str )
        {
            foreach( PasswordMethod method in Enum.GetValues( typeof( PasswordMethod ) ))
            {
                if( str.EqualsIgnoreCase( method.ToString() ) )
                {
                    return method;
                }
            }

            throw new FormatException( $"Invalid {nameof( PasswordMethod )}: {str}" );
        }

        public static IList<AssemblyConfig> ParsePluginConfigFromString( string xmlString )
        {
            using( StringReader reader = new StringReader( xmlString ) )
            {
                return ParsePluginConfigFromTextReader( reader );
            }
        }

        /// <summary>
        /// Parses the given XML file and returns the IRC Plugin Config settings.
        /// </summary>
        /// <param name="fileName">The file name to parse.</param>
        /// <exception cref="FileNotFoundException">If the given filename does not exist.</exception>
        /// <returns>The IrcConfig objected based on the XML.</returns>
        public static IList<AssemblyConfig> ParsePluginConfigFromFile( string fileName )
        {
            if( File.Exists( fileName ) == false )
            {
                throw new FileNotFoundException( "Could not find IRC Config file " + fileName );
            }

            using( TextReader reader = File.OpenText( fileName ) )
            {
                return ParsePluginConfigFromTextReader( reader );
            }
        }

        private static IList<AssemblyConfig> ParsePluginConfigFromTextReader( TextReader reader )
        {
            XDocument doc = XDocument.Load( reader );

            if( pluginConfigRootNodeName.EqualsIgnoreCase( doc.Root.Name.LocalName ) == false )
            {
                throw new XmlException(
                    $"Root XML node for plugin config should be named '{pluginConfigRootNodeName}'.  Got: '{doc.Root.Name.LocalName}'"
                );
            }

            List<AssemblyConfig> plugins = new List<AssemblyConfig>();

            foreach( XElement childNode in doc.Root.Elements() )
            {
                if( "assembly".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    List<string> blackListedChannels = new List<string>();
                    string path = string.Empty;

                    foreach( XAttribute attribute in childNode.Attributes() )
                    {
                        if( "path".EqualsIgnoreCase( attribute.Name.LocalName ) )
                        {
                            path = attribute.Value;
                        }
                    }

                    foreach( XElement assemblyChild in childNode.Elements() )
                    {
                        if( "ignorechannel".EqualsIgnoreCase( assemblyChild.Name.LocalName ) )
                        {
                            blackListedChannels.Add( assemblyChild.Value );
                        }
                    }

                    plugins.Add( new AssemblyConfig( path, blackListedChannels ) );
                }
            }

            return plugins;
        }
    }
}