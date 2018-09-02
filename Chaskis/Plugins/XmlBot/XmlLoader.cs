//
//          Copyright Seth Hendrick 2017.
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

namespace Chaskis.Plugins.XmlBot
{
    public static class XmlLoader
    {
        private const string rootXmlElementName = "xmlbotconfig";

        public static IList<IIrcHandler> LoadXmlBotConfig( string file, IIrcConfig ircConfig )
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
                if( messageNode.Name == "message" )
                {
                    string command = string.Empty;
                    string response = string.Empty;
                    int cooldown = 0;
                    ResponseOptions responseTo = ResponseOptions.ChannelAndPms;

                    foreach( XmlNode messageChild in messageNode.ChildNodes )
                    {
                        switch( messageChild.Name )
                        {
                            case "command":
                                command = messageChild.InnerText;
                                break;

                            case "response":
                                response = messageChild.InnerText;
                                break;

                            case "cooldown":
                                cooldown = int.Parse( messageChild.InnerText );
                                break;

                            case "respondto":
                                ResponseOptions option;
                                if( Enum.TryParse<ResponseOptions>( messageChild.InnerText, out option ) )
                                {
                                    responseTo = option;
                                }
                                else
                                {
                                    throw new FormatException(
                                        messageChild.InnerText + " Is not a valid repondto option."
                                    );
                                }
                                break;
                        }
                    }

                    MessageHandler handler = new MessageHandler(
                        command,
                        XmlBot.GetMessageHandler( response, ircConfig ),
                        cooldown,
                        responseTo
                    );

                    handlers.Add( handler );
                }
            }

            return handlers;
        }
    }
}
