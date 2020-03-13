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
        private static CancellationTokenSource token;

        public static int Main( string[] args )
        {
            try
            {
                token = new CancellationTokenSource();
                Console.CancelKeyPress += Console_CancelKeyPress;

                CliMain main = new CliMain();
                Task task = main.RunChaskis( args, token.Token );
                task.Wait();

                // No error if there were no exceptions.
                return 0;
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
            finally
            {
                Console.CancelKeyPress -= Console_CancelKeyPress;
                token?.Dispose();
            }
        }

        private static void Console_CancelKeyPress( object sender, ConsoleCancelEventArgs e )
        {
            // If we are CTRL+C, we should stop the process,
            // but let things clean up first.  If we get CTRL+BREAK,
            // let the process be killed without cleaning up.
            if( e.SpecialKey == ConsoleSpecialKey.ControlC )
            {
                Console.WriteLine( "Got CTRL+C, gracefully cleaning up process." );
                e.Cancel = true;
                token.Cancel();
            }
        }
    }
}