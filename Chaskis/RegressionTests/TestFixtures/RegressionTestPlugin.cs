//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChaskisCore;
using SethCS.Basic;

namespace Chaskis.RegressionTests
{
    [ChaskisPlugin( "chaskistests" )]
    public class RegressionTestPlugin : IPlugin
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "1.0.0";

        private List<IIrcHandler> handlers;

        private GenericLogger log;

        private const string sleepPattern = @"!chaskistest\s+sleep\s+(?<timeMs>\d+)";
        private const string forceSleepPattern = @"!chaskistest\s+force\s+sleep\s+(?<timeMs>\d+)";
        private const string canaryPattern = @"!chaskistest\s+canary";
        private const string asyncAwaitThreadTestPattern = @"!chaskistest\s+asyncawait\s+threadname";
        private const string asyncAwaitExceptionTestPattern = @"!chaskistest\s+asyncawait\s+exception";

        // ---------------- Constructor ----------------

        public RegressionTestPlugin()
        {
            this.handlers = new List<IIrcHandler>();
        }

        // ---------------- Properties ----------------

        public string About
        {
            get
            {
                return "This plugin is used during Regression Tests";
            }
        }

        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/RegressionTests/TestFixtures";
            }
        }

        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        // ---------------- Functions ----------------

        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }

        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            writer.SendMessage(
                this.About,
                response.Channel
            );
        }

        public void Init( PluginInitor pluginInit )
        {
            this.log = pluginInit.Log;

            {
                MessageHandler sleepHandler = new MessageHandler(
                    sleepPattern,
                    this.HandleSleep
                );

                this.handlers.Add( sleepHandler );
            }

            {
                MessageHandler forceSleepHandler = new MessageHandler(
                    sleepPattern,
                    this.HandleForceSleep
                );

                this.handlers.Add( forceSleepHandler );
            }

            {
                MessageHandler canaryHandler = new MessageHandler(
                    canaryPattern,
                    this.HandleCanary
                );

                this.handlers.Add( canaryHandler );
            }

            {
                MessageHandler asyncHandler = new MessageHandler(
                    asyncAwaitThreadTestPattern,
                    this.DoAsyncTest
                );

                this.handlers.Add( asyncHandler );
            }

            {
                MessageHandler asyncHandler = new MessageHandler(
                    asyncAwaitExceptionTestPattern,
                    this.DoAsyncExceptionTest
                );

                this.handlers.Add( asyncHandler );
            }
        }

        public void Dispose()
        {
            this.log.WriteLine( "Disposing Plugin" );
        }

        // -------- Handlers --------

        /// <summary>
        /// Does a Thread.Sleep, but it can be aborted.
        /// </summary>
        private void HandleSleep( IIrcWriter writer, IrcResponse response )
        {
            int timeout = int.Parse( response.Match.Groups["timeMs"].Value );

            writer.SendMessage( "Sleeping for " + timeout + "ms...", response.Channel );

            try
            {
                Thread.Sleep( timeout );

                writer.SendMessage( "Starting Sleep for " + timeout + "ms...Done!", response.Channel );
            }
            catch( ThreadInterruptedException )
            {
                writer.SendMessage( "Caught ThreadInterruptedException during sleep.", response.Channel );
                throw;
            }
        }

        /// <summary>
        /// Does a Thread.Sleep in a finally block, so it will not be interrupted.
        /// </summary>
        private void HandleForceSleep( IIrcWriter writer, IrcResponse response )
        {
            try
            {

            }
            finally
            {
                this.HandleSleep( writer, response );
            }
        }

        private async void DoAsyncTest( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessage( "Starting from " + Thread.CurrentThread.Name, response.Channel );

            string bgThreadName = "NEVER SET!";
            await Task.Factory.StartNew( () => bgThreadName = Thread.CurrentThread.Name );

            writer.SendMessage( "Background Thread Name: " + bgThreadName + "END", response.Channel );
            writer.SendMessage( "Finishing from " + Thread.CurrentThread.Name, response.Channel );
        }

        private async void DoAsyncExceptionTest( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessage( "About to throw Exception" + Thread.CurrentThread.Name, response.Channel );

            try
            {
                await Task.Factory.StartNew(
                    () => { throw new Exception( "Throwing Exception From Background Thread" ); }
                );
            }
            catch( Exception e )
            {
                writer.SendMessage( "Caught Exception " + e.Message, response.Channel );
                throw;
            }
        }

        /// <summary>
        /// Used to ensure we are still alive during testing.
        /// </summary>
        private void HandleCanary( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessage(
                "Canary Alive!",
                response.Channel
            );
        }
    }
}
