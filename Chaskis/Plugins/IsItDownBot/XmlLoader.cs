//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Xml;

namespace Chaskis.Plugins.IsItDownBot
{
    public static class XmlLoader
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// What the root node of the xml config should be.
        /// </summary>
        private const string rootNodeName = "isitdownbotconfig";

        // ---------------- Functions ----------------

        public static IsItDownBotConfig LoadConfig( string configPath )
        {
            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException( "Could not find " + nameof( IsItDownBotConfig ) + " file " + configPath );
            }

            XmlDocument doc = new XmlDocument();

            doc.Load( configPath );

            XmlElement rootNode = doc.DocumentElement;
            if( rootNode.Name != rootNodeName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + rootNodeName + "\".  Got: " + rootNode.Name
                );
            }

            IsItDownBotConfig config = new IsItDownBotConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                switch( childNode.Name )
                {
                    case "commandprefix":
                        config.CommandPrefix = childNode.InnerText;
                        break;

                    case "websites":
                        foreach( XmlNode websiteNode in childNode.ChildNodes )
                        {
                            switch( websiteNode.Name )
                            {
                                case "website":
                                    Website website = new Website();
                                    foreach( XmlNode websiteNodeChild in websiteNode.ChildNodes )
                                    {
                                        switch( websiteNodeChild.Name )
                                        {
                                            case "url":
                                                website.Url = websiteNodeChild.InnerText;
                                                break;

                                            case "interval":
                                                int minutes = int.Parse( websiteNodeChild.InnerText );
                                                website.CheckInterval = TimeSpan.FromMinutes( minutes );
                                                break;

                                            case "channel":
                                                website.Channels.Add( websiteNodeChild.InnerText );
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;

                }
            }

            return config;
        }
    }
}
