//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

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
                else if( "serverpasswordfile".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.ServerPassword = ReadFirstLineOfFile( childNode.Value, "Server Password File" );
                }
                else if( "nickservpasswordfile".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.NickServPassword = ReadFirstLineOfFile( childNode.Value, "NickServ Password File" );
                }
                else if( "ratelimit".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.RateLimit = int.Parse( childNode.Value );
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

        private static string ReadFirstLineOfFile( string fileName, string context )
        {
            if( string.IsNullOrWhiteSpace( fileName ) )
            {
                return string.Empty;
            }

            if( File.Exists( fileName ) == false )
            {
                throw new FileNotFoundException(
                    "Could not find file '" + fileName + "', which is needed for " + context
                );
            }

            using( FileStream stream = new FileStream( fileName, FileMode.Open, FileAccess.Read ) )
            {
                using( StreamReader reader = new StreamReader( stream ) )
                {
                    return reader.ReadLine();
                }
            }
        }

        /// <summary>
        /// Parses the given XML file and returns the IRC Plugin Config settings.
        /// </summary>
        /// <param name="fileName">The file name to parse.</param>
        /// <exception cref="FileNotFoundException">If the given filename does not exist.</exception>
        /// <exception cref="ArgumentNullException">If the XML has empty strings or the filename is null/empty.</exception>
        /// <exception cref="ApplicationException">If the irc config isn't valid.</exception>
        /// <returns>The IrcConfig objected based on the XML.</returns>
        public static IList<AssemblyConfig> ParsePluginConfig( string fileName )
        {
            if( File.Exists( fileName ) == false )
            {
                throw new FileNotFoundException( "Could not find IRC Config file " + fileName );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( fileName );

            XmlNode rootNode = doc.DocumentElement;
            if( rootNode.Name != pluginConfigRootNodeName )
            {
                throw new XmlException(
                    "Root XML node for plugin config should be named \"" + pluginConfigRootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            List<AssemblyConfig> plugins = new List<AssemblyConfig>();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                if( childNode.Name == "assembly" )
                {
                    List<string> blackListedChannels = new List<string>();

                    string path = string.Empty;
                    foreach( XmlAttribute attribute in childNode.Attributes )
                    {
                        switch( attribute.Name )
                        {
                            case "path":
                                path = attribute.Value;
                                break;
                        }
                    }

                    foreach( XmlNode assemblyChild in childNode.ChildNodes )
                    {
                        switch( assemblyChild.Name )
                        {
                            case "ignorechannel":
                                blackListedChannels.Add( assemblyChild.InnerText );
                                break;
                        }
                    }

                    plugins.Add( new AssemblyConfig( path, blackListedChannels ) );
                }
            }

            return plugins;
        }
    }
}