//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;
using System.Xml.Linq;
using SethCS.Extensions;

namespace Chaskis.Plugins.NewVersionNotifier
{
    public static class XmlLoader
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// What the root node of the xml config should be.
        /// </summary>
        private const string rootNodeName = "newversionnotifierconfig";

        // ---------------- Functions ----------------

        public static NewVersionNotifierConfig LoadConfigFromString( string xmlString )
        {
            using( StringReader reader = new StringReader( xmlString ) )
            {
                return LoadFromTextReader( reader );
            }
        }

        public static NewVersionNotifierConfig LoadConfigFromFile( string filePath )
        {
            if( File.Exists( filePath ) == false )
            {
                throw new FileNotFoundException( "Could not find " + nameof( NewVersionNotifierConfig ) + " file: " + filePath );
            }

            using( TextReader reader = File.OpenText( filePath ) )
            {
                return LoadFromTextReader( reader );
            }
        }

        private static NewVersionNotifierConfig LoadFromTextReader( TextReader reader )
        {
            XDocument doc = XDocument.Load( reader );

            if( rootNodeName.EqualsIgnoreCase( doc.Root.Name.LocalName ) == false )
            {
                throw new XmlException(
                    $"Root XML node should be named '{rootNodeName}'.  Got: '{doc.Root.Name.LocalName}'"
                );
            }

            NewVersionNotifierConfig config = new NewVersionNotifierConfig();

            foreach( XElement childNode in doc.Root.Elements() )
            {
                if( "message".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    config.Message = childNode.Value;
                }
                else if( "channels".EqualsIgnoreCase( childNode.Name.LocalName ) )
                {
                    foreach( XElement channelNode in childNode.Elements() )
                    {
                        if( "channel".EqualsIgnoreCase( channelNode.Name.LocalName ) )
                        {
                            config.ChannelsToSendTo.Add( channelNode.Value );
                        }
                    }
                }
            }

            return config;
        }
    }
}
