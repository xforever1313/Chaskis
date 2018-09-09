//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chaskis.Core;
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

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            if( helpArgs.Length == 0 )
            {
                msgArgs.Writer.SendMessage(
                    "Got help request with no arguments.",
                    msgArgs.Channel
                );
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                foreach( string arg in helpArgs )
                {
                    builder.Append( arg + " " );
                }
                msgArgs.Writer.SendMessage(
                    "Got help request with these args: " + builder.ToString(),
                    msgArgs.Channel
                );
            }
        }

        public void Init( PluginInitor pluginInit )
        {
            this.log = pluginInit.Log;

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = sleepPattern,
                    LineAction = this.HandleSleep
                };

                MessageHandler sleepHandler = new MessageHandler(
                    msgConfig
                );

                this.handlers.Add( sleepHandler );
            }

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = forceSleepPattern,
                    LineAction = this.HandleForceSleep
                };

                MessageHandler forceSleepHandler = new MessageHandler(
                    msgConfig
                );

                this.handlers.Add( forceSleepHandler );
            }

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = canaryPattern,
                    LineAction = this.HandleCanary
                };

                MessageHandler canaryHandler = new MessageHandler(
                    msgConfig
                );

                this.handlers.Add( canaryHandler );
            }

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = asyncAwaitThreadTestPattern,
                    LineAction = this.DoAsyncTest
                };

                MessageHandler asyncHandler = new MessageHandler(
                    msgConfig
                );

                this.handlers.Add( asyncHandler );
            }

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = asyncAwaitExceptionTestPattern,
                    LineAction = this.DoAsyncExceptionTest
                };

                MessageHandler asyncHandler = new MessageHandler(
                    msgConfig
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
        private void HandleSleep( MessageHandlerArgs args )
        {
            int timeout = int.Parse( args.Match.Groups["timeMs"].Value );

            args.Writer.SendMessage( "Sleeping for " + timeout + "ms...", args.Channel );

            try
            {
                Thread.Sleep( timeout );

                args.Writer.SendMessage( "Starting Sleep for " + timeout + "ms...Done!", args.Channel );
            }
            catch( ThreadInterruptedException )
            {
                args.Writer.SendMessage( "Caught ThreadInterruptedException during sleep.", args.Channel );
                throw;
            }
        }

        /// <summary>
        /// Does a Thread.Sleep in a finally block, so it will not be interrupted.
        /// </summary>
        private void HandleForceSleep( MessageHandlerArgs args )
        {
            try
            {

            }
            finally
            {
                this.HandleSleep( args );
            }
        }

        private async void DoAsyncTest( MessageHandlerArgs args )
        {
            args.Writer.SendMessage( "Starting from " + Thread.CurrentThread.Name, args.Channel );

            string bgThreadName = "NEVER SET!";
            await Task.Factory.StartNew( () => bgThreadName = Thread.CurrentThread.Name );

            args.Writer.SendMessage( "Background Thread Name: " + bgThreadName + "END", args.Channel );
            args.Writer.SendMessage( "Finishing from " + Thread.CurrentThread.Name, args.Channel );
        }

        private async void DoAsyncExceptionTest( MessageHandlerArgs args )
        {
            args.Writer.SendMessage( "About to throw Exception" + Thread.CurrentThread.Name, args.Channel );

            try
            {
                await Task.Factory.StartNew(
                    () => { throw new Exception( "Throwing Exception From Background Thread" ); }
                );
            }
            catch( Exception e )
            {
                args.Writer.SendMessage( "Caught Exception " + e.Message, args.Channel );
                throw;
            }
        }

        /// <summary>
        /// Used to ensure we are still alive during testing.
        /// </summary>
        private void HandleCanary( MessageHandlerArgs args )
        {
            args.Writer.SendMessage(
                "Canary Alive!",
                args.Channel
            );
        }
    }
}
