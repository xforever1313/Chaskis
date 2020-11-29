//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SethCS.Basic;

namespace Chaskis.Cli
{
    /// <summary>
    /// Main method that can be shared between the CLI and the service.
    /// </summary>
    public abstract class ChaskisMain
    {
        public ChaskisMain()
        {
        }

        public async Task RunChaskis( IReadOnlyList<string> args, CancellationToken cancelToken )
        {
            try
            {
                StaticLogger.Log.OnWriteLine += LogInfo;
                StaticLogger.Log.OnErrorWriteLine += LogError;

                ArgumentParser parser = new ArgumentParser( args, Chaskis.DefaultRootDirectory );

                if( parser.IsValid == false )
                {
                    PrintHelp();
                    throw new ArgumentException( "Invalid Argument Detected." );
                }
                else if( parser.PrintHelp )
                {
                    PrintHelp();
                    return;
                }
                else if( parser.PrintVersion )
                {
                    PrintVersion();
                    return;
                }

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
                        chaskis.InitStage2_LoadPlugins();
                        chaskis.InitStage3_DefaultHandlers();
                        chaskis.InitStage4_OpenConnection();

                        Console.WriteLine();

                        try
                        {
                            await Task.Delay( -1, cancelToken );
                        }
                        catch( TaskCanceledException )
                        {
                            StaticLogger.Log.WriteLine( "Shutdown Requested" );
                        }
                    }
                }
            }
            finally
            {
                StaticLogger.Log.OnWriteLine -= LogInfo;
                StaticLogger.Log.OnErrorWriteLine -= LogError;
            }
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
        }

        /// <summary>
        /// Prints the version info.
        /// </summary>
        private static void PrintVersion()
        {
            Console.WriteLine( "Chaskis IRC Bot Version:" );
            Console.WriteLine( Chaskis.VersionStr );
            Console.WriteLine( "Copyright (C) 2016-2020 Seth Hendrick" );
            Console.WriteLine();
            Console.WriteLine( "Released under the Boost Software License:" );
            Console.WriteLine( "http://www.boost.org/LICENSE_1_0.txt" );
        }

        public abstract void LogInfo( string str );

        public abstract void LogError( string str );
    }
}
