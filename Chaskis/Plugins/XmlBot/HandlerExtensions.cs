//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Chaskis.Core;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Plugins.XmlBot
{
    public static class HandlerExtensions
    {
        // ---------------- Fields ----------------

        private static readonly Random random;

        // ---------------- Constructor ----------------

        static HandlerExtensions()
        {
            random = new Random();
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Converts an XML node to a config object.
        /// </summary>
        /// <param name="rng">
        /// Leave this null to use the default RNG, otherwise pass this in if you want to use your own (e.g. with a different seed)
        /// </param>
        public static void Deserialize(
            this MessageHandlerConfig msgConfig,
            XmlNode handlerNode,
            IIrcConfig ircConfig,
            Random rng = null
        )
        {
            IReadOnlyList<string> responses = DeserializeBase( msgConfig, handlerNode );

            // I can not for the life of me figure out how to make this generic between this and
            // action... maybe we can't?
            MessageHandlerAction action = delegate ( MessageHandlerArgs args )
            {
                HandleResponse( args, responses, ircConfig, rng );
            };

            msgConfig.LineAction = action;
        }

        /// <summary>
        /// Converts an XML node to a config object.
        /// </summary>
        /// <param name="rng">
        /// Leave this null to use the default RNG, otherwise pass this in if you want to use your own (e.g. with a different seed)
        /// </param>
        public static void Deserialze(
            this ActionHandlerConfig actionConfig,
            XmlNode handlerNode,
            IIrcConfig ircConfig,
            Random rng = null
        )
        {
            IReadOnlyList<string> responses = DeserializeBase( actionConfig, handlerNode );

            // I can not for the life of me figure out how to make this generic between this
            // and message... maybe we can't?
            ActionHandlerAction action = delegate ( ActionHandlerArgs args )
            {
                HandleResponse( args, responses, ircConfig, rng );
            };

            actionConfig.LineAction = action;
        }

        public static IReadOnlyList<string> DeserializeBase<TChild, TLineActionType, TLineActionArgs>(
            this BasePrivateMessageConfig<TChild, TLineActionType, TLineActionArgs> config,
            XmlNode handlerNode
        )
            where TLineActionType : Delegate
            where TLineActionArgs : IPrivateMessageHandlerArgs
        {
            List<string> responses = new List<string>();
            foreach ( XmlNode messageChild in handlerNode.ChildNodes )
            {
                switch ( messageChild.Name )
                {
                    case "command":
                        config.LineRegex = messageChild.InnerText;
                        break;

                    case "response":
                        responses.Add( messageChild.InnerText );
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

            if ( responses.IsEmpty() )
            {
                throw new ValidationException( "Missing: response tag" );
            }
            if ( responses.Exists( i => string.IsNullOrWhiteSpace( i ) ) )
            {
                throw new ValidationException( "Found a response that is null, empty, or whitespace." );
            }

            return responses;
        }

        private static void HandleResponse(
            IPrivateMessageHandlerArgs args,
            IReadOnlyList<string> responses,
            IIrcConfig ircConfig,
            Random rng
        )
        {
            int index;
            if ( responses.Count == 1 )
            {
                index = 0;
            }
            else
            {
                if ( rng == null )
                {
                    rng = random;
                }

                index = rng.Next( 0, responses.Count );
            }

            string response = responses[index];

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
