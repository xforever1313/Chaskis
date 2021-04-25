//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chaskis.Core;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Plugins.XmlBot
{
    public static class XmlLoader
    {
        private const string rootXmlElementName = "xmlbotconfig";

        /// <summary>
        /// Converts an XML file to a list of handlers.
        /// </summary>
        /// <param name="rng">
        /// Leave this null to use the default RNG, otherwise pass this in if you want to use your own (e.g. with a different seed)
        /// </param>
        public static IList<IIrcHandler> LoadXmlBotConfig( string file, IReadOnlyIrcConfig ircConfig, Random rng = null )
        {
            ArgumentChecker.IsNotNull( ircConfig, nameof( ircConfig ) );

            if( File.Exists( file ) == false )
            {
                throw new FileNotFoundException( "Could not find XmlBotConfig file '" + file + '"' );
            }

            List<IIrcHandler> handlers = new List<IIrcHandler>();

            XmlDocument doc = new XmlDocument();
            doc.Load( file );

            XmlNode rootNode = doc.DocumentElement;
            if( rootNode.Name != rootXmlElementName )
            {
                throw new XmlException(
                    "Root XML node should be named \"" + rootXmlElementName + "\".  Got: " + rootNode.Name
                );
            }

            foreach( XmlNode messageNode in rootNode.ChildNodes )
            {
                IIrcHandler handler = null;

                if ( messageNode.Name.EqualsIgnoreCase( "message" ) )
                {
                    MessageHandlerConfig config = new MessageHandlerConfig();
                    config.Deserialize( messageNode, ircConfig, rng );
                    handler = new MessageHandler( config );
                }
                else if ( messageNode.Name.EqualsIgnoreCase( "action" ) )
                {
                    ActionHandlerConfig config = new ActionHandlerConfig();
                    config.Deserialze( messageNode, ircConfig, rng );
                    handler = new ActionHandler( config );
                }

                if ( handler != null )
                {
                    handlers.Add( handler );
                }
            }

            return handlers;
        }
    }
}
