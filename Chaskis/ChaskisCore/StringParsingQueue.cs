//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SethCS.Basic;

namespace ChaskisCore
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
    public class StringParsingQueue : IDisposable, INonDisposableStringParsingQueue
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Event queue.
        /// </summary>
        private InterruptibleEventExecutor eventQueue;

        private bool inited;

        private bool isDisposed;

        /// <summary>
        /// The IRC handlers.
        /// </summary>
        private IReadOnlyDictionary<string, IHandlerConfig> ircHandlers;

        // ---------------- Constructor ----------------

        public StringParsingQueue()
        {
            this.eventQueue = new InterruptibleEventExecutor(
                15 * 1000, // Lets start with 15 seconds and see how that works.,
                nameof( StringParsingQueue ) + " Thread"
            );

            this.eventQueue.OnError += this.EventQueue_OnError;

            this.inited = false;
            this.isDisposed = false;
        }

        ~StringParsingQueue()
        {
            try
            {
                this.Dispose( false );
            }
            catch( Exception e )
            {
                StringWriter errorMessage = new StringWriter();

                errorMessage.WriteLine( "***************" );
                errorMessage.WriteLine( "Caught Exception in while finalizing String Parsing Queue Thread:" );
                errorMessage.WriteLine( e.Message );
                errorMessage.WriteLine( e.StackTrace );
                errorMessage.WriteLine( "***************" );
            }
        }

        // ---------------- Properties ----------------

        // ---------------- Functions ----------------

        /// <summary>
        /// Starts the parsing thread.
        /// </summary>
        public void Start( IReadOnlyDictionary<string, IHandlerConfig> ircHandlers )
        {
            DisposeCheck();

            if( this.inited == false )
            {
                this.ircHandlers = ircHandlers;
                this.eventQueue.Start();
                this.inited = true;
            }
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
        /// Disposes this class.
        /// </summary>
        public void Dispose()
        {
            DisposeCheck();

            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Takes in the given handler args and runs the event handlers
        /// on the parsing thread.
        /// </summary>
        public void ParseAndRunEvent( HandlerArgs args )
        {
            HandlerArgs innerArgs = args;

            // Right now, each line is its own event... should we change this to each handler being
            // its own event?
            this.BeginInvoke(
                delegate ()
                {
                    foreach( KeyValuePair<string, IHandlerConfig> handlers in this.ircHandlers )
                    {
                        innerArgs.BlackListedChannels = handlers.Value.BlackListedChannels;
                        foreach( IIrcHandler handler in handlers.Value.Handlers )
                        {
                            handler.HandleEvent( innerArgs );
                        }
                    }
                }
            );
        }

        /// <summary>
        /// Invokes the given action on the event queue thread.
        /// Does not block.
        /// </summary>
        public void BeginInvoke( Action action )
        {
            if( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            this.eventQueue.AddEvent( action );
        }

        protected virtual void Dispose( bool isDisposing )
        {
            if( this.isDisposed == false )
            {
                if( isDisposing )
                {
                    // Free managed objects here.
                    this.eventQueue.OnError -= this.EventQueue_OnError;
                }

                // Free unmanaged objects or threads here.
                if( this.inited )
                {
                    this.eventQueue.Dispose();
                }

                this.isDisposed = true;
            }
        }

        private void EventQueue_OnError( Exception err )
        {
            StringWriter errorMessage = new StringWriter();

            errorMessage.WriteLine( "***************" );
            errorMessage.WriteLine( "Caught Exception in " + Thread.CurrentThread.Name + ":" );
            errorMessage.WriteLine( err.Message );
            errorMessage.WriteLine( err.StackTrace );
            errorMessage.WriteLine( "***************" );

            StaticLogger.Log.ErrorWriteLine( errorMessage.ToString() );
        }

        private void DisposeCheck()
        {
            if( this.isDisposed )
            {
                throw new ObjectDisposedException( nameof( StringParsingQueue ) );
            }
        }
    }
}
