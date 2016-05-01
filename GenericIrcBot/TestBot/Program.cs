
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using GenericIrcBot;
using System.IO;

namespace Chaskis
{
    class MainClass
    {
        public static int Main( string[] args )
        {
            try
            {
                string rootDir = Path.Combine(
                                     Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
                                     "Chaskis"
                                 );

                ArgumentParser parser = new ArgumentParser( args, rootDir );

                if( parser.IsValid == false )
                {
                    Console.WriteLine( "Invalid Argument Detected." );
                    PrintHelp();
                    return 1;
                }
                else if( parser.PrintHelp )
                {
                    PrintHelp();
                    return 0;
                }
                else if( parser.PrintVersion )
                {
                    PrintVersion();
                    return 0;
                }

                IIrcConfig ircConfig = XmlLoader.ParseIrcConfig( parser.IrcConfigLocation );

                List<IIrcHandler> configs = new List<IIrcHandler>();

                // Load Plugins.
                {
                    IList<AssemblyConfig> pluginList = XmlLoader.ParsePluginConfig( parser.IrcPluginConfigLocation );
                    if( pluginList.Count == 0 )
                    {
                        Console.WriteLine( "WARNING: No plugins loaded." );
                    }
                    PluginManager manager = new PluginManager();

                    foreach( AssemblyConfig pluginInfo in pluginList )
                    {
                        bool success = manager.LoadAssembly( Path.GetFullPath( pluginInfo.AssemblyPath ), pluginInfo.ClassName );
                        if( ( success == false ) && parser.FailOnPluginFailure )
                        {
                            Console.WriteLine( "Fail on assembly was enable.  Terminating." );
                            return 2;
                        }
                        else if( success )
                        {
                            Console.WriteLine( "Successfully loaded plugin " + pluginInfo.ClassName );
                        }
                    }

                    configs.AddRange( manager.Handlers );
                }

                // Must always check for pings.
                configs.Add( new PingHandler() );

                Console.WriteLine();
                using( IrcBot bot = new IrcBot( ircConfig, configs ) )
                {
                    bot.Start();
                    ConsoleKeyInfo key = Console.ReadKey();
                }
            }
            catch( Exception err )
            {
                Console.WriteLine( "FATAL ERROR:" + Environment.NewLine + err.Message );
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Prints the help message.
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine( "Chaskis IRC Bot Help:" );
            Console.WriteLine( "--help, -h, /?    --------  Prints this message and exits." );
            Console.WriteLine( "--version         --------  Prints this message and exits." );
            Console.WriteLine( "--configPath=xxx  --------  The IRC config xml file to use." );
            Console.WriteLine( "                            Default is in AppData." );
            Console.WriteLine( "--pluginConfigPath=xxx ---  The plugin config xml file to use." );
            Console.WriteLine( "                            Default is in AppData." );
            Console.WriteLine( "--failOnBadPlugin=yes|no -  Whether or not to fail if a plugin load fails." );
            Console.WriteLine( "                            Defaulted to no." );
        }

        /// <summary>
        /// Prints the version info.
        /// </summary>
        private static void PrintVersion()
        {
            Console.WriteLine( "Chaskis IRC Bot Version:" );
            Console.WriteLine( IrcBot.VersionString );
            Console.WriteLine( "Copyright (C) 2016 Seth Hendrick" );
            Console.WriteLine();
            Console.WriteLine( "Released under the Boost Software License:" );
            Console.WriteLine( "http://www.boost.org/LICENSE_1_0.txt" );
        }
    }
}
