//
//          Copyright Seth Hendrick 2016-2020.
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
using SethCS.Basic;
using SethCS.Extensions;

namespace Chaskis.RegressionTests.TestCore
{
    /// <summary>
    /// This class emulates an IRC server that Chaskis will talk to.
    /// </summary>
    public class IrcServer : IDisposable
    {
        // ---------------- Fields ----------------

        private bool isDisposed;

        /// <summary>
        /// Commands sent out from server.
        /// </summary>
        private readonly GenericLogger outCommands;

        /// <summary>
        /// Commands sent in to server.
        /// </summary>
        private readonly GenericLogger inCommands;

        /// <summary>
        /// Log for the server.
        /// </summary>
        private readonly GenericLogger serverLog;

        private readonly StringBuffer buffer;

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
        private readonly object respondToPingsObject;

        private bool respondToJoins;
        private readonly object respondToJoinsLock;

        private string nickName;
        private readonly object nickNameLock;

        private AutoResetEvent connectedEvent;

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

            this.respondToJoins = true;
            this.respondToJoinsLock = new object();

            this.nickName = string.Empty;
            this.nickNameLock = new object();
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

        private bool RespondToJoins
        {
            get
            {
                lock( this.respondToJoinsLock )
                {
                    return this.respondToJoins;
                }
            }
            set
            {
                lock( this.respondToJoinsLock )
                {
                    this.respondToJoins = value;
                }
            }
        }

        private string NickName
        {
            get
            {
                lock( this.nickNameLock )
                {
                    return this.nickName;
                }
            }
            set
            {
                lock( this.nickNameLock )
                {
                    this.nickName = value;
                }
            }
        }

        // ---------------- Functions ----------------

        // -------- Start / Stop --------

        /// <summary>
        /// Starts the server on the given port.
        /// </summary>
        public void StartServer( ushort port )
        {
            if( this.connection != null )
            {
                throw new InvalidOperationException( "Connection already made!" );
            }

            this.buffer.FlushQueue();

            this.connectedEvent = new AutoResetEvent( false );

            this.connection = new TcpListener(
                new IPEndPoint( IPAddress.Loopback, port )
            );
            this.connection.Start();

            this.IsConnected = true;
            this.readerThread = new Thread( this.ReadFromSocket );
            this.readerThread.Start();

            this.serverLog.WriteLine( "Listening for connection on port " + port );
        }

        /// <summary>
        /// Waits until a client connects to the server.
        /// </summary>
        public bool WaitForConnection( int timeout = 30 * 1000 )
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
        /// Resets the server connection.
        /// Needed if the client goes down.
        /// </summary>
        public void ResetServerConnection()
        {
            this.connectionReader?.Dispose();
            this.connectionWriter?.Dispose();
        }

        /// <summary>
        /// Stops the server.
        /// Calls <see cref="Dispose()"/> under the hood.
        /// </summary>
        public void StopServer()
        {
            if( this.connection == null )
            {
                throw new InvalidOperationException( "No Connection Exists!" );
            }

            this.IsConnected = false;
            this.ResetServerConnection();
            this.connection?.Stop();
            this.connection = null;

            this.readerThread?.Join();
        }

        /// <summary>
        /// Disposes the server.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        // -------- Join --------

        /// <summary>
        /// Do we respond to joins when the client joins a channel?
        /// </summary>
        public void SetJoinResponse( bool enable )
        {
            this.RespondToJoins = enable;
        }

        // -------- Ping / Pong --------

        /// <summary>
        /// Do we repond to pings from the client?
        /// Defaulted to true.
        /// </summary>
        public void SetPingResponse( bool enable )
        {
            this.RespondToPings = enable;
        }

        // -------- Send / Wait Commands --------

        /// <summary>
        /// Sends a message from a specific channel.
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="channel">The channel to send to</param>
        /// <param name="nick">The name of the user that send the message.</param>
        /// <param name="waitMsg">The message to wait for as a regex.</param>
        /// <param name="timeout">How long to wait before giving up.</param>
        public bool SendMessageToChannelAsWaitMsg( string msg, string channel, string nick, string waitMsg, int timeout = TestConstants.DefaultTimeout )
        {
            this.SendMessageToChannelAs( msg, channel, nick );
            bool success = this.WaitForMessageOnChannel( waitMsg, channel, timeout );

            return success;
        }

        /// <summary>
        /// Sends a message from a specific channel.
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="channel">The channel to send to</param>
        /// <param name="nick">The name of the user that send the message.</param>
        public void SendMessageToChannelAs( string msg, string channel, string nick )
        {
            string msgString = string.Format(
                ":{0}!{0}@localhost PRIVMSG {1} :{2}",
                nick,
                channel,
                msg
            );

            this.SendRawCommand( msgString );
        }

        /// <summary>
        /// Sends a PONG to chaskis.
        /// </summary>
        public void SendPong( string response )
        {
            this.SendRawCommand(
                ":localhost PONG localhost :" + response
            );
        }

        /// <summary>
        /// Sends a JOIN message from the server
        /// to the client.
        /// </summary>
        /// <param name="user">The user that joined the channel</param>
        /// <param name="channel">The channel the user joined.</param>
        public void SendJoined( string user, string channel )
        {
            string msgString = string.Format(
                ":{0}!{0}@localhost JOIN {1}",
                user,
                channel
            );

            this.SendRawCommand( msgString );
        }

        /// <summary>
        /// Sends a PART message from the server
        /// to the client.
        /// </summary>
        /// <param name="user">The user that parted the channel</param>
        /// <param name="channel">The channel the user parted.</param>
        public void SendPartedFrom( string user, string channel )
        {
            string msgString = string.Format(
                ":{0}!{0}@localhost PART {1}",
                user,
                channel
            );

            this.SendRawCommand( msgString );
        }

        /// <summary>
        /// Sends a PART message from the server
        /// to the client with a reason.
        /// </summary>
        /// <param name="user">The user that parted the channel</param>
        /// <param name="channel">The channel the user parted.</param>
        public void SendPartedFromWithReason( string user, string channel, string reason )
        {
            string msgString = string.Format(
                ":{0}!{0}@localhost PART {1} :{2}",
                user,
                channel,
                reason
            );

            this.SendRawCommand( msgString );
        }

        /// <summary>
        /// Sends a KICK message from the server
        /// to the client.
        /// </summary>
        /// <param name="kickedUser">The user that was kicked from the channel</param>
        /// <param name="channel">The channel the user was kicked from.</param>
        /// <param name="moderator">The user that performed the kicking.</param>
        public void SendKickedFromBy( string kickedUser, string channel, string moderator )
        {
            string msgString = string.Format(
                ":{2}!{2}@localhost KICK {1} {0}",
                kickedUser,
                channel,
                moderator
            );

            this.SendRawCommand( msgString );
        }

        /// <summary>
        /// Sends a KICK message from the server
        /// to the client with a reason.
        /// </summary>
        /// <param name="kickedUser">The user that was kicked from the channel</param>
        /// <param name="channel">The channel the user was kicked from.</param>
        /// <param name="moderator">The user that performed the kicking.</param>
        /// <param name="reason">The reason the user was kicked.</param>
        public void SendKickedFromByWithReason( string kickedUser, string channel, string moderator, string reason )
        {
            string msgString = string.Format(
                ":{2}!{2}@localhost KICK {1} {0} :{3}",
                kickedUser,
                channel,
                moderator,
                reason
            );

            this.SendRawCommand( msgString );
        }

        /// <summary>
        /// Sends the given string AS IS to the chaskis process.
        /// </summary>
        public void SendRawCommand( string msg )
        {
            if( this.connectionWriter == null )
            {
                throw new InvalidOperationException( "Not Connected." );
            }

            lock( this.connectionWriter )
            {
                this.outCommands.WriteLine( msg.EscapeNonCharacters() );
                this.connectionWriter.WriteLine( msg );
                this.connectionWriter.Flush();
            }
        }

        /// <summary>
        /// Waits for the given PRIVMSG to appear from the chaskis process.
        /// </summary>
        public bool WaitForMessageOnChannel( string msgRegex, string channel, int timeout = TestConstants.DefaultTimeout )
        {
            return this.WaitForString( "^PRIVMSG " + channel + " :" + msgRegex, timeout );
        }

        /// <summary>
        /// Waits for the given NOTICE to appear from the chaskis process.
        /// </summary>
        public bool WaitForNoticeOnChannel( string msgRegex, string channel, int timeout = TestConstants.DefaultTimeout )
        {
            return this.WaitForString( "^NOTICE " + channel + " :" + msgRegex, timeout );
        }

        /// <summary>
        /// Waits for the given RAW string to come
        /// from the Chaskis process.
        /// </summary>
        public bool WaitForString( string regex, int timeout = TestConstants.DefaultTimeout )
        {
            this.serverLog.WriteLine( "Waiting for regex " + regex.EscapeNonCharacters() + "..." );
            bool success = this.buffer.WaitForString( regex, timeout );
            this.serverLog.WriteLine( "Waiting for regex " + regex.EscapeNonCharacters() + "...{0}", success ? "Done!" : "Fail!" );

            return success;
        }

        /// <summary>
        /// Creates a <see cref="StringWatcher"/> to watch for strings
        /// from the server.  Remember to call <see cref="StringWatcher.Dispose"/>
        /// when done.
        /// </summary>
        public StringWatcher CreateStringWatcher( string regex )
        {
            return new StringWatcher( this.buffer, regex, this.serverLog );
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
            while( this.IsConnected )
            {
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
                            if( this.RespondToJoins )
                            {
                                this.RespondToJoin( line );
                            }
                            this.HandleNickCommand( line );
                        }
                    }
                }
                catch( ObjectDisposedException e )
                {
                    this.serverLog.WriteLine(
                        "Caught ObjectDisposedException in Reader Thread.  Connection was probably reset." + Environment.NewLine + e.ToString()
                    );
                }
                catch( SocketException e )
                {
                    if ( e.SocketErrorCode == SocketError.Interrupted )
                    {
                        this.serverLog.WriteLine(
                            "Caught Interrupted SocketException in Reader Thread.  Connection was probably reset." + Environment.NewLine + e.ToString()
                        );
                    }
                    else
                    {
                        this.serverLog.WriteLine(
                            "Caught unexpected SocketException in Reader Thread.  ABORT" + Environment.NewLine + e.ToString()
                        );
                        this.IsConnected = false;
                    }
                }
                catch( Exception e )
                {
                    this.serverLog.WriteLine( "Caught Exception in ReaderThread:" + Environment.NewLine + e.ToString() );
                    this.IsConnected = false;
                }
                finally
                {
                    client?.Close();
                }
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

        private void HandleNickCommand( string line )
        {
            Match match = Regex.Match(
                line,
                @"NICK\s+(?<nick>\S+)"
            );
            if( match.Success )
            {
                this.NickName = match.Groups["nick"].Value;
            }
        }

        private void RespondToJoin( string line )
        {
            Match match = Regex.Match(
                line,
                @"JOIN\s+(?<channel>\S+)"
            );

            if( match.Success )
            {
                string command = string.Format(
                    ":{0}!~{0}@{1} JOIN {2}",
                    this.NickName,
                    new IPAddress( new byte[] { 127, 0, 0, 1 } ), // Just use localhost for now I guess?
                    match.Groups["channel"]
                );

                this.SendRawCommand( command );
            }
        }
    }
}
