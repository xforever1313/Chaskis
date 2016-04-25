
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using GenericIrcBot;
using System.IO;

namespace TestBot
{
    class MainClass
    {
        private const string nick = "SethTestBot";

        private static readonly List<Tuple<string, string>> pluginList = new List<Tuple<string, string>> {
            new Tuple<string, string>( Path.Combine( "..", "..", "..", "WelcomeBotPlugin", "bin", "Debug", "WelcomeBotPlugin.dll" ), "WelcomeBotPlugin.WelcomeBot" )
        };

        public static void Main( string[] args )
        {
            IrcConfig config = new IrcConfig();
            config.Nick = nick;
            config.Server = "irc.freenode.net";
            config.Channel = "#testseth";
            config.RealName = "Seths Test Bot";
            config.UserName = "SethTestBot";

            // Load Plugins.
            List<IIrcHandler> configs = new List<IIrcHandler>();

            {
                PluginManager manager = new PluginManager();

                foreach( Tuple<string, string> pluginInfo in pluginList )
                {
                    manager.LoadAssembly( Path.GetFullPath( pluginInfo.Item1 ), pluginInfo.Item2 );
                    configs.AddRange( manager.Handlers );
                }
            }

            // Must always check for pings.
            configs.Add( new PingHandler() );

            using( IrcBot bot = new IrcBot( config, configs ) )
            {
                bot.Start();
                ConsoleKeyInfo key = Console.ReadKey();
            }
        }
    }
}
