//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using NetRunner.ExternalLibrary;
using SethCS.Basic;

namespace Chaskis.RegressionTests
{
    /// <summary>
    /// This class emulates an IRC server that Chaskis will talk to.
    /// </summary>
    public class IrcServer : BaseTestContainer, IDisposable
    {
        // ---------------- Fields ----------------

        private bool isDisposed;

        /// <summary>
        /// Commands sent out from server.
        /// </summary>
        private GenericLogger outCommands;

        /// <summary>
        /// Commands sent in to server.
        /// </summary>
        private GenericLogger inCommands;

        /// <summary>
        /// Log for the server.
        /// </summary>
        private GenericLogger serverLog;

        private StringBuffer buffer;

        /// <summary>
        /// The thread that reads.
        /// </summary>
        private Thread readerThread;

        /// <summary>
        /// Whether or not to keep reading.
        /// </summary>
        private bool keepReading;

        /// <summary>
        /// Used to mutex-protect keepReading.
        /// </summary>
        private object keepReadingObject;

        private bool respondToPings;

        private object respondToPingsObject;

        private ManualResetEvent connectedEvent;

        // -------- TCP Client --------
        private TcpListener connection;
        private StreamWriter connectionWriter;
        private StreamReader connectionReader;

        // ---------------- Constructor ----------------

        public IrcServer()
        {
            this.isDisposed = false;

            this.outCommands = Logger.GetLogFromContext( "server_out" );
            this.inCommands = Logger.GetLogFromContext( "server_in" );
            this.serverLog = Logger.GetLogFromContext( "server_log" );

            this.buffer = new StringBuffer();
            this.keepReadingObject = new object();

            this.respondToPings = true;
            this.respondToPingsObject = new object();
        }

        ~IrcServer()
        {
            try
            {
                Dispose( false );
            }
            catch( Exception )
            {
                // Swallow Exception... don't want GC thread to crash.
            }
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Whether or not to keep reading.
        /// </summary>
        private bool IsConnected
        {
            get
            {
                lock( this.keepReadingObject )
                {
                    return this.keepReading;
                }
            }

            set
            {
                lock( this.keepReadingObject )
                {
                    this.keepReading = value;
                }
            }
        }

        /// <summary>
        /// Whether or not to keep reading.
        /// </summary>
        private bool RespondToPings
        {
            get
            {
                lock( this.respondToPingsObject )
                {
                    return this.respondToPings;
                }
            }

            set
            {
                lock( this.respondToPingsObject )
                {
                    this.respondToPings = value;
                }
            }
        }

        // ---------------- Functions ----------------

        // -------- Start / Stop --------

        /// <summary>
        /// Starts the server on the given port.
        /// </summary>
        public bool StartServer( short port )
        {
            if( this.connection != null )
            {
                throw new InvalidOperationException( "Connection already made!" );
            }

            this.connectedEvent = new ManualResetEvent( false );

            this.connection = new TcpListener(
                new IPEndPoint( IPAddress.Loopback, port )
            );
            this.connection.Start();

            this.IsConnected = true;
            this.readerThread = new Thread( this.ReadFromSocket );
            this.readerThread.Start();

            this.serverLog.WriteLine( "Listening for connection on port " + port );

            return true;
        }

        /// <summary>
        /// Waits until a client connects to the server.
        /// </summary>
        public bool WaitForConnection()
        {
            return this.WaitForConnection( 30 * 1000 );
        }

        /// <summary>
        /// Waits until a client connects to the server.
        /// </summary>
        public bool WaitForConnection( int timeout )
        {
            this.serverLog.WriteLine( "Waiting for connection on port..." );
            bool success = this.connectedEvent.WaitOne( timeout );
            this.serverLog.WriteLine(
                "Waiting for connection...{0}",
                success ? "Done!" : "Fail!"
            );

            return success;
        }

        /// <summary>
        /// Stops the server.
        /// Calls <see cref="Dispose()"/> under the hood.
        /// </summary>
        public bool StopServer()
        {
            if( this.connection == null )
            {
                throw new InvalidOperationException( "No Connection Exists!" );
            }

            this.IsConnected = false;
            this.connection?.Stop();
            this.connection = null;

            this.readerThread?.Join();
            return true;
        }

        /// <summary>
        /// Disposes the server.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        // -------- Ping / Pong --------

        /// <summary>
        /// Do we repond to pings from the client?
        /// Defaulted to true.
        /// </summary>
        public bool SetPingResponse( bool enable )
        {
            this.RespondToPings = enable;
            return true;
        }

        // -------- Send / Wait Commands --------

        /// <summary>
        /// Sends a message from a specific channel.  Waits for a specific response from chaskis.
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="channel">The channel to send to</param>
        /// <param name="nick">The name of the user that send the message.</param>
        /// <param name="waitMsg">The message to wait for as a regex.</param>
        public bool SendMessageToChannelAsWaitMsg( string msg, string channel, string nick, string waitMsg )
        {
            return this.SendMessageToChannelAsWaitMsgWithTimeout( msg, channel, nick, waitMsg, TestConstants.DefaultTimeout );
        }

        /// <summary>
        /// Sends a message from a specific channel.
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="channel">The channel to send to</param>
        /// <param name="nick">The name of the user that send the message.</param>
        /// <param name="waitMsg">The message to wait for as a regex.</param>
        /// <param name="timeout">How long to wait before giving up.</param>
        public bool SendMessageToChannelAsWaitMsgWithTimeout( string msg, string channel, string nick, string waitMsg, int timeout )
        {
            bool success = this.SendMessageToChannelAs( msg, channel, nick );
            success &= this.WaitForMessageOnChannelWithTimeout( waitMsg, channel, timeout );

            return success;
        }

        /// <summary>
        /// Sends a message from a specific channel.
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="channel">The channel to send to</param>
        /// <param name="nick">The name of the user that send the message.</param>
        public bool SendMessageToChannelAs( string msg, string channel, string nick )
        {
            string msgString = string.Format(
                ":{0}!{0}@localhost PRIVMSG {1} :{2}",
                nick,
                channel,
                msg
            );

            return this.SendRawCommand( msgString );
        }

        /// <summary>
        /// Sends a PONG to chaskis.
        /// </summary>
        public bool SendPong( string response )
        {
            return this.SendRawCommand(
                ":localhost PONG :" + response
            );
        }

        /// <summary>
        /// Sends the given string AS IS to the chaskis process.
        /// </summary>
        public bool SendRawCommand( string msg )
        {
            if( this.connectionWriter == null )
            {
                throw new InvalidOperationException( "Not Connected." );
            }

            lock( this.connectionWriter )
            {
                this.outCommands.WriteLine( msg );
                this.connectionWriter.WriteLine( msg );
                this.connectionWriter.Flush();
            }
            return true;
        }

        /// <summary>
        /// Waits for the given PRIVMSG to appear from the chaskis process.
        /// </summary>
        public bool WaitForMessageOnChannel( string msgRegex, string channel )
        {
            return this.WaitForMessageOnChannelWithTimeout( msgRegex, channel, TestConstants.DefaultTimeout );
        }

        /// <summary>
        /// Waits for the given PRIVMSG to appear from the chaskis process.
        /// </summary>
        public bool WaitForMessageOnChannelWithTimeout( string msgRegex, string channel, int timeout )
        {
            return this.WaitForStringWithTimeout( "^PRIVMSG " + channel + " :" + msgRegex, timeout );
        }

        /// <summary>
        /// Waits for the given RAW string to come
        /// from the Chaskis process.
        /// </summary>
        public bool WaitForString( string regex )
        {
            return this.WaitForStringWithTimeout( regex, TestConstants.DefaultTimeout );
        }

        /// <summary>
        /// Waits for the given RAW string to come
        /// from the Chaskis process.
        /// </summary>
        public bool WaitForStringWithTimeout( string regex, int timeout )
        {
            this.serverLog.WriteLine( "Waiting for regex " + regex + "..." );
            bool success = this.buffer.WaitForString( regex, timeout );
            this.serverLog.WriteLine( "Waiting for regex " + regex + "...{0}", success ? "Done!" : "Fail!" );

            return success;
        }

        // -------- Helpers --------

        protected virtual void Dispose( bool disposing )
        {
            if( isDisposed == false )
            {
                if( disposing )
                {
                    // Dispose managed state (managed objects).
                }

                // Free unmanaged resources (unmanaged objects) here.
                // TCP Listener (socket) and Threads count as
                // unmanaged objects.
                this.StopServer();

                // Set large fields to null.

                isDisposed = true;
            }
        }

        private void ReadFromSocket()
        {
            TcpClient client = null;
            try
            {
                client = this.connection.AcceptTcpClient();
                this.connectionReader = new StreamReader( client.GetStream() );
                this.connectionWriter = new StreamWriter( client.GetStream() );

                this.connectedEvent.Set();

                while( this.IsConnected )
                {
                    string line = this.connectionReader.ReadLine();
                    if( string.IsNullOrEmpty( line ) == false )
                    {
                        this.inCommands.WriteLine( line );
                        this.buffer.EnqueueString( line );
                        if( this.RespondToPings )
                        {
                            this.RespondToPing( line );
                        }
                    }
                }
            }
            catch( Exception e )
            {
                this.serverLog.WriteLine( "Caught Exception in ReaderThread:" + Environment.NewLine + e.ToString() );
            }
            finally
            {
                this.IsConnected = false;
                client?.Close();
            }
        }

        /// <summary>
        /// Need to respond to ping, otherwise Chaskis will disconnect.
        /// </summary>
        private void RespondToPing( string line )
        {
            Match match = Regex.Match(
                line,
                @"PING\s+(?<msg>\S+)"
            );

            if( match.Success )
            {
                this.SendPong( match.Groups["msg"].Value );
            }
        }
    }
}
