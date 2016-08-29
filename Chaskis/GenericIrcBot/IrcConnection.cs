
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SethCS.Basic;

namespace GenericIrcBot
{
    public class IrcConnection : IDisposable, IConnection, IIrcWriter
    {
        // -------- Fields --------

        /// <summary>
        /// Connection to the server.
        /// </summary>
        private TcpClient connection;

        /// <summary>
        /// Used to send commands.
        /// </summary>
        private StreamWriter ircWriter;

        /// <summary>
        /// Lock for the IRC writer.
        /// </summary>
        private object ircWriterLock;

        /// <summary>
        /// Used to read commands.
        /// </summary>
        private StreamReader ircReader;

        /// <summary>
        /// The thread that reads.
        /// </summary>
        private Thread readerThread;

        /// <summary>
        /// Whether or not to keep reading.
        /// </summary>
        private bool keepReading;

        /// <summary>
        /// Event queue.
        /// </summary>
        private EventExecutor eventQueue;

        /// <summary>
        /// Used to mutex-protect keepReading.
        /// </summary>
        private object keepReadingObject;

        /// <summary>
        /// Event to whether or not to abort the reconnection attempt or not.
        /// </summary>
        private ManualResetEvent reconnectAbortEvent;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public IrcConnection( IIrcConfig config )
        {
            this.Config = new ReadOnlyIrcConfig( config );
            this.IsConnected = false;

            this.connection = null;
            this.ircWriter = null;
            this.ircReader = null;

            this.keepReadingObject = new object();
            this.KeepReading = false;

            this.eventQueue = new EventExecutor(
                true,
                delegate( Exception err )
                {
                    if ( this.ErrorLogEvent != null )
                    {
                        StringWriter errorMessage = new StringWriter();

                        errorMessage.WriteLine( "***************" );
                        errorMessage.WriteLine( "Caught Exception in Event Queue Thread:" );
                        errorMessage.WriteLine( err.Message );
                        errorMessage.WriteLine( err.StackTrace );
                        errorMessage.WriteLine( "***************" );

                        this.ErrorLogEvent.Invoke( errorMessage.ToString() );
                    }
                }
            );

            this.ircWriterLock = new object();
            this.reconnectAbortEvent = new ManualResetEvent( false );

            // Start Executing
            this.eventQueue.Start();
        }

        // -------- Properties --------

        /// <summary>
        /// Whether or not we are connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Called when a line is read.
        /// The parameter is the line from the connection.
        /// </summary>
        public Action<string> ReadEvent { get; set; }

        /// <summary>
        /// Called when a status needs to be logged.
        /// The parameter is the message to be logged.
        /// </summary>
        public Action<string> InfoLogEvent { get; set; }

        /// <summary>
        /// Called when an error needs to be logged.
        /// The parameter is the error message.
        /// </summary>
        public Action<string> ErrorLogEvent { get; set; }

        /// <summary>
        /// Whether or not to keep reading.
        /// </summary>
        private bool KeepReading
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
        /// Read-only reference to the IRC Config to use.
        /// </summary>
        public IIrcConfig Config{ get; private set; }


        // -------- Functions --------

        /// <summary>
        /// Connects using the supplied settings.
        /// Throws InvalidOperationException if already connected.
        /// </summary>
        public void Connect()
        {
            if( this.IsConnected == true )
            {
                throw new InvalidOperationException(
                    "Already connected."
                );
            }

            // Connect.
            this.connection = new TcpClient( this.Config.Server, this.Config.Port );
            this.ircWriter = new StreamWriter( this.connection.GetStream() );
            this.ircReader = new StreamReader( this.connection.GetStream() );

            // Start Reading.
            this.KeepReading = true;
            this.readerThread = new Thread( ReaderThread );
            this.readerThread.Start();

            // USER <user> <mode> <unused> <realname>
            // This command is used at the beginning of a connection to specify the username, 
            // real name and initial user modes of the connecting client.
            // <realname> may contain spaces, and thus must be prefixed with a colon. 
            this.ircWriter.WriteLine( "USER {0} 0 * :{1}", this.Config.UserName, this.Config.RealName );
            this.ircWriter.Flush();

            // NICK <nickname>
            this.ircWriter.WriteLine( "NICK {0}", this.Config.Nick );
            this.ircWriter.Flush();

            // Join Channel.
            // JOIN <channels>
            // If channel does not exist it will be created.
            this.ircWriter.WriteLine( "JOIN {0}", this.Config.Channel );
            this.ircWriter.Flush();

            // Tell nickserv we are a bot.
            if( string.IsNullOrEmpty( this.Config.Password ) == false )
            {
                this.ircWriter.WriteLine( "/msg nickserv identify {0}", this.Config.Password );
            }

            this.IsConnected = true;

            this.InfoLogEvent?.Invoke( "Connection made!" );
        }

        /// <summary>
        /// Sends the given command to the channel.
        /// Thread-safe.
        /// Throws InvalidOperationException if not connected.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        public void SendCommandToChannel( string msg )
        {
            SendMessageToUser( msg, this.Config.Channel );
        }

        /// <summary>
        /// Sends the given command to the user.  Also works for sending messages
        /// to other channels.
        /// Thread-safe.
        /// Throws InvalidOperationException if not connected.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="userNick">The user (or #channel) to send the message to.</param>
        public void SendMessageToUser( string msg, string userNick )
        {
            if ( this.IsConnected == false )
            {
                throw new InvalidOperationException(
                    "Not connected, can not send command."
                );
            }

            lock ( this.ircWriterLock )
            {
                // This should be in the lock.  If this is before the lock,
                // CanWrite can return true, this thread can be preempted,
                // and a thread that disconnects the connection runs.  Now, when this thread runs again, we try to write
                // to a socket that is closed which is a problem.
                if ( ( this.connection == null )|| ( this.connection.GetStream().CanWrite == false ) )
                {
                    return;
                }

                using ( StringReader reader = new StringReader( msg ) )
                {
                    string line;
                    while ( ( line = reader.ReadLine() ) != null )
                    {
                        if ( string.IsNullOrEmpty( line ) == false )
                        {
                            // PRIVMSG < msgtarget > < message >
                            this.ircWriter.WriteLine( "PRIVMSG {0} :{1}", userNick, line );
                            this.ircWriter.Flush();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends a pong to the given url.
        /// </summary>
        /// <param name="response">The response we need to send.</param>
        public void SendPong( string response )
        {
            if ( this.IsConnected == false )
            {
                throw new InvalidOperationException(
                    "Not connected, can not send PONG."
                );
            }

            lock ( this.ircWriterLock )
            {
                if ( ( this.connection == null ) || ( this.connection.GetStream().CanWrite == false ) )
                {
                    return;
                }

                // PONG :response
                this.ircWriter.WriteLine( "PONG :{0}", response );
                this.ircWriter.Flush();
            }
        }

        /// <summary>
        /// Sends a part to the current channel we are on.
        /// Note, this will make the bot LEAVE the channel.  Only use
        /// if you know what you are doing.
        /// </summary>
        /// <param name="reason">The reason for parting.</param>
        public void SendPart( string reason )
        {
            lock ( this.ircWriterLock )
            {
                if ( ( this.connection == null ) || ( this.connection.GetStream().CanWrite == false ) )
                {
                    return;
                }

                // PART :reason
                this.ircWriter.WriteLine( "PART {0} :{1}", this.Config.Channel, this.Config.QuitMessage );
                this.ircWriter.Flush();
            }
        }

        /// <summary>
        /// Disconnected.
        /// No-Op if already disconnected.
        /// </summary>
        public void Disconnect()
        {
            if( IsConnected != false )
            {
                this.InfoLogEvent?.Invoke( "Disconnecting..." );

                // Stop the reader thread.  This prevents any more events from
                // being queued.

                // These events must be in in order for this thing to work.

                // 1. Set KeepReading to false.  The abort logic in the Reader
                //    thread depends on this.
                this.KeepReading = false;

                // 2. We could be in the reconnecting state.  Trigger that thread to awaken
                //    if its sleeping between reconnects.  This will cause the thread to return.
                this.reconnectAbortEvent.Set();

                // 4. Close the TCP stream.  If we are waiting for data to come over TCP from the IRC server,
                //    we need to close the IRC connection for the reader thread to abort.
                //
                //    The ircReader and the ircWriter share the same stream, so closing one will close the other.
                //    Any calls to SendPong or SendMessageToUser are ignored as the stream's CanWrite will be set to false
                //    since the stream is closed.
                //
                //    We also must lock on the writer lock.  We don't want to close the stream
                //    while the writer is writing.
                lock ( this.ircWriterLock )
                {
                    this.ircReader.Close();
                }

                // 5. Wait for the reader thread to exit.
                this.readerThread.Join();

                // Execute all remaining events. Any that call into writing to the channel
                // will be a No-Op as the stream is closed.
                this.eventQueue.Dispose();

                // Finish disconnecting by closing the connection.
                DisconnectHelper();

                this.InfoLogEvent?.Invoke( "Disconnect Complete." );
            }
        }

        /// <summary>
        /// Helps disconnect the connection.
        /// </summary>
        private void DisconnectHelper()
        {
            // Disconnect.
            this.connection.Close();

            // Reset everything to null.
            this.ircWriter = null;
            this.ircReader = null;
            this.connection = null;

            // We are not connected.
            this.IsConnected = false;
        }

        /// <summary>
        /// Cleans up everything.
        /// Calls Disconnect.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// The thread that does the reading.
        /// </summary>
        private void ReaderThread()
        {
            try
            {
                while ( this.KeepReading )
                {
                    try
                    {
                        // ReadLine blocks until we call Close() on the underlying stream.
                        string s = this.ircReader.ReadLine();
                        if ( ( string.IsNullOrWhiteSpace( s ) == false ) && ( string.IsNullOrEmpty( s ) == false ) )
                        {
                            if ( this.ReadEvent != null )
                            {
                                this.eventQueue.AddEvent( () => ReadEvent( s ) );
                            }

                        }
                    }
                    catch ( SocketException err )
                    {
                        this.InfoLogEvent?.Invoke( "IRC Connection closed: " + err.Message );
                        if ( this.KeepReading )
                        {
                            this.ErrorLogEvent?.Invoke( "WARNING IRC connection closed, but we weren't terminating.  Trying to reconnect..." );
                            AttemptReconnect();
                        }
                        else
                        {
                            // We requested the irc connection be closed since KeepReading is false.
                            // Return, which will terminate this thread.
                            return;
                        }
                    }
                    catch ( IOException err )
                    {
                        this.InfoLogEvent?.Invoke( "IRC Connection closed: " + err.Message );
                        if ( this.KeepReading )
                        {
                            this.ErrorLogEvent?.Invoke( "WARNING IRC connection closed, but we weren't terminating.  Trying to reconnect..." );
                            AttemptReconnect();
                        }
                        else
                        {
                            // We requested the irc connection be closed since KeepReading is false.
                            // Return, which will terminate this thread.
                            return;
                        }
                    }
                    catch ( Exception err )
                    {
                        // Unexpected exception occurred.  The connection probably dropped.
                        // Nothing we can do now except to attempt to try again.
                        this.ErrorLogEvent?.Invoke(
                            "IRC Reader Thread caught unexpected exception:" + Environment.NewLine + err.ToString() + Environment.NewLine + "Attempting to reconnect..."
                        );

                        AttemptReconnect();
                    }
                } // End While
            }
            catch ( Exception err )
            {
                // Fatal Unexpected exception occurred.
                this.ErrorLogEvent?.Invoke(
                    "FATAL ERROR: IRC Reader Thread caught unexpected exception and can not reconnect:" + Environment.NewLine + err.ToString()
                );
            }
        }

        /// <summary>
        /// Attempts to restablish the IRC connection.
        /// </summary>
        private void AttemptReconnect()
        {
            // First, acquire the lock for the IRC writer before closing the connection.
            // We don't want any events to run and try to write to a closed connection.
            lock ( this.ircWriterLock )
            {
                this.ircReader.Close(); // Close the IRC Stream.  This also closes the writer as well.
                DisconnectHelper();
            }
            // Bad news is when we release the lock, any event that wants to write to the IRC Channel
            // will not be.  We *could* cache them by keeping the ircWriterLock until we establish connection again,
            // but then when we rejoin the channel we'll flood it with messages.  That's undesirable.
            // So, when we unblock the thread, any calls to WriteToChannel or SendPong will be a No-Op.  All the events
            // (such as writing to a database, which is more important) will occur though and will not be blocked.

            // Keep trying to connect until either we are connected again,
            // or the program is terminating (KeepReading goes to false).
            int timeoutMinutes = 1;
            while ( ( this.IsConnected == false ) && this.KeepReading )
            {
                try
                {
                    this.InfoLogEvent?.Invoke(
                        "Waiting " + timeoutMinutes + " minutes, then attempting reconnect..."
                    );

                    int timeout = timeoutMinutes * 60 * 1000; // Convert to milliseconds.

                    // Sleep the thread.  If we get the signal that we want
                    // to close the program, return, and we won't attempt to reconnect.
                    if ( this.reconnectAbortEvent.WaitOne( timeout ) )
                    {
                        this.InfoLogEvent?.Invoke(
                            "Terminate signal detected, aborting reconnect..."
                        );
                        return;
                    }

                    // Increase the timeout.  No sense wasting CPU.
                    if ( timeoutMinutes < 10 )
                    {
                        timeoutMinutes++;
                    }

                    this.InfoLogEvent?.Invoke(
                        "Attempting reconnect..."
                    );

                    // Try connecting.
                    Connect();

                    if ( this.IsConnected )
                    {
                        this.InfoLogEvent?.Invoke(
                            "We have restablished connection!"
                        );
                    }
                    else
                    {
                        this.InfoLogEvent?.Invoke(
                            "Reconnect failed, trying again."
                        );
                    }
                }
                catch ( SocketException err )
                {
                    this.InfoLogEvent?.Invoke( "IRC Connection closed during reconnection: " + err.Message );
                    if ( this.KeepReading )
                    {
                        this.ErrorLogEvent?.Invoke( "WARNING IRC connection closed, but we weren't terminating.  Trying to reconnect again..." );
                    }
                    else
                    {
                        // Otherwise, if we are terminating (KeepReading is false),
                        // stop trying to reconnect and return.
                        return;
                    }
                }
                catch ( Exception e )
                {
                    this.ErrorLogEvent?.Invoke(
                        "Reconnect failed, got exception trying again." + Environment.NewLine + e.ToString()
                    );
                }
            } // End While
        }
    }
}
