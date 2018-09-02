//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using Chaskis.Core;
using SethCS.Basic;

namespace Chaskis
{
    internal static class MainClass
    {
        public static int Main( string[] args )
        {
            try
            {
                ArgumentParser parser = new ArgumentParser( args, Chaskis.DefaultRootDirectory );

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

                StaticLogger.Log.OnWriteLine -= StaticLogger_OnWriteLine;
                StaticLogger.Log.OnWriteLine += StaticLogger_OnWriteLine;

                StaticLogger.Log.OnErrorWriteLine -= StaticLogger_OnErrorWriteLine;
                StaticLogger.Log.OnErrorWriteLine += StaticLogger_OnErrorWriteLine;

                if( parser.BootStrap )
                {
                    BootStrapper bootStrapper = new BootStrapper( parser.ChaskisRoot );
                    bootStrapper.DoBootStrap();
                }
                else
                { 
                    using( Chaskis chaskis = new Chaskis( parser.ChaskisRoot ) )
                    {
                        chaskis.InitState1_LoadIrcConfig();
                        bool pluginLoaded = chaskis.InitStage2_LoadPlugins();
                        if( ( pluginLoaded == false ) && parser.FailOnPluginFailure )
                        {
                            Console.WriteLine( "Fail on assembly was enable.  Terminating." );
                            return 2;
                        }
                        chaskis.InitStage3_DefaultHandlers();
                        chaskis.InitStage4_OpenConnection();

                        Console.WriteLine();
                        Console.Read();
                    }
                }
            }
            catch( Exception err )
            {
                Console.WriteLine( "FATAL ERROR:" + Environment.NewLine + err.Message );
                return -1;
            }
            finally
            {
                StaticLogger.Log.OnWriteLine -= StaticLogger_OnWriteLine;
                StaticLogger.Log.OnErrorWriteLine -= StaticLogger_OnErrorWriteLine;
            }

            return 0;
        }

        private static void StaticLogger_OnWriteLine( string str )
        {
            Console.Write( str );
            Console.Out.Flush();
        }

        private static void StaticLogger_OnErrorWriteLine( string str )
        {
            Console.Error.Write( str );
            Console.Error.Flush();
        }

        /// <summary>
        /// Prints the help message.
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine( "Chaskis IRC Bot Help:" );
            Console.WriteLine( "--help, -h, /?    --------  Prints this message and exits." );
            Console.WriteLine( "--version         --------  Prints this message and exits." );
            Console.WriteLine( "--chaskisroot=xxx  -------- The chaskis root, where to find the chaskis config files." );
            Console.WriteLine( "                            Default is in AppData." );
            Console.WriteLine( "                            If --bootstrap is passed in, location of where to bootstrap." );
            Console.WriteLine( "--bootstrap --------------  Puts default configuration in this area." );
            Console.WriteLine( "                            Default is in AppData if --chaskisroot is not specified." );
            Console.WriteLine( "--failOnBadPlugin=yes|no -  Whether or not to fail if a plugin load fails." );
            Console.WriteLine( "                            Defaulted to no." );
        }

        /// <summary>
        /// Prints the version info.
        /// </summary>
        private static void PrintVersion()
        {
            Console.WriteLine( "Chaskis IRC Bot Version:" );
            Console.WriteLine( Chaskis.VersionStr );
            Console.WriteLine( "Copyright (C) 2016-2018 Seth Hendrick" );
            Console.WriteLine();
            Console.WriteLine( "Released under the Boost Software License:" );
            Console.WriteLine( "http://www.boost.org/LICENSE_1_0.txt" );
        }
    }
}