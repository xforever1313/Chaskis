//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;

namespace Chaskis.Plugins.QuoteBot
{
    public static class XmlLoader
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// What the root node of the xml config should be.
        /// </summary>
        private const string quoteBotConfigRootNode = "quotebotconfig";

        // ---------------- Functions ----------------

        /// <summary>
        /// Loads the given QuoteBotConfig from the given path,
        /// and returns the corresponding object.
        /// </summary>
        public static QuoteBotConfig LoadConfig( string configPath )
        {
            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException( "Could not find cowsay bot config Config file " + configPath );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( configPath );

            XmlElement rootNode = doc.DocumentElement;
            if( rootNode.Name != quoteBotConfigRootNode )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + quoteBotConfigRootNode + "\".  Got: " + rootNode.Name
                );
            }

            QuoteBotConfig config = new QuoteBotConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                switch( childNode.Name )
                {
                    case "addcommand":
                        config.AddCommand = childNode.InnerText;
                        break;

                    case "deletecommand":
                        config.DeleteCommand = childNode.InnerText;
                        break;

                    case "randomcommand":
                        config.RandomCommand = childNode.InnerText;
                        break;

                    case "getcommand":
                        config.GetCommand = childNode.InnerText;
                        break;
                }
            }

            config.Validate();

            return config;
        }
    }
}
