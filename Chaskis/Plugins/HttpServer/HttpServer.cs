//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;
using System.Net;
using SethCS.Exceptions;
using System.Threading;
using SethCS.Basic;

namespace Chaskis.Plugins.HttpServer
{
    public class HttpServer : IDisposable
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Event that is fired if the HTTP Server has status to report.
        /// </summary>
        public event Action<string> OnStatus;

        /// <summary>
        /// Event that is triggered if the HTTP Server has an error status to report.
        /// </summary>
        public event Action<string> OnError;

        private bool isDisposed;

        /// <summary>
        /// Reference to http listener.
        /// </summary>
        private readonly HttpListener listener;

        private readonly HttpServerConfig config;

        private readonly Thread listenThread;

        private bool isListening;
        private readonly object isListeningLock;

        private readonly EventExecutor eventExecutor;

        // ---------------- Constructor ----------------

        public HttpServer( HttpServerConfig config )
        {
            this.isDisposed = false;
            this.isListening = false;
            this.isListeningLock = new object();

            ArgumentChecker.IsNotNull( config, nameof( config ) );
            config.Validate();
            this.config = config;

            if( HttpListener.IsSupported == false )
            {
                throw new PlatformNotSupportedException(
                    "This platform does not support HTTP Listeners..."
                );
            }

            this.listener = new HttpListener();
            this.listener.Prefixes.Add( "http://*:" + config.Port );

            this.listenThread = new Thread( this.HandleRequestThreadEntry );
            this.listenThread.Name = "Http Server Thread";

            this.eventExecutor = new EventExecutor( "HTTP Sever Executor" );
            this.eventExecutor.OnError += this.FireOnError;
        }

        ~HttpServer()
        {
            try
            {
                this.Dispose( false );
            }
            catch( Exception )
            {
                // Don't let the GC thread die.
            }
        }

        // ---------------- Properties ----------------

        public bool IsListening
        {
            get
            {
                lock( this.isListeningLock )
                {
                    return this.isListening;
                }
            }

            private set
            {
                lock( this.isListeningLock )
                {
                    this.isListening = value;
                }
            }
        }

        // ---------------- Functions ----------------

        public void Start()
        {
            this.IsListening = true;
            this.eventExecutor.Start();
            this.listenThread.Start();
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        private void HandleRequestThreadEntry()
        {
            try
            {
                while( this.IsListening )
                {
                    // First, get an HTTP Context.
                    HttpListenerContext context = null;
                    try
                    {
                        context = this.listener.GetContext();
                    }
                    catch( HttpListenerException err )
                    {
                        // Error code 995 means GetContext got aborted (E.g. when shutting down).
                        // If that's the case, just start over.  The while loop will break out and
                        // the thread will exit cleanly.
                        if( err.ErrorCode == 995 )
                        {
                            this.OnStatus?.Invoke( "HTTP Server got terminated (Error code 995), Terminating HTTPServer" );
                            continue;
                        }
                        else
                        {
                            // Otherwise, a fatal error happened, re-throw.
                            throw;
                        }
                    }

                    // Handle responses in a separate thread, so this thread can answer any other
                    // HTTP Requests.
                    this.eventExecutor.AddEvent( () => this.HandleResponse( context ) );
                }
            }
            catch( Exception e )
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine( "FATAL EXCEPTION in HttpServer.  Aborting web server, but IRC bot will continue to run." );
                builder.AppendLine( e.ToString() );

                this.OnStatus?.Invoke( builder.ToString() );
            }
            finally
            {
                this.OnStatus?.Invoke( "HTTP Server Stopped" );
            }
        }

        protected void Dispose( bool fromDispose )
        {
            if( this.isDisposed )
            {
                return;
            }

            try
            {
                this.IsListening = false;
                if( fromDispose )
                {
                    // Release managed code here.
                    this.listener?.Stop();
                    this.listener?.Close();
                }
                else
                {
                    // If we are not-managed, then the http server may not have disposed
                    // yet.  Tell the listen thread to interrupt in case
                    // the GC thread is disposing this class instead of the listener first,
                    // so we don't hang forever.
                    this.listenThread.Interrupt();
                }

                // Release unmanaged code or threads here
                this.listenThread.Join();
                
                // Event executor does not follow dispose pattern,
                // so it has unmanaged threads we need to dispose of.
                this.eventExecutor.Dispose();
            }
            finally
            {
                this.isDisposed = true;
            }
        }

        private void HandleResponse( HttpListenerContext context )
        {
            // There are bunch of exceptions that can happen here.
            // 1.  Client closes connection.  This will cause our response's stream to close.
            //     We may see errors such as "The specified network name is no longer available"
            //     or "The I/O operation has been aborted because of either a thread exit or an application request".
            //     NEITHER or these should cause the program to crash.  Simply grab the next connection and move on.
            // 2.  ObjectDisposeExceptions can happen if the above happens as well.  We'll handle those when needed.

            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // ---- Determine Response and Action to take ----
                try
                {
                }
                catch( Exception e )
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine( "Unexpected Exception while determining HTTP Response:" );
                    builder.AppendLine( e.ToString() );
                    this.OnError?.Invoke( builder.ToString() );
                }
                finally
                {
                    // ---- Write Response (error or okay) to the client. ----

                    try
                    {
                    }
                    catch( Exception e )
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.AppendLine( "Error when writing HTTP Response" );
                        builder.AppendLine( e.ToString() );

                        this.OnError?.Invoke( builder.ToString() );
                    }

                    response.Close(); // <- REMEMBER TO CLOSE THIS!
                }
            }
            catch( Exception e )
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine( "Caught Exception when handling HTTP Response: " );
                builder.AppendLine( e.Message );
                builder.AppendLine( "This can happen for several expected reasons, we're probably okay!" );
                builder.AppendLine( e.StackTrace );

                this.OnError?.Invoke( builder.ToString() );
            }
        }

        private void FireOnError( Exception err )
        {
            this.OnError?.Invoke( err.ToString() );
        }
    }
}
