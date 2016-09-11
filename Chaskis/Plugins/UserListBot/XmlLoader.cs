
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chaskis.Plugins.UserListBot
{
    public static class XmlLoader
    {
        // -------- Fields --------

        private const string rootNodeName = "userlistconfig";

        // -------- Functions --------

        /// <summary>
        /// Parses a user list bot config from the given XML file.
        /// </summary>
        /// <param name="xmlFilePath">The path of the XML file.</param>
        /// <returns>The parsed user list bot config.</returns>
        public static UserListBotConfig LoadConfig( string xmlFilePath )
        {
            if ( File.Exists( xmlFilePath ) == false )
            {
                throw new FileNotFoundException( "Could not find user list bot config file " + xmlFilePath );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( xmlFilePath );

            XmlElement rootNode = doc.DocumentElement;
            if ( rootNode.Name != rootNodeName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + rootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            UserListBotConfig config = new UserListBotConfig();

            foreach ( XmlNode childNode in rootNode.ChildNodes )
            {
                switch ( childNode.Name )
                {
                    case "command":
                        config.Command = childNode.InnerText;
                        break;

                    case "cooldown":
                        config.Cooldown = int.Parse( childNode.InnerText );
                        break;
                }
            }

            config.Validate();

            return config;
        }
    }
}
