//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chaskis.Core;

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

        /// <summary>
        /// Parses the given XML file and returns the IRC Config settings.
        /// </summary>
        /// <param name="fileName">The file name to parse.</param>
        /// <exception cref="FileNotFoundException">If the given filename does not exist.</exception>
        /// <exception cref="FormatException">If the port in the XML file is invalid.</exception>
        /// <exception cref="ApplicationException">If the irc config isn't valid.</exception>
        /// <returns>The IrcConfig objected based on the XML.</returns>
        public static IIrcConfig ParseIrcConfig( string fileName )
        {
            if( File.Exists( fileName ) == false )
            {
                throw new FileNotFoundException( "Could not find IRC Config file " + fileName );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( fileName );

            XmlNode rootNode = doc.DocumentElement;
            if( rootNode.Name != ircConfigRootNodeName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + ircConfigRootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            IrcConfig config = new IrcConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                switch( childNode.Name.ToLower() )
                {
                    case "server":
                        config.Server = childNode.InnerText;
                        break;

                    case "channels":
                        foreach( XmlNode channelNode in childNode.ChildNodes )
                        {
                            switch( channelNode.Name )
                            {
                                case "channel":
                                    config.Channels.Add( channelNode.InnerText );
                                    break;
                            }
                        }
                        break;

                    case "port":
                        config.Port = short.Parse( childNode.InnerText );
                        break;

                    case "usessl":
                        config.UseSsl = bool.Parse( childNode.InnerText );
                        break;

                    case "username":
                        config.UserName = childNode.InnerText;
                        break;

                    case "nick":
                        config.Nick = childNode.InnerText;
                        break;

                    case "realname":
                        config.RealName = childNode.InnerText;
                        break;

                    case "quitmessage":
                        config.QuitMessage = childNode.InnerText;
                        break;

                    case "bridgebots":
                        foreach( XmlNode bridgeBotNode in childNode.ChildNodes )
                        {
                            if( bridgeBotNode.Name == "bridgebot" )
                            {
                                string botName = string.Empty;
                                string botRegex = string.Empty;

                                foreach( XmlNode bridgeBotChild in bridgeBotNode.ChildNodes )
                                {
                                    switch( bridgeBotChild.Name )
                                    {
                                        case "botname":
                                            botName = bridgeBotChild.InnerText;
                                            break;

                                        case "botregex":
                                            botRegex = bridgeBotChild.InnerText;
                                            break;
                                    }
                                }
                                config.BridgeBots.Add( botName, botRegex );
                            }
                        }
                        break;

                    case "admins":
                        foreach( XmlNode adminNode in childNode.ChildNodes )
                        {
                            if( adminNode.Name == "admin" )
                            {
                                config.Admins.Add( adminNode.InnerText.ToLower() );
                            }
                        }
                        break;

                    case "serverpasswordfile":
                        config.ServerPassword = ReadFirstLineOfFile( childNode.InnerText, "Server Password File" );
                        break;

                    case "nickservpasswordfile":
                        config.NickServPassword = ReadFirstLineOfFile( childNode.InnerText, "NickServ Password File" );
                        break;

                    case "ratelimit":
                        config.RateLimit = int.Parse( childNode.InnerText );
                        break;
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