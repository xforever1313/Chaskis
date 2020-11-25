//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading;
using SethCS.Basic;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public class IrcWatchdog : IDisposable
    {
        // ---------------- Fields ----------------

        private bool isDisposed;

        private bool keepGoing;
        private readonly object keepGoingLock;

        private readonly GenericLogger log;
        private readonly Thread thread;
        private readonly Action testAction;
        private readonly Action failureAction;

        private readonly AutoResetEvent watchdogResetEvent;
        private readonly ManualResetEventSlim threadStartedEvent;

        private readonly int timeout;

        // ---------------- Constructor ----------------

        public IrcWatchdog( GenericLogger log, Action testAction, Action failureAction, int watchdogTimeout )
        {
            ArgumentChecker.IsNotNull( log, nameof( log ) );
            ArgumentChecker.IsNotNull( testAction, nameof( testAction ) );
            ArgumentChecker.IsNotNull( failureAction, nameof( failureAction ) );

            this.isDisposed = false;
            this.log = log;
            this.thread = new Thread( this.ThreadEntry )
            {
                Name = "Watchdog"
            };

            this.keepGoing = false;
            this.keepGoingLock = new object();

            this.testAction = testAction;
            this.failureAction = failureAction;
            this.timeout = watchdogTimeout;

            this.Started = false;

            this.threadStartedEvent = new ManualResetEventSlim( false );
            this.watchdogResetEvent = new AutoResetEvent( false );
        }

        ~IrcWatchdog()
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

        public bool Started { get; private set; }

        private bool KeepGoing
        {
            get
            {
                lock ( this.keepGoingLock )
                {
                    return this.keepGoing;
                }
            }
            set
            {
                lock ( this.keepGoingLock )
                {
                    this.keepGoing = value;
                }
            }
        }

        // ---------------- Functions ----------------

        public void Start()
        {
            if ( this.Started == false )
            {
                this.KeepGoing = true;
                this.thread.Start();
                if ( this.threadStartedEvent.Wait( 5000 ) == false )
                {
                    throw new TimeoutException( "Watchdog thread did not start within 5 seconds" );
                }
                this.Started = true;
            }
            else
            {
                throw new InvalidOperationException( "Thread already started!" );
            }
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Resets the watchdog so it doesn't fail.
        /// </summary>
        public void ResetTimer()
        {
            this.watchdogResetEvent.Set();
        }

        protected void Dispose( bool fromDispose )
        {
            if ( this.isDisposed )
            {
                return;
            }

            if ( fromDispose )
            {
                this.KeepGoing = false;
                this.watchdogResetEvent.Set();
                if ( this.Started )
                {
                    this.thread.Join();
                }
                this.watchdogResetEvent.Dispose();
                this.threadStartedEvent.Dispose();
            }
            else
            {
                if ( this.Started )
                {
                    this.thread.Interrupt();
                    if ( this.thread.Join( 5000 ) == false )
                    {
                        this.thread.Abort();
                    }
                }
            }

            this.isDisposed = true;
        }

        private void ThreadEntry()
        {
            try
            {
                this.threadStartedEvent.Set();
                while ( this.KeepGoing )
                {
                    try
                    {
                        this.testAction();
                        if ( this.watchdogResetEvent.WaitOne( timeout ) == false )
                        {
                            // If we failed the test action, try one more time.
                            this.log.WarningWriteLine( "Watchdog failed first check, attempting second." );
                            this.testAction();
                            if ( this.watchdogResetEvent.WaitOne( timeout ) == false )
                            {
                                // We failed, need to trigger failure.
                                this.failureAction();
                            }
                        }

                        // After doing the test, wait a bit before trying again, but only if are not flagged to stop.
                        if ( this.KeepGoing )
                        {
                            this.watchdogResetEvent.WaitOne( timeout );
                        }
                    }
                    catch ( ThreadInterruptedException )
                    {
                        throw;
                    }
                    catch ( Exception e )
                    {
                        this.log.WarningWriteLine(
                            "Unexpected exception in watchdog thread: " + Environment.NewLine + e.ToString()
                        );
                    }
                }
            }
            catch ( Exception e )
            {
                this.log.ErrorWriteLine(
                    "WATCHDOG THREAD FATALLY EXITED" + Environment.NewLine + e
                );
            }
            finally
            {
                this.log.WriteLine( "WATCHDOG thread exited" );
            }
        }
    }
}
