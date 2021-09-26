//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public class ChaskisActionChannel : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly Channel<Action> channel;

        private Task runner;
        private CancellationTokenSource cancelToken;

        private bool isStarted;
        private bool isDisposed;

        // ---------------- Constructor ----------------

        protected ChaskisActionChannel( string name )
        {
            this.channel = Channel.CreateUnbounded<Action>();
            this.isStarted = false;
            this.isDisposed = false;
            this.Name = name;
        }

        ~ChaskisActionChannel()
        {
            try
            {
                this.Dispose( false );
            }
            catch
            {
                // Don't let GC thread die.
            }
        }

        // ---------------- Properties ----------------

        public string Name { get; private set; }

        // ---------------- Functions ----------------

        public void BeginInvoke( Action action )
        {
            ArgumentChecker.IsNotNull( action, nameof( action ) );
            DisposeCheck();

            if( this.channel.Writer.TryWrite( action ) == false )
            {
                OnBadEnqueue( action );
            }
        }

        protected void Start()
        {
            DisposeCheck();
            ThrowIfStarted();

            this.cancelToken = new CancellationTokenSource();
            this.runner = Task.Factory.StartNew(
                ThreadEntry,
                TaskCreationOptions.LongRunning
            );
            this.isStarted = true;
        }

        /// <summary>
        /// Event gets called when we can't enqueue an action.
        /// </summary>
        protected virtual void OnBadEnqueue( Action action )
        {
        }

        /// <summary>
        /// Event gets called when the background thread picks
        /// up on a <see cref="TaskCanceledException"/>.
        /// The thread will exit after invoking this method.
        /// </summary>
        protected virtual void OnTaskCancelled( TaskCanceledException e )
        {
        }

        /// <summary>
        /// Event gets called when some kind of error happens in the background thread.
        /// </summary>
        protected virtual void OnError( Exception e )
        {
        }

        /// <summary>
        /// Event gets called when some kind of fatal error happens in the background thread.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFatalError( Exception e )
        {
        }

        /// <summary>
        /// Event gets called when our background thread exits.
        /// </summary>
        protected virtual void OnThreadExit()
        {
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool fromDispose )
        {
            if( this.isDisposed == false )
            {
                if( this.isStarted )
                {
                    if( fromDispose )
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

        protected void ThrowIfStarted()
        {
            if( this.isStarted )
            {
                throw new InvalidOperationException( "Already Started" );
            }
        }

        protected void DisposeCheck()
        {
            if( this.isDisposed )
            {
                throw new ObjectDisposedException( this.GetType().Name );
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
                        OnTaskCancelled( e );
                        return;
                    }
                    catch( Exception e )
                    {
                        OnError( e );
                    }
                }
            }
            catch( TaskCanceledException e )
            {
                OnTaskCancelled( e );
                return;
            }
            catch( Exception e )
            {
                OnFatalError( e );
            }
            finally
            {
                OnThreadExit();
            }
        }
    }
}
