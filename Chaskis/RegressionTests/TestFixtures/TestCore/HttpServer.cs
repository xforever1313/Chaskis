//
//          Copyright Seth Hendrick 2017-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using SethCS.Basic;

namespace Chaskis.RegressionTests.TestCore
{
    public class HttpServer : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly GenericLogger serverLog;

        private readonly Dictionary<string, string> fileMap;

        /// <summary>
        /// Reference to http listener.
        /// </summary>
        private HttpListener listener;

        private readonly Thread listeningThread;

        // ---------------- Constructor ----------------

        public HttpServer()
        {
            this.HangTime = 0;
            this.fileMap = new Dictionary<string, string>();
            this.serverLog = Logger.GetLogFromContext( "http_server" );
            this.listeningThread = new Thread( this.HandleRequest );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Whether or not we are listening.
        /// </summary>
        public bool IsListening { get; private set; }

        public int HangTime { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Starts the server on the given port.
        /// </summary>
        public void StartHttpServer( ushort port )
        {
            if( this.IsListening )
            {
                throw new InvalidOperationException( "Already Started!" );
            }

            this.serverLog.WriteLine( "Starting HTTP Server on port " + port + "..." );

            if( HttpListener.IsSupported == false )
            {
                throw new PlatformNotSupportedException(
                    "This platform does not support HTTP Listeners..."
                );
            }

            this.listener = new HttpListener();
            this.listener.Prefixes.Add( "http://127.0.0.1:" + port + "/" );
            this.listener.Start();
            this.listeningThread.Start();
            this.IsListening = true;

            this.serverLog.WriteLine( "Starting HTTP Server on port " + port  + "...Done!" );
        }

        public void MapFileToUrl( string filePath, string url )
        {
            this.fileMap[url] = filePath;
        }

        public void StopHttpServer()
        {
            if( this.IsListening )
            {
                this.serverLog.WriteLine( "Stopping HTTP Server..." );

                this.IsListening = false;
                this.listener.Stop();
                this.listeningThread.Join();

                this.serverLog.WriteLine( "Stopping HTTP Server...Done!" );
            }
        }

        public void SetHttpServerHangTimeTo( int hangTime )
        {
            this.HangTime = hangTime;
        }

        public void Dispose()
        {
            StopHttpServer();
        }

        private void HandleRequest()
        {
            // There are bunch of exceptions that can happen here.
            // 1.  Client closes connection.  This will cause our response's stream to close.
            //     We may see errors such as "The specified network name is no longer available"
            //     or "The I/O operation has been aborted because of either a thread exit or an application request".
            //     NEITHER or these should cause the program to crash.  Simply grab the next connection and move on.
            // 2.  ObjectDisposeExceptions can happen if the above happens as well.  We'll handle those when needed.

            try
            {
                while( this.IsListening )
                {
                    HttpListenerContext context = null;
                    try
                    {
                        // Apparently, HttpListener.Stop() may not be working correctly in dotnet 3
                        // when used with GetContext().
                        // The milestone for this fix is 5.0.  So until then, use GetContextAsync().Result.
                        //
                        // https://github.com/dotnet/runtime/issues/25497
                        context = listener.GetContextAsync().Result;
                    }
                    catch( HttpListenerException err )
                    {
                        // Error code 995 means GetContext got aborted (E.g. when shutting down).
                        // If that's the case, just start over.  The while loop will break out and
                        // the thread will exit cleanly.
                        if( err.ErrorCode == 995 )
                        {
                            this.serverLog.WriteLine( "Server got terminated, shutting down..." );
                            continue;
                        }
                        else
                        {
                            this.serverLog.WriteLine( "FATAL ERROR (" + err.ErrorCode + "): " + err.ToString() );
                            throw;
                        }
                    }

                    try
                    {
                        HttpListenerRequest request = context.Request;
                        HttpListenerResponse response = context.Response;

                        HttpStatusCode code = HttpStatusCode.BadRequest;
                        byte[] responseBuffer = null;

                        try
                        {
                            string url = request.RawUrl;
                            if( this.fileMap.ContainsKey( url ) == false )
                            {
                                throw new KeyNotFoundException( "URL " + url + " never set up." );
                            }

                            responseBuffer = File.ReadAllBytes( this.fileMap[url] );
                            code = HttpStatusCode.OK;

                            if( this.HangTime > 0 )
                            {
                                this.serverLog.WriteLine( "Hanging for " + this.HangTime + "ms..." );
                                Thread.Sleep( this.HangTime );
                                this.serverLog.WriteLine( "Hanging for " + this.HangTime + "ms... Done!" );
                            }
                        }
                        catch( Exception e )
                        {
                            responseBuffer = Encoding.UTF8.GetBytes( e.ToString() );
                            code = HttpStatusCode.InternalServerError;

                            this.serverLog.WriteLine( "**********" );
                            this.serverLog.WriteLine( "Caught Exception when determining response: " + e.Message );
                            this.serverLog.WriteLine( e.StackTrace );
                            this.serverLog.WriteLine( "**********" );
                        }
                        finally
                        {
                            try
                            {
                                if( responseBuffer != null )
                                {
                                    response.StatusCode = Convert.ToInt32( code );
                                    response.ContentLength64 = responseBuffer.Length;
                                    response.OutputStream.Write( responseBuffer, 0, responseBuffer.Length );
                                }
                            }
                            catch( Exception e )
                            {
                                this.serverLog.WriteLine( "**********" );
                                this.serverLog.WriteLine( "Caught Exception when writing response: " + e.Message );
                                this.serverLog.WriteLine( e.StackTrace );
                                this.serverLog.WriteLine( "**********" );
                            }
                            response.OutputStream.Close();
                        }

                        this.serverLog.WriteLine(
                            request.HttpMethod + " from: " + request.UserHostName + " " + request.UserHostAddress + " '" + request.UserAgent + "' " + request.RawUrl + " (" + response.StatusCode + ")"
                        );
                    } // End request/response try{}
                    catch( Exception err )
                    {
                        this.serverLog.WriteLine( "**********" );
                        this.serverLog.WriteLine( "Caught exception when handling resposne: " + err.Message );
                        this.serverLog.WriteLine( "This can happen for several expected reasons.  We're okay!" );
                        this.serverLog.WriteLine( err.StackTrace );
                        this.serverLog.WriteLine( "**********" );
                    }
                } // End while IsListening loop.
            }
            catch( Exception e )
            {
                this.serverLog.WriteLine( "**********" );
                this.serverLog.WriteLine( "FATAL Exception in HTTP Listener.  Aborting web server: " + e.Message );
                this.serverLog.WriteLine( e.StackTrace );
                this.serverLog.WriteLine( "**********" );
            }
        }
    }
}
