//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chaskis.Cli
{
    public class CliMain : ChaskisMain
    {
        public override void LogError( string str )
        {
            Console.Error.Write( str );
            Console.Error.Flush();
        }

        public override void LogInfo( string str )
        {
            Console.Write( str );
            Console.Out.Flush();
        }
    }

    internal static class MainClass
    {
        public static int Main( string[] args )
        {
            try
            {
                using( CancellationTokenSource token = new CancellationTokenSource() )
                {
                    CliMain main = new CliMain();
                    Task task = main.RunChaskis( args, token.Token );

                    // In case we got arguments for help or version,
                    // only do this if our task is still doing things.
                    if( task.IsCompleted == false )
                    {
                        Console.WriteLine( "Press any key to quit" );
                        Console.Read();
                    }

                    token.Cancel();

                    task.Wait();

                    // No error if there were no exceptions.
                    return 0;
                }
            }
            catch( ArgumentException )
            {
                // Invalid CLI arguments, return 1.
                return 1;
            }
            catch( PluginLoadException )
            {
                // Plugin did not load, return 2.
                return 2;
            }
            catch( Exception err )
            {
                Console.WriteLine( "FATAL ERROR:" + Environment.NewLine + err.Message );
                return -1;
            }
        }
    }
}