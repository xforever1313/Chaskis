//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Xml;

namespace Chaskis.Plugins.RssBot
{
    public static class XmlLoader
    {
        // ---------------- Fields ----------------

        private const string rootXmlElementName = "rssbotconfig";

        // ---------------- Functions ----------------

        public static RssBotConfig ParseConfig( string fileName )
        {
            if( File.Exists( fileName ) == false )
            {
                throw new FileNotFoundException( "Can not find Rss Bot Config " + fileName );
            }

            XmlDocument doc = new XmlDocument();
            doc.Load( fileName );

            XmlNode rootNode = doc.DocumentElement;
            if( rootNode.Name != rootXmlElementName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + rootXmlElementName + "\".  Got: " + rootNode.Name
                );
            }

            RssBotConfig config = new RssBotConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                if( childNode.Name == "feed" )
                {
                    Feed feed = new Feed();

                    foreach( XmlNode feedNode in childNode.ChildNodes )
                    {
                        switch( feedNode.Name )
                        {
                            case "url":
                                feed.Url = feedNode.InnerText;
                                break;

                            case "refreshinterval":
                                long minutes = long.Parse( feedNode.InnerText );
                                feed.RefreshInterval = TimeSpan.FromMinutes( minutes );
                                break;

                            case "channel":
                                feed.AddChannel( feedNode.InnerText );
                                break;
                        }
                    }

                    config.AddFeed( feed );
                }
            }

            config.Validate();

            return config;
        }
    }
}
