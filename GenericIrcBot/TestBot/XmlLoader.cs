
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using GenericIrcBot;

namespace TestBot
{
    /// <summary>
    /// Loads XML files and returns objects based on the contents.
    /// </summary>
    public static class XmlLoader
    {
        /// <summary>
        /// What the root node name should be.
        /// </summary>
        private const string rootNodeName = "ircbotconfig";

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
            if ( File.Exists( fileName ) == false )
            {
                throw new FileNotFoundException( "Could not find IRC Config file " + fileName );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( fileName );

            XmlElement rootNode = doc.DocumentElement;
            if ( rootNode.Name != rootNodeName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + rootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            IrcConfig config = new IrcConfig();

            foreach ( XmlElement childNode in rootNode.ChildNodes )
            {
                switch ( childNode.Name )
                {
                    case "server":
                        config.Server = childNode.InnerText;
                        break;

                    case "channel":
                        config.Channel = childNode.InnerText;
                        break;

                    case "port":
                        config.Port = short.Parse( childNode.InnerText );
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

                    case "password":
                        config.Password = childNode.InnerText;
                        break;
                }
            }

            config.Validate();

            return config;
        }
    }
}
