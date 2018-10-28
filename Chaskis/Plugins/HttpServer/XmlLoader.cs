//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;

namespace Chaskis.Plugins.HttpServer
{
    public static class XmlLoader
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// What the root node of the xml config should be.
        /// </summary>
        private const string rootNodeName = "httpserverconfig";

        // ---------------- Functions ----------------

        public static HttpServerConfig LoadConfig( string configPath )
        {
            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException( "Could not find " + nameof( HttpServerConfig ) + " file " + configPath );
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

            HttpServerConfig config = new HttpServerConfig();

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                switch( childNode.Name )
                {
                    case "port":
                        config.Port = ushort.Parse( childNode.InnerText );
                        break;
                }
            }

            return config;
        }
    }
}
