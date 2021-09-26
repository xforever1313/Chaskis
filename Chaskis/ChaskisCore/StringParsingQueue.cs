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
using System.Threading.Channels;
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
    public class StringParsingQueue : IDisposable, INonDisposableStringParsingQueue
    {
        // ---------------- Fields ----------------

        private readonly Channel<Action> channel;

        private Task runner;
        private CancellationTokenSource cancelToken;

        private bool inited;

        private bool isDisposed;

        private IReadOnlyDictionary<string, IHandlerConfig> ircHandlers;

        // ---------------- Constructor ----------------

        public StringParsingQueue()
        {
            this.inited = false;
            this.isDisposed = false;
            this.channel = Channel.CreateUnbounded<Action>();
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
                errorMessage.WriteLine( e.ToString() );
                errorMessage.WriteLine( "***************" );

                StaticLogger.Log.ErrorWriteLine( errorMessage.ToString() );
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
                this.cancelToken = new CancellationTokenSource();
                this.runner = Task.Run( ThreadEntry );
                this.inited = true;
            }
            else
            {
                throw new InvalidOperationException( "Already started" );
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
            if( this.channel.Writer.TryWrite( action ) == false )
            {
                StaticLogger.Log.ErrorWriteLine( "Could not enqueue action onto channel" );
            }
        }

        protected virtual void Dispose( bool isDisposing )
        {
            if( this.isDisposed == false )
            {
                if( inited )
                {
                    if( isDisposing )
                    {
                        // If we are disposing, gracefully wait for all tasks to complete.
                        this.channel.Writer.Complete();
                        Task.WaitAll( this.runner );
                        this.cancelToken.Dispose();
                    }
                    else
                    {
                        // Otherwise, if we are being GC'ed just force everything to stop.
                        this.cancelToken.Cancel();
                        this.cancelToken.Dispose();
                    }
                }

                this.isDisposed = true;
            }
        }

        private void DisposeCheck()
        {
            if( this.isDisposed )
            {
                throw new ObjectDisposedException( nameof( StringParsingQueue ) );
            }
        }

        private async void ThreadEntry()
        {
            try
            {
                while( await this.channel.Reader.WaitToReadAsync( this.cancelToken.Token ) )
                {
                    try
                    {
                        Action action = await this.channel.Reader.ReadAsync( this.cancelToken.Token );
                        action();
                    }
                    catch( TaskCanceledException e )
                    {
                        PrintError( e );
                        return;
                    }
                    catch( Exception e )
                    {
                        PrintError( e );
                    }
                }
            }
            catch( TaskCanceledException e )
            {
                PrintError( e );
                return;
            }
            catch( Exception e )
            {
                StringWriter errorMessage = new StringWriter();

                errorMessage.WriteLine( "***************" );
                errorMessage.WriteLine( "FATAL Exception in " + Thread.CurrentThread.Name + ":" );
                errorMessage.WriteLine( e.ToString() );
                errorMessage.WriteLine( "***************" );

                StaticLogger.Log.ErrorWriteLine( errorMessage.ToString() );
            }
            finally
            {
                StaticLogger.Log.WriteLine( "Exiting processing thread" );
            }
        }

        private void PrintError( Exception err )
        {
            StringWriter errorMessage = new StringWriter();

            errorMessage.WriteLine( "***************" );
            errorMessage.WriteLine( "Caught Exception in " + Thread.CurrentThread.Name + ":" );
            errorMessage.WriteLine( err.ToString() );
            errorMessage.WriteLine( "***************" );

            StaticLogger.Log.ErrorWriteLine( errorMessage.ToString() );
        }
    }
}
