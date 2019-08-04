//
//          Copyright Seth Hendrick 2016-2019.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;
using System.Xml;
using Chaskis.Core;
using SethCS.Exceptions;

namespace Chaskis.Plugins.XmlBot
{
    public static class HandlerExtensions
    {
        public static void Deserialize(
            this MessageHandlerConfig msgConfig,
            XmlNode handlerNode,
            IIrcConfig ircConfig
        )
        {
            string response = DeserializeBase( msgConfig, handlerNode );

            // I can not for the life of me figure out how to make this generic between this and
            // action... maybe we can't?
            MessageHandlerAction action = delegate ( MessageHandlerArgs args )
            {
                HandleResponse( args, response, ircConfig );
            };

            msgConfig.LineAction = action;
        }

        public static void Deserialze(
            this ActionHandlerConfig actionConfig,
            XmlNode handlerNode,
            IIrcConfig ircConfig
        )
        {
            string response = DeserializeBase( actionConfig, handlerNode );

            // I can not for the life of me figure out how to make this generic between this
            // and message... maybe we can't?
            ActionHandlerAction action = delegate ( ActionHandlerArgs args )
            {
                HandleResponse( args, response, ircConfig );
            };

            actionConfig.LineAction = action;
        }

        public static string DeserializeBase<TChild, TLineActionType, TLineActionArgs>(
            this BasePrivateMessageConfig<TChild, TLineActionType, TLineActionArgs> config,
            XmlNode handlerNode
        )
            where TLineActionType : Delegate
            where TLineActionArgs : IPrivateMessageHandlerArgs
        {
            string response = null;
            foreach ( XmlNode messageChild in handlerNode.ChildNodes )
            {
                switch ( messageChild.Name )
                {
                    case "command":
                        config.LineRegex = messageChild.InnerText;
                        break;

                    case "response":
                        response = messageChild.InnerText;
                        break;

                    case "cooldown":
                        config.CoolDown = int.Parse( messageChild.InnerText );
                        break;

                    case "respondto":
                        ResponseOptions option;
                        if ( Enum.TryParse( messageChild.InnerText, out option ) )
                        {
                            config.ResponseOption = option;
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

            if ( string.IsNullOrWhiteSpace( response ) )
            {
                throw new ValidationException( "Missing: response tag" );
            }

            return response;
        }

        private static void HandleResponse( IPrivateMessageHandlerArgs args, string response, IIrcConfig ircConfig )
        {
            StringBuilder responseToSend = new StringBuilder(
                Parsing.LiquefyStringWithIrcConfig( response, args.User, ircConfig.Nick, args.Channel )
            );

            foreach ( string group in args.Regex.GetGroupNames() )
            {
                responseToSend.Replace(
                    "{%" + group + "%}",
                    args.Match.Groups[group].Value
                );
            }

            args.Writer.SendMessage(
                responseToSend.ToString(),
                args.Channel
            );
        }
    }
}
