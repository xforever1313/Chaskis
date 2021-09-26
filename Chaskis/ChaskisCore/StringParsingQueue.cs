//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SethCS.Basic;

namespace Chaskis.Core
{
    /// <summary>
    /// Interface to the StringParsingQueue that only allows
    /// adding events.
    /// 
    /// Useful for when you don't want someone disposing the queue by accident.
    /// </summary>
    public interface INonDisposableStringParsingQueue
    {
        /// <summary>
        /// Takes in the given handler args and runs the event handlers
        /// on the parsing thread.
        /// </summary>
        void ParseAndRunEvent( HandlerArgs args );

        /// <summary>
        /// Invokes the given action on the event queue thread.
        /// Does not block.
        /// </summary>
        void BeginInvoke( Action action );

        /// <summary>
        /// Wait for all CURRENT enqueued events to execute.
        /// </summary>
        void WaitForAllEventsToExecute();
    }

    /// <summary>
    /// This is the class that takes strings from a server,
    /// parses them, and calls the IRC Handlers.
    /// </summary>
    public sealed class StringParsingQueue : ChaskisActionChannel, INonDisposableStringParsingQueue
    {
        // ---------------- Fields ----------------

        private IReadOnlyDictionary<string, IHandlerConfig> ircHandlers;

        // ---------------- Constructor ----------------

        public StringParsingQueue() :
            base( nameof( StringParsingQueue ) )
        {
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Starts the parsing thread.
        /// </summary>
        public void Start( IReadOnlyDictionary<string, IHandlerConfig> ircHandlers )
        {
            ThrowIfStarted();
            this.ircHandlers = ircHandlers;
            Start();
        }

        /// <summary>
        /// Waits for all CURRENT events on the event queue
        /// to finish executing.
        /// </summary>
        public void WaitForAllEventsToExecute()
        {
            ManualResetEvent doneEvent = new ManualResetEvent( false );
            this.BeginInvoke( () => doneEvent.Set() );

            // Wait for our last event to execute before leaving.
            doneEvent.WaitOne();
        }

        /// <summary>
        /// Takes in the given handler args and runs the event handlers
        /// on the parsing thread.
        /// </summary>
        public void ParseAndRunEvent( HandlerArgs args )
        {
            foreach( KeyValuePair<string, IHandlerConfig> handlers in this.ircHandlers )
            {
                string pluginName = handlers.Key;
                IHandlerConfig innerHandlers = handlers.Value;

                // Need to clone because I am silly and forgot that our black-listed
                // channel list can't be set to the passed-in args since 
                // each plugin has its own black-listed channels.
                // Setting this in the passed-in args means we could have a race-condition
                // between accessing the list, and setting it.
                //
                // Luckily, HandlerArgs is small when it clones, its a simple memberwise clone.
                // It shouldn't be too big of a hit in terms of heap allocations.
                HandlerArgs innerArgs = args.Clone();
                innerArgs.BlackListedChannels = innerHandlers.BlackListedChannels;

                foreach( IIrcHandler handler in innerHandlers.Handlers )
                {
                    IIrcHandler innerHandler = handler;
                    void theAction()
                    {
                        try
                        {
                            innerHandler.HandleEvent( innerArgs );
                        }
                        catch( Exception e )
                        {
                            throw new EventHandlerException(
                                pluginName,
                                innerHandler,
                                args.Line,
                                e
                            );
                        }
                    }

                    this.BeginInvoke( theAction );
                }
            }
        }

        protected override void OnBadEnqueue( Action action )
        {
            StaticLogger.Log.ErrorWriteLine( "Could not enqueue action onto channel" );
        }

        protected override void OnError( Exception e )
        {
            this.PrintError( e );
        }

        protected override void OnTaskCancelled( TaskCanceledException e )
        {
            this.PrintError( e );
        }

        protected override void OnThreadExit()
        {
            StaticLogger.Log.WriteLine( $"Exiting {this.Name}" );
        }

        protected override void OnFatalError( Exception e )
        {
            StringWriter errorMessage = new StringWriter();

            errorMessage.WriteLine( "***************" );
            errorMessage.WriteLine( $"FATAL Exception in {this.Name}:" );
            errorMessage.WriteLine( e.ToString() );
            errorMessage.WriteLine( "***************" );

            StaticLogger.Log.ErrorWriteLine( errorMessage.ToString() );
        }

        private void PrintError( Exception err )
        {
            StringWriter errorMessage = new StringWriter();

            errorMessage.WriteLine( "***************" );
            errorMessage.WriteLine( $"Caught Exception in {this.Name}:" );
            errorMessage.WriteLine( err.ToString() );
            errorMessage.WriteLine( "***************" );

            StaticLogger.Log.ErrorWriteLine( errorMessage.ToString() );
        }
    }
}
