//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SethCS.Basic;
using SethCS.Extensions;

namespace ChaskisCore
{
    public class IrcConnection : IDisposable, IConnection, IChaskisEventScheduler, IChaskisEventSender
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Called when a line is read.
        /// The parameter is the line from the connection.
        /// </summary>
        public event Action<string> ReadEvent;

        // Rationale for maximum length:
        // Per this site: http://www.networksorcery.com/enp/protocol/irc.htm
        // The command MUST either be a valid IRC command or a three digit 
        // number represented in ASCII text. IRC messages are always lines 
        // of characters terminated with a CR-LF pair, and these messages SHALL NOT 
        // exceed 512 characters in length, counting all characters including the 
        // trailing CR-LF. Thus, there are 510 characters maximum allowed for the 
        // command and its parameters. There is no provision for continuation of 
        // message lines.
        //
        // Maximum number of characters for nicknames, usernames, and channels vary per server. Lets assume 64 is the worst-case scenario.
        // 
        // Characters in use:
        //     | char       | Length |
        //     | PRIVMSG    | 7      |
        //     | WhiteSpace | 1      |
        //     | Channel    | 64     |
        //     | WhiteSpace | 1      |
        //     | Colon      | 1      |
        // 
        // That is a total of 74 characters. 510-74 is 436.
        // I say let's round down to 400 just to be extra safe.
        // 
        // If a message is more than 400 characters, split it up and have it appear on a new line.

        /// <summary>
        /// This is the maximum length of a message.  Should
        /// any messages the bot produces be more than this, it
        /// will be split and printed to a new line.
        /// </summary>
        public const int MaximumLength = 400;

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
        /// Writer Queue.
        /// </summary>
        private EventExecutor writerQueue;

        private EventScheduler eventScheduler;

        /// <summary>
        /// Used to mutex-protect keepReading.
        /// </summary>
        private object keepReadingObject;

        /// <summary>
        /// Event to whether or not to abort the reconnection attempt or not.
        /// </summary>
        private ManualResetEvent reconnectAbortEvent;

        /// <summary>
        /// Connection watch dog: Restarts the connection if
        /// we can't talk to the server.
        /// </summary>
        private Thread connectionWatchDog;

        private ManualResetEvent connectionWatchDogKeepGoing;

        /// <summary>
        /// Event that gets triggered when we get a pong.
        /// </summary>
        private AutoResetEvent connectionWatchDogPongEvent;

        bool inited;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public IrcConnection( IIrcConfig config )
        {
            this.inited = false;
            this.Config = new ReadOnlyIrcConfig( config );
            this.IsConnected = false;

            this.connection = null;
            this.ircWriter = null;
            this.ircReader = null;

            this.keepReadingObject = new object();
            this.KeepReading = false;

            this.eventQueue = new EventExecutor(
                true
            );

            this.eventQueue.OnError += EventQueue_OnError;

            this.writerQueue = new EventExecutor( false );
            this.writerQueue.OnError += EventQueue_OnError;

            this.ircWriterLock = new object();
            this.reconnectAbortEvent = new ManualResetEvent( false );
            this.eventScheduler = new EventScheduler();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Whether or not we are connected.
        /// </summary>
        public bool IsConnected { get; private set; }

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
        public IIrcConfig Config { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Inits this class.
        /// </summary>
        public void Init()
        {
            if( this.inited == false )
            {
                // Start Executing
                this.writerQueue.Start();
                this.eventQueue.Start();

                this.connectionWatchDog = new Thread( this.ConnectionWatchDogEntry );
                this.connectionWatchDogKeepGoing = new ManualResetEvent( false );
                this.connectionWatchDogPongEvent = new AutoResetEvent( false );
                this.connectionWatchDog.Start();

                this.inited = true;
            }
        }

        /// <summary>
        /// Connects using the supplied settings.
        /// Throws InvalidOperationException if already connected.
        /// </summary>
        public void Connect()
        {
            if( this.inited == false )
            {
                throw new InvalidOperationException( nameof( this.Init ) + " has not been called yet..." );
            }

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
            Thread.Sleep( this.Config.RateLimit );

            // NICK <nickname>
            this.ircWriter.WriteLine( "NICK {0}", this.Config.Nick );
            this.ircWriter.Flush();
            Thread.Sleep( this.Config.RateLimit );

            // Join Channel.
            // JOIN <channels>
            // If channel does not exist it will be created.
            foreach( string channel in this.Config.Channels )
            {
                this.ircWriter.WriteLine( "JOIN {0}", channel );
                this.ircWriter.Flush();
                Thread.Sleep( this.Config.RateLimit );
            }

            // Tell nickserv we are a bot.
            if( string.IsNullOrEmpty( this.Config.Password ) == false )
            {
                this.ircWriter.WriteLine( "/msg nickserv identify {0}", this.Config.Password );
                this.ircWriter.Flush();
                Thread.Sleep( this.Config.RateLimit );
            }

            this.IsConnected = true;

            this.AddCoreEvent( "CONNECT TO " + this.Config.Server + " AS " + this.Config.Nick );
            StaticLogger.Log.WriteLine( "Connection made!" );
        }

        /// <summary>
        /// Sends the given message to ALL channels this bot is listening on.
        /// </summary>
        public void SendBroadcastMessage( string message )
        {
            foreach( string channel in this.Config.Channels )
            {
                this.SendMessage( message, channel );
            }
        }

        /// <summary>
        /// Sends the given message to the given channel.
        /// Thread-safe.
        /// Throws InvalidOperationException if not connected.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="channel">The user or #channel to send the message to.</param>
        public void SendMessage( string msg, string channel )
        {
            if( this.IsConnected == false )
            {
                throw new InvalidOperationException(
                    "Not connected, can not send command."
                );
            }

            using( StringReader reader = new StringReader( msg ) )
            {
                string line;
                while( ( line = reader.ReadLine() ) != null )
                {
                    if( string.IsNullOrEmpty( line ) == false )
                    {
                        if( line.Length <= MaximumLength )
                        {
                            this.SendMessageHelper( line, channel );
                        }
                        else
                        {
                            // If our length is too great, split it up by our maximum length
                            // and set each split part as a separate message.
                            string[] splitString = line.SplitByLength( MaximumLength );
                            for( int i = 0; i < ( splitString.Length - 1 ); ++i )
                            {
                                this.SendMessageHelper( splitString[i] + "<more>", channel );
                            }
                            this.SendMessageHelper( splitString[splitString.Length - 1], channel );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given message to our writer queue.
        /// </summary>
        private void SendMessageHelper( string line, string channel )
        {
            this.AddToWriterQueue(
                delegate ()
                {
                    lock( this.ircWriterLock )
                    {
                        // This should be in the lock.  If this is before the lock,
                        // CanWrite can return true, this thread can be preempted,
                        // and a thread that disconnects the connection runs.  Now, when this thread runs again, we try to write
                        // to a socket that is closed which is a problem.
                        if( ( this.connection == null ) || ( this.connection.GetStream().CanWrite == false ) )
                        {
                            return;
                        }

                        // PRIVMSG < msgtarget > < message >
                        this.ircWriter.WriteLine( "PRIVMSG {0} :{1}", channel, line );
                        this.ircWriter.Flush();
                    }
                } 
            );
        }

        /// <summary>
        /// Sends a ping to the server so if we are still connected.
        /// </summary>
        /// <param name="msg">The message to ping the server with.</param>
        public void SendPing( string msg )
        {
            // TODO: Maybe add verbose output?
            // StaticLogger.WriteLine( "Sending Ping to server with message '{0}'...", msg );

            string pingString = string.Format( "PING {0}", msg );

            this.SendRawCmd( pingString );
        }

        /// <summary>
        /// Call when we receive a pong from the server.
        /// </summary>
        /// <param name="response">The response from the server.</param>
        public void ReceivedPong( string response )
        {
            if( response == "watchdog" )
            {
                // TODO: Maybe add verbose output?
                // StaticLogger.WriteLine( "Received Watchdog Pong!" );
                this.connectionWatchDogPongEvent.Set();
            }
        }

        /// <summary>
        /// Sends a pong to the server with the given response.
        /// </summary>
        /// <param name="response">The response we need to send.</param>
        public void SendPong( string response )
        {
            string pongString = string.Format( "PONG :{0}", response );
            this.SendRawCmd( pongString );
        }

        /// <summary>
        /// Sends a part to the current channel we are on.
        /// Note, this will make the bot LEAVE the channel.  Only use
        /// if you know what you are doing.
        /// </summary>
        /// <param name="reason">The reason for parting.</param>
        public void SendPart( string reason )
        {
            string partString = string.Format( "PART {0} :{1}", this.Config.Channels, this.Config.QuitMessage );
            this.SendRawCmd( partString );
        }

        /// <summary>
        /// Sends a raw command to the server.
        /// Only use if you REALLY know what you are doing.
        /// </summary>
        /// <param name="cmd">The IRC command to send.</param>
        public void SendRawCmd( string cmd )
        {
            if( this.IsConnected == false )
            {
                throw new InvalidOperationException(
                    "Not connected, can not send command."
                );
            }

            this.AddToWriterQueue(
                delegate ()
                {
                    lock( this.ircWriterLock )
                    {
                        if( ( this.connection == null ) || ( this.connection.GetStream().CanWrite == false ) )
                        {
                            return;
                        }

                        this.ircWriter.WriteLine( cmd );
                        this.ircWriter.Flush();
                    }
                }
            );
        }

        /// <summary>
        /// Adds action to the writer queue.
        /// Used for rate-limiting.
        /// </summary>
        private void AddToWriterQueue( Action action )
        {
            this.writerQueue.AddEvent(
                delegate ()
                {
                    action();
                    Thread.Sleep( this.Config.RateLimit );
                }
            );
        }

        /// <summary>
        /// Disconnected.
        /// No-Op if already disconnected.
        /// </summary>
        public void Disconnect()
        {
            if( IsConnected != false )
            {
                StaticLogger.Log.WriteLine( "Disconnecting..." );

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
                lock( this.ircWriterLock )
                {
                    this.ircReader.Close();
                }

                // 5. Wait for the reader thread to exit.
                this.readerThread.Join();

                // Stop all scheduled events.
                this.eventScheduler.Dispose();

                // Execute all remaining events. Any that call into writing to the channel
                // will be a No-Op as the stream is closed.
                this.eventQueue.Dispose();
                this.writerQueue.Dispose();

                // Finish disconnecting by closing the connection.
                DisconnectHelper();

                this.connectionWatchDogKeepGoing.Set();
                this.connectionWatchDogPongEvent.Set();
                this.connectionWatchDog.Interrupt();
                this.connectionWatchDog.Join();
                this.connectionWatchDogKeepGoing.Dispose();
                this.connectionWatchDogPongEvent.Dispose();

                StaticLogger.Log.WriteLine( "Disconnect Complete." );
            }
        }

        /// <summary>
        /// Helps disconnect the connection.
        /// </summary>
        private void DisconnectHelper()
        {
            this.AddCoreEvent( "DISCONNECTING FROM " + this.Config.Server + " AS " + this.Config.Nick );

            // Disconnect.
            this.connection.Close();

            // Reset everything to null.
            this.ircWriter = null;
            this.ircReader = null;
            this.connection = null;

            // We are not connected.
            this.IsConnected = false;

            this.AddCoreEvent( "DISCONNECTED FROM " + this.Config.Server + " AS " + this.Config.Nick );
        }

        /// <summary>
        /// Cleans up everything.
        /// Calls Disconnect.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            this.eventQueue.OnError -= this.EventQueue_OnError;
            this.writerQueue.OnError -= this.EventQueue_OnError;
        }

        /// <summary>
        /// Schedules a recurring event to be run.
        /// </summary>
        /// <param name="interval">The interval to fire the event at.</param>
        /// <param name="action">
        /// The action to perform after the delay.
        /// Its parameter is an <see cref="IIrcWriter"/> so messages can be sent
        /// out to the channel.
        /// </param>
        /// <returns>The id of the event which can be used to stop it</returns>
        public int ScheduleRecurringEvent( TimeSpan interval, Action<IIrcWriter> action )
        {
            Action<IIrcWriter> theAction = action;
            return this.eventScheduler.ScheduleRecurringEvent(
                interval,
                interval,
                delegate ()
                {
                    this.eventQueue.AddEvent( () => theAction( this ) );
                }
            );
        }

        /// <summary>
        /// Schedules a single event
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="delay">How long to wait until we fire the first event.</param>
        /// <param name="action">
        /// The action to perform after the delay.
        /// Its parameter is an <see cref="IIrcWriter"/> so messages can be sent
        /// out to the channel.
        /// </param>
        /// <returns>The id of the event which can be used to stop it</returns>
        public int ScheduleEvent( TimeSpan delay, Action<IIrcWriter> action )
        {
            Action<IIrcWriter> theAction = action;
            return this.eventScheduler.ScheduleEvent(
                delay,
                delegate ()
                {
                    this.eventQueue.AddEvent( () => theAction( this ) );
                }
            );
        }

        /// <summary>
        /// Stops the event from running.
        /// No-Op if the event is not running.
        /// </summary>
        /// <param name="id">ID of the event to stop.</param>
        public void StopEvent( int id )
        {
            this.eventScheduler.StopEvent( id );
        }

        /// <summary>
        /// Adds the given Chaskis to the event queue.
        /// </summary>
        public void SendChaskisEvent( ChaskisEvent e )
        {
            string s = e.ToString();
            this.AddStringToParsingQueue( s );
        }

        private void AddCoreEvent( string args )
        {
            ChaskisEvent e = new ChaskisEvent(
                ChaskisEventSource.CORE,
                ChaskisEventProtocol.IRC.ToString(),
                ChaskisEvent.BroadcastEventStr,
                args.Split( ' ' )
            );

            this.SendChaskisEvent( e );
        }

        private void AddStringToParsingQueue( string s )
        {
            Action<string> readEvent = this.ReadEvent;
            if( readEvent != null )
            {
                this.eventQueue.AddEvent( () => ReadEvent( s ) );
            }
        }

        /// <summary>
        /// The thread that does the reading.
        /// </summary>
        private void ReaderThread()
        {
            try
            {
                while( this.KeepReading )
                {
                    try
                    {
                        // ReadLine blocks until we call Close() on the underlying stream.
                        string s = this.ircReader.ReadLine();
                        if( ( string.IsNullOrWhiteSpace( s ) == false ) && ( string.IsNullOrEmpty( s ) == false ) )
                        {
                            this.AddStringToParsingQueue( s );
                        }
                    }
                    catch( SocketException err )
                    {
                        StaticLogger.Log.WriteLine( "IRC Connection closed: " + err.Message );
                        if( this.KeepReading )
                        {
                            StaticLogger.Log.ErrorWriteLine(
                                "WARNING IRC connection closed, but we weren't terminating.  Wait for watchdog to reconnect..."
                            );
                            return;
                        }
                        else
                        {
                            // We requested the irc connection be closed since KeepReading is false.
                            // Return, which will terminate this thread.
                            return;
                        }
                    }
                    catch( IOException err )
                    {
                        StaticLogger.Log.WriteLine( "IRC Connection closed: " + err.Message );
                        if( this.KeepReading )
                        {
                            StaticLogger.Log.ErrorWriteLine(
                                "WARNING IRC connection closed, but we weren't terminating.  Wait for watchdog to reconnect..."
                            );
                            return;
                        }
                        else
                        {
                            // We requested the irc connection be closed since KeepReading is false.
                            // Return, which will terminate this thread.
                            return;
                        }
                    }
                    catch( Exception err )
                    {
                        // Unexpected exception occurred.  The connection probably dropped.
                        // Nothing we can do now except to attempt to try again.
                        StaticLogger.Log.ErrorWriteLine(
                            "IRC Reader Thread caught unexpected exception:" + Environment.NewLine + err.ToString() + Environment.NewLine + "Wait for watchdog to reconnect..."
                        );
                    }
                } // End While
            }
            catch( Exception err )
            {
                // Fatal Unexpected exception occurred.
                StaticLogger.Log.ErrorWriteLine(
                    "FATAL ERROR: IRC Reader Thread caught unexpected exception!" + Environment.NewLine + err.ToString()
                );
                StaticLogger.Log.ErrorWriteLine(
                    "If this was because of a watchdog timeout, stand by..."
                );
            }
        }

        private void ConnectionWatchDogEntry()
        {
            StaticLogger.Log.WriteLine( "Connection Watchdog Started." );
            bool keepGoing = true;
            while( keepGoing )
            {
                try
                {
                    if( this.connectionWatchDogKeepGoing.WaitOne( 60 * 1000 ) == false )
                    {
                        // If we timeout, send a ping.  If we do NOT timeout, then we want to wait for a ping.
                        this.SendPing( "watchdog" );
                        if( this.connectionWatchDogPongEvent.WaitOne( 60 * 1000 ) == false )
                        {
                            StaticLogger.Log.WriteLine(
                                "Watch Dog has failed to receive a PONG within 60 seconds, attempting reconnect"
                            );
                            this.AddCoreEvent( "WATCHDOG FAILED" );
                            this.AttemptReconnect();
                        }
                    }
                    else
                    {
                        keepGoing = false;
                    }
                }
                catch( Exception e )
                {
                    StaticLogger.Log.ErrorWriteLine( "Connection Watch Dog had an exception:" );
                    StaticLogger.Log.ErrorWriteLine( e.ToString() );
                }
            }
            StaticLogger.Log.WriteLine( "Connection Watchdog Exiting." );
        }

        /// <summary>
        /// Attempts to restablish the IRC connection.
        /// </summary>
        private void AttemptReconnect()
        {
            // First, acquire the lock for the IRC writer before closing the connection.
            // We don't want any events to run and try to write to a closed connection.
            lock( this.ircWriterLock )
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
            while( ( this.IsConnected == false ) && this.KeepReading )
            {
                try
                {
                    StaticLogger.Log.WriteLine(
                        "Waiting " + timeoutMinutes + " minutes, then attempting reconnect..."
                    );

                    int timeout = timeoutMinutes * 60 * 1000; // Convert to milliseconds.

                    // Sleep the thread.  If we get the signal that we want
                    // to close the program, return, and we won't attempt to reconnect.
                    if( this.reconnectAbortEvent.WaitOne( timeout ) )
                    {
                        StaticLogger.Log.WriteLine(
                            "Terminate signal detected, aborting reconnect..."
                        );
                        return;
                    }

                    // Increase the timeout.  No sense wasting CPU.
                    if( timeoutMinutes < 10 )
                    {
                        timeoutMinutes++;
                    }

                    StaticLogger.Log.WriteLine(
                        "Attempting reconnect..."
                    );

                    // Try connecting.
                    Connect();

                    if( this.IsConnected )
                    {
                        StaticLogger.Log.WriteLine(
                            "We have restablished connection!"
                        );
                    }
                    else
                    {
                        StaticLogger.Log.WriteLine(
                            "Reconnect failed, trying again."
                        );
                    }
                }
                catch( SocketException err )
                {
                    StaticLogger.Log.WriteLine( "IRC Connection closed during reconnection: " + err.Message );
                    if( this.KeepReading )
                    {
                        StaticLogger.Log.ErrorWriteLine( "WARNING IRC connection closed, but we weren't terminating.  Trying to reconnect again..." );
                    }
                    else
                    {
                        // Otherwise, if we are terminating (KeepReading is false),
                        // stop trying to reconnect and return.
                        return;
                    }
                }
                catch( Exception e )
                {
                    StaticLogger.Log.ErrorWriteLine(
                        "Reconnect failed, got exception trying again." + Environment.NewLine + e.ToString()
                    );
                }
            } // End While
        }

        private void EventQueue_OnError( Exception err )
        {
            StringWriter errorMessage = new StringWriter();

            errorMessage.WriteLine( "***************" );
            errorMessage.WriteLine( "Caught Exception in Event Queue Thread:" );
            errorMessage.WriteLine( err.Message );
            errorMessage.WriteLine( err.StackTrace );
            errorMessage.WriteLine( "***************" );

            StaticLogger.Log.ErrorWriteLine( errorMessage.ToString() );
        }
    }
}