//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;

namespace Chaskis.Plugins.WelcomeBot
{
    public static class XmlLoader
    {
        // ---------------- Fields ----------------

        private const string welcomeBotRootNodeName = "welcomebotconfig";

        // ---------------- Functions ----------------

        public static WelcomeBotConfig LoadConfig( string xmlFilePath )
        {
            if( File.Exists( xmlFilePath ) == false )
            {
                throw new FileNotFoundException( "Could not find welcomebot config file " + xmlFilePath );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( xmlFilePath );

            XmlElement rootNode = doc.DocumentElement;
            if( rootNode.Name != welcomeBotRootNodeName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + welcomeBotRootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            WelcomeBotConfig config = new WelcomeBotConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                switch( childNode.Name )
                {
                    case "joinmessages":
                        config.EnableJoinMessages = bool.Parse( childNode.InnerText );
                        break;

                    case "partmessages":
                        config.EnablePartMessages = bool.Parse( childNode.InnerText );
                        break;

                    case "kickmessages":
                        config.EnableKickMessages = bool.Parse( childNode.InnerText );
                        break;

                    case "karmabotintegration":
                        config.KarmaBotIntegration = bool.Parse( childNode.InnerText );
                        break;

                    default:
                        // Ignore everything else, we don't care about anything else.
                        break;
                }
            }

            config.Validate();

            return config;
        }
    }
}
