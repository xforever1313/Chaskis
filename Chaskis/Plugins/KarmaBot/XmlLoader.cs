//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;

namespace Chaskis.Plugins.KarmaBot
{
    public static class XmlLoader
    {
        /// <summary>
        /// Root node of the XML config.
        /// </summary>
        private const string karmaBotConfigRootNode = "karmabotconfig";

        /// <summary>
        /// Loads the karma bot config from the given xml file.
        /// </summary>
        /// <param name="xmlFilePath">The file name to load.</param>
        /// <returns>A karma bot config object from the given xml file.</returns>
        public static KarmaBotConfig LoadKarmaBotConfig( string xmlFilePath )
        {
            if( File.Exists( xmlFilePath ) == false )
            {
                throw new FileNotFoundException( "Could not find karma bot plugin config file " + xmlFilePath );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( xmlFilePath );

            XmlElement rootNode = doc.DocumentElement;
            if( rootNode.Name != karmaBotConfigRootNode )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + karmaBotConfigRootNode + "\".  Got: " + rootNode.Name
                );
            }

            KarmaBotConfig config = new KarmaBotConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                switch( childNode.Name )
                {
                    case "increasecmd":
                        config.IncreaseCommandRegex = childNode.InnerText;
                        break;

                    case "decreasecmd":
                        config.DecreaseCommandRegex = childNode.InnerText;
                        break;

                    case "querycmd":
                        config.QueryCommand = childNode.InnerText;
                        break;
                }
            }
            config.Validate();
            return config;
        }
    }
}