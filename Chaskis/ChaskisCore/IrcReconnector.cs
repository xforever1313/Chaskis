//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;
using System.Threading;
using SethCS.Basic;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    /// <summary>
    /// This class handles reconnecting the IRC connection.
    /// </summary>
    /// <remarks>
    /// How this bascially works is there is a thread whose job it is to start
    /// the watchdog timer, send a test action, which tests the connection.
    /// If that action fails after the specfied timeout, we try to reconnect.
    /// If we Reset the reconnector, we queue the test action again and try again until
    /// we fail.
    /// </remarks>
    public class IrcReconnector : IDisposable
    {
        // ---------------- Events ----------------

        /// <summary>
        /// Event that gets fired if this class needs to log a message.
        /// </summary>
        public event Action<string> OnMessage;

        /// <summary>
        /// Event that gets fired if this class needs to log an error.
        /// </summary>
        public event Action<string> OnError;

        // ---------------- Fields ----------------

        private readonly int watchdogTimeout;

        private readonly Action testAction;
        private readonly Action reconnectAction;

        private readonly WatchdogTimer watchdog;

        private readonly EventExecutor reconnectionThread;
        private readonly ManualResetEvent testTimer;

        private bool isStarted;
        private bool isDisposed;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="testAction">The action to take to test the connection.</param>
        /// <param name="reconnectAction">
        /// The action to take to recoonect.  DO NOT let this action return until we reconnect successfully.
        /// </param>
        /// <param name="watchdogTimeout">What our watchdog timeout should be in milliseconds.</param>
        public IrcReconnector( Action testAction, Action reconnectAction, int watchdogTimeout )
        {
            ArgumentChecker.IsNotNull( testAction, nameof( testAction ) );
            ArgumentChecker.IsNotNull( reconnectAction, nameof( reconnectAction ) );

            this.watchdog = new WatchdogTimer( watchdogTimeout, "IRC Connection" );

            this.watchdog.OnStarted += this.Watchdog_OnStarted;
            this.watchdog.OnReset += this.Watchdog_OnReset;
            this.watchdog.OnStopped += this.Watchdog_OnStopped;
            this.watchdog.OnTimeoutExpired += this.Watchdog_OnTimeoutExpired;
            this.watchdog.OnTimeoutExpiredError += this.Watchdog_OnTimeoutExpiredError;

            this.testAction = testAction;
            this.reconnectAction = reconnectAction;

            this.reconnectionThread = new EventExecutor();
            this.reconnectionThread.OnError += this.ReconnectionThread_OnError;

            this.isStarted = false;
            this.isDisposed = false;

            this.testTimer = new ManualResetEvent( false );

            this.watchdogTimeout = watchdogTimeout;
        }

        // ---------------- Properties ----------------

        // ---------------- Functions ----------------

        /// <summary>
        /// Starts the watchdog timer.
        /// </summary>
        public void Start()
        {
            this.DisposeCheck();

            if( this.isStarted )
            {
                throw new InvalidOperationException( nameof( IrcReconnector ) + " has already been started" );
            }

            this.reconnectionThread.Start();
            this.isStarted = true;
            this.reconnectionThread.AddEvent( this.TimerEvent );
        }

        /// <summary>
        /// Resets the watchdog timer.
        /// </summary>
        public void ResetWatchdog()
        {
            this.DisposeCheck();

            if( this.watchdog.IsStarted )
            {
                try
                {
                    this.watchdog.Stop();
                }
                catch( InvalidOperationException )
                {
                }
                finally
                {
                    this.reconnectionThread.AddEvent( this.TimerEvent );
                }
            }
        }

        public void Dispose()
        {
            if( this.isDisposed )
            {
                return;
            }

            try
            {
                // Order is important here!

                // First, unsubscribe the event.  This will prevent
                // the watchdog from firing any events if it just so happens to 
                // timeout.
                this.watchdog.OnTimeoutExpired -= this.Watchdog_OnTimeoutExpired;
                this.watchdog.OnStarted -= this.Watchdog_OnStarted;
                this.watchdog.OnReset -= this.Watchdog_OnReset;
                this.watchdog.OnStopped -= this.Watchdog_OnStopped;
                this.watchdog.OnTimeoutExpiredError -= this.Watchdog_OnTimeoutExpiredError;

                // Set the testTimerEvent.  This means that any TimerEvents on the event queue
                // will exit right away.
                this.testTimer.Set();

                // Kill off our watchdog.
                this.watchdog.Dispose();

                // Kill our event queue.
                this.reconnectionThread.OnError -= this.ReconnectionThread_OnError;
                this.reconnectionThread.Dispose();

                this.testTimer.Dispose();
            }
            finally
            {
                this.isDisposed = true;
            }
        }

        private void Watchdog_OnStarted()
        {
            this.OnMessage?.Invoke( "Watchdog Timer Started." );
        }

        private void Watchdog_OnReset()
        {
            this.OnMessage?.Invoke( "Watchdog Timer Reset." );
        }

        private void Watchdog_OnStopped()
        {
            this.OnMessage?.Invoke( "Watchdog Timer Stopped." );
        }

        private void Watchdog_OnTimeoutExpired()
        {
            this.OnError?.Invoke(
                "Watch Dog has failed to receive a PONG within " + this.watchdogTimeout + "ms, attempting reconnect"
            );

            this.watchdog.Stop();

            this.reconnectionThread.AddEvent( this.reconnectAction );
            this.reconnectionThread.AddEvent( this.TimerEvent );
        }

        private void Watchdog_OnTimeoutExpiredError( Exception err )
        {
            StringBuilder errorMessage = new StringBuilder();

            errorMessage.AppendLine( "***************" );
            errorMessage.AppendLine( "Caught Exception in Watchdog Timer Event:" );
            errorMessage.AppendLine( err.Message );
            errorMessage.AppendLine( err.StackTrace );
            errorMessage.AppendLine( "***************" );

            this.OnError?.Invoke( errorMessage.ToString() );
        }

        private void ReconnectionThread_OnError( Exception err )
        {
            StringBuilder errorMessage = new StringBuilder();

            errorMessage.AppendLine( "***************" );
            errorMessage.AppendLine( "Caught Exception in Irc Connection Event Executor:" );
            errorMessage.AppendLine( err.Message );
            errorMessage.AppendLine( err.StackTrace );
            errorMessage.AppendLine( "***************" );

            this.OnError?.Invoke( errorMessage.ToString() );
        }

        private void DisposeCheck()
        {
            if( this.isDisposed )
            {
                throw new ObjectDisposedException( nameof( IrcReconnector ) + " has been disposed already." );
            }
        }

        private void TimerEvent()
        {
            bool aborting = this.testTimer.WaitOne( this.watchdogTimeout );
            if( aborting == false )
            {
                // Start the watchdog before the test action.
                // We don't want a race condition where we do the test action,
                // and the action that resets the watchdog happens before
                // we start the watchdog.  If we start it prior, we won't
                // have this issue.
                this.watchdog.Start();
                this.testAction?.Invoke();
            }
            // Otherwise, this class is being disposed.  Do nothing but return.
        }
    }
}
