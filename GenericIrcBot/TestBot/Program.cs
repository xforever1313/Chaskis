
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using GenericIrcBot;
using System.Collections.Generic;

namespace TestBot
{
    class MainClass
    {
        private const string nick = "SethTestBot";

        public static void Main( string[] args )
        {
            IrcConfig config = new IrcConfig();
            config.Nick = nick;
            config.Server = "irc.freenode.net";
            config.Channel = "#testseth";
            config.RealName = "Seths Test Bot";
            config.UserName = "SethTestBot";

            // Generate line configs.

            List<IIrcHandler> configs = new List<IIrcHandler>();

            IIrcHandler helpLineConfig = new MessageHandler(
                                             @"!bot\s+help",
                                             HandleHelpCommand,
                                             30,
                                             ResponseOptions.RespondOnlyToChannel
                                         );

            configs.Add( helpLineConfig );

            helpLineConfig = new MessageHandler(
                @"!?bot\s+help",
                HandleHelpCommand,
                0,
                ResponseOptions.RespondOnlyToPMs
            );

            configs.Add( helpLineConfig );
           
            helpLineConfig = new JoinHandler( JoinMessage );

            configs.Add( helpLineConfig );

            helpLineConfig = new PartHandler( PartMessage );
            configs.Add( helpLineConfig );

            configs.Add( new PingHandler() );

            using( IrcBot bot = new IrcBot( config, configs ) )
            {
                bot.Start();
                ConsoleKeyInfo key = Console.ReadKey();
            }
        }

        private static void HandleHelpCommand( IIrcWriter writer, IrcResponse response )
        {
            if( response.Channel == nick )
            {
                writer.SendMessageToUser( "This is a bot!", response.RemoteUser );
            }
            else
            {
                writer.SendCommand( "This is a bot!" );
            }
        }

        private static void JoinMessage( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessageToUser( "Greetings!  Welcome to the channel!", response.RemoteUser );
            writer.SendCommand( response.RemoteUser + " has joined " + response.Channel );
        }

        private static void PartMessage( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessageToUser( "Thanks for visiting the channel!  Please come back soon!", response.RemoteUser );
            writer.SendCommand( response.RemoteUser + " has left " + response.Channel );
        }
    }
}
