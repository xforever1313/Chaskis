//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SethCS.Basic;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Core
{
    public class IrcConnection : IDisposable, IConnection, IChaskisEventScheduler, IInterPluginEventSender
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Called when a line is read.
        /// The parameter is the line from the connection.
        /// </summary>
        public event Action<string> ReadEvent;

        /// <summary>
        /// Internal event called when the reader thread exits.
        /// Mainly here for unit testing reasons.
        /// </summary>
        internal event Action OnReaderThreadExit;

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
        public static readonly int MaximumLength = 400;

        private readonly IIrcMac connection;

        private readonly MacWriter macWriter;

        /// <summary>
        /// The thread that reads.
        /// </summary>
        private Thread readerThread;

        /// <summary>
        /// Whether or not to keep reading.
        /// </summary>
        private bool keepReading;

        private readonly EventScheduler eventScheduler;

        /// <summary>
        /// Used to mutex-protect keepReading.
        /// </summary>
        private readonly object keepReadingObject;

        /// <summary>
        /// Event to whether or not to abort the reconnection attempt or not.
        /// </summary>
        private readonly ManualResetEvent reconnectAbortEvent;

        private bool inited;

        /// <summary>
        /// TODO: Remove this.
        /// </summary>
        private readonly INonDisposableStringParsingQueue parsingQueue;

        public static readonly string WatchdogStr = "watchdog";

        private readonly IrcWatchdog watchDog;

        private readonly PingHandler pingHandler;
        private readonly PongHandler pongHandler;

        // -------- Constructor --------

        public IrcConnection( IReadOnlyIrcConfig config, INonDisposableStringParsingQueue parsingQueue ) :
            this( config, parsingQueue, new IrcMac( config, StaticLogger.Log ) )
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public IrcConnection( IReadOnlyIrcConfig config, INonDisposableStringParsingQueue parsingQueue, IIrcMac macLayer )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );
            ArgumentChecker.IsNotNull( parsingQueue, nameof( parsingQueue ) );
            ArgumentChecker.IsNotNull( macLayer, nameof( macLayer ) );

            this.inited = false;
            this.Config = config;
            this.IsConnected = false;

            this.connection = macLayer;

            this.keepReadingObject = new object();
            this.KeepReading = false;

            this.macWriter = new MacWriter( StaticLogger.Log, macLayer, this.Config.RateLimit );

            this.reconnectAbortEvent = new ManualResetEvent( false );
            this.eventScheduler = new EventScheduler();

            this.parsingQueue = parsingQueue;
            this.watchDog = new IrcWatchdog(
                StaticLogger.Log,
                () => this.SendPing( WatchdogStr ),
                this.Watchdog_OnFailure,
                this.Config.WatchdogTimeout
            );

            this.pingHandler = new PingHandler();
            this.pongHandler = new PongHandler();
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
        public IReadOnlyIrcConfig Config { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Inits this class.
        /// </summary>
        public void Init()
        {
            if( this.inited == false )
            {
                // Start Executing
                this.macWriter.Start();
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

            if( this.readerThread != null )
            {
                throw new InvalidOperationException(
                    "Somehow our reader thread is not null, this should never happen.  Something went wrong."
                );
            }

            // Connect.
            this.connection.Connect();

            // Start Reading.
            this.KeepReading = true;
            this.readerThread = new Thread( ReaderThread )
            {
                Name = this.Config.Server + " IRC reader thread"
            };
            this.readerThread.Start();

            // Per RFC-2812, the server password sets a "connection password".
            // This MUST be sent before any attempt to set the username and nick name.
            // Therefore, this is the first command that gets sent.
            if( string.IsNullOrEmpty( this.Config.ServerPassword ) == false )
            {
                this.macWriter.SendMessage( $"PASS {this.Config.GetServerPassword()}" );
            }
            else
            {
                StaticLogger.Log.WriteLine( "No Server Password Specified" );
            }

            // USER <user> <mode> <unused> <realname>
            // This command is used at the beginning of a connection to specify the username,
            // real name and initial user modes of the connecting client.
            // <realname> may contain spaces, and thus must be prefixed with a colon.
            this.macWriter.SendMessage( $"USER {this.Config.UserName} 0 * :{this.Config.RealName}" );

            // NICK <nickname>
            this.macWriter.SendMessage( $"NICK {this.Config.Nick}" );

            // If the server has a NickServ service, tell nickserv our password
            // so it registers our bot and does not change its nickname on us.
            if( string.IsNullOrEmpty( this.Config.NickServPassword ) == false )
            {
                this.macWriter.SendMessage(
                    $"PRIVMSG {this.Config.NickServNick} :{this.Config.GetNickServMessage()}"
                );
                Thread.Sleep( this.Config.RateLimit );
            }
            else
            {
                StaticLogger.Log.WriteLine( "No NickServ Password Specified" );
            }

            // At this point, we are connected, we just simply haven't joined channels yet.
            this.IsConnected = true;

            StaticLogger.Log.WriteLine( "Connection made!" );
            this.OnReadLine(
                new ConnectedEventArgs
                {
                    Server = this.Config.Server,
                    Protocol = ChaskisEventProtocol.IRC,
                    Writer = this
                }.ToXml()
            );

            // Join Channel.
            // JOIN <channels>
            // If channel does not exist it will be created.
            foreach( string channel in this.Config.Channels )
            {
                this.SendJoin( channel );
            }

            // Wait for joins to go out before continuing.
            using( ManualResetEventSlim e = new ManualResetEventSlim( false ) )
            {
                const int maxSeconds = 10;

                this.macWriter.BeginInvoke( () => e.Set() );
                if( e.Wait( TimeSpan.FromSeconds( maxSeconds ) ) == false )
                {
                    throw new TimeoutException(
                        $"Writer Queue did not join the channels within {maxSeconds} seconds"
                    );
                }
            }

            this.OnReadLine(
                new FinishedJoiningChannelsEventArgs
                {
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                    Writer = this
                }.ToXml()
            );

            // Once we connected, go ahead and start the watchdog.
            // This will be the only time the watchdog should be started.
            // This is wrapped in a started check since the watchdog will call
            // this function when it tries to reconnect, and we don't want to call start
            // when its already started.
            if ( this.watchDog.Started == false )
            {
                this.watchDog.Start();
            }
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
            this.OnReadLine(
                new SendMessageEventArgs
                {
                    ChannelOrUser = channel,
                    Message = msg,
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                    Writer = this
                }.ToXml()
            );

            this.SendMessageInternal( msg, channel, string.Empty, string.Empty, PrivateMessageHelper.IrcCommand );
        }

        /// <summary>
        /// Sends an IRC action.  This is the equivalent of the '/me' command in many IRC clients.
        /// Thread-safe.
        /// Throws InvalidOperationException if not connected.
        /// </summary>
        /// <param name="msg">The action in message form the bot is going to take.</param>
        /// <param name="channel">Which channel to send the action to.</param>
        public void SendAction( string msg, string channel )
        {
            this.OnReadLine(
                new SendActionEventArgs
                {
                    ChannelOrUser = channel,
                    Message = msg,
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                    Writer = this
                }.ToXml()
            );
            this.SendMessageInternal( msg, channel, "\u0001ACTION ", "\u0001", PrivateMessageHelper.IrcCommand );
        }

        /// <summary>
        /// Sends the given NOTICE to the given channel.
        /// Thread-safe.
        /// Throws InvalidOperationException if not connected.
        /// </summary>
        /// <param name="msg">The NOTICE to send.</param>
        /// <param name="channel">The user or #channel to send the NOTICE to.</param>
        public void SendNotice( string msg, string channel )
        {
            this.OnReadLine(
                new SendNoticeEventArgs
                {
                    ChannelOrUser = channel,
                    Message = msg,
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                    Writer = this
                }.ToXml()
            );
            this.SendMessageInternal( msg, channel, string.Empty, string.Empty, "NOTICE" );
        }

        /// <summary>
        /// Sends the proper "PONG" response when responding to a CTCP Ping.
        /// </summary>
        /// <remarks>
        /// Under-the-hood, this really sends a CTCP Ping, but with a notice.
        /// </remarks>
        /// <param name="msg">The message to tie with the pong, should usually be the same message we got with the ping.</param>
        /// <param name="userName">The user to send the ping to.</param>
        public void SendCtcpPong( string msg, string userName )
        {
            this.OnReadLine(
                new SendCtcpPongEventArgs
                {
                    ChannelOrUser = userName,
                    Message = msg,
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                    Writer = this
                }.ToXml()
            );
            this.SendMessageInternal( msg, userName, "\u0001PING ", "\u0001", "NOTICE" );
        }

        /// <inheritdoc/>
        public void SendCtcpVersionResponse( string message, string userName )
        {
            this.OnReadLine(
                new SendCtcpVersionEventArgs
                {
                    ChannelOrUser = userName,
                    Message = message,
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                    Writer = this
                }.ToXml()
           );

           // CTCP users notices for responses.
           this.SendMessageInternal( message, userName, "\u0001VERSION ", "\u0001", "NOTICE" );
        }

        private void SendMessageInternal( string msg, string channel, string prefix, string suffix, string type )
        {
            if ( this.IsConnected == false )
            {
                throw new InvalidOperationException(
                    "Not connected, can not send command."
                );
            }

            using ( StringReader reader = new StringReader( msg ) )
            {
                string line;
                while ( ( line = reader.ReadLine() ) != null )
                {
                    if ( string.IsNullOrEmpty( line ) == false )
                    {
                        if ( line.Length <= MaximumLength )
                        {
                            this.SendMessageHelper( prefix + line + suffix, channel, type );
                        }
                        else
                        {
                            // If our length is too great, split it up by our maximum length
                            // and set each split part as a separate message.
                            string[] splitString = line.SplitByLength( MaximumLength );
                            for ( int i = 0; i < ( splitString.Length - 1 ); ++i )
                            {
                                this.SendMessageHelper( prefix + splitString[i] + " <more>" + suffix, channel, type );
                            }
                            this.SendMessageHelper( prefix + splitString[splitString.Length - 1] + suffix, channel, type );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given message to our writer queue.
        /// </summary>
        private void SendMessageHelper( string line, string channel, string msgType )
        {
            // PRIVMSG < msgtarget > < message >
            this.SendRawCmd(
                $"{msgType} {channel} :{line}"
            );
        }

        /// <summary>
        /// Sends a ping to the server so if we are still connected.
        /// </summary>
        /// <param name="msg">The message to ping the server with.</param>
        public void SendPing( string msg )
        {
            StaticLogger.Log.WriteLine(
                Convert.ToInt32( LogVerbosityLevel.HighVerbosity ),
                "Sending Ping to server with message '{0}'...",
                msg
            );

            string pingString = string.Format( "PING {0}", msg );

            this.SendRawCmd( pingString );
        }

        /// <summary>
        /// Call when we receive a pong from the server.
        /// </summary>
        /// <param name="response">The response from the server.</param>
        public void ReceivedPong( string response )
        {
            if( response.EqualsIgnoreCase( WatchdogStr ) )
            {
                if ( this.watchDog.Started )
                {
                    this.watchDog.ResetTimer();
                }
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

        /// <inheritdoc/>
        public void SendJoin( string channel )
        {
            this.OnReadLine(
                new SendJoinEventArgs
                {
                    Channel = channel,
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                }.ToXml()
            );

            string joinString = $"JOIN {channel}";
            this.SendRawCmd( joinString );
        }

        /// <inheritdoc/>
        public void SendPart( string reason, string channel )
        {
            // TODO: Make reason string more smart if not specified.
            reason = reason ?? this.Config.QuitMessage;

            this.OnReadLine(
                new SendPartEventArgs
                {
                    Channel = channel,
                    Reason = reason,
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                    Writer = this
                }.ToXml()
            );

            string partString = $"PART {channel} :{reason}";
            this.SendRawCmd( partString );
        }

        public void SendKick( string userToKick, string channel, string reason = null )
        {
            this.OnReadLine(
                new SendKickEventArgs
                {
                    Channel = channel,
                    Reason = reason ?? string.Empty,
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                    Writer = this
                }.ToXml()
            );

            string kickString;
            if( string.IsNullOrWhiteSpace( reason ) )
            {
                kickString = $"KICK {channel} {userToKick}";
            }
            else
            {
                kickString = $"KICK {channel} {userToKick} :{reason}";
            }

            this.SendRawCmd( kickString );
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

            this.OnReadLine(
                new SendEventArgs
                {
                    Command = cmd,
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server,
                    Writer = this
                }.ToXml()
            );

            // This should be in the lock.  If this is before the lock,
            // IsConnected can return true, this thread can be preempted,
            // and a thread that disconnects the connection runs.  Now, when this thread runs again, we try to write
            // to a socket that is closed which is a problem.
            lock( this.macWriter )
            {
                this.macWriter.SendMessage( cmd );
            }
        }

        /// <summary>
        /// Helps disconnect the connection.
        /// </summary>
        private void DisconnectHelper()
        {
            // First, acquire the mac writer.  This will prevent anything writing to the connection,
            // as those are also guarded by this lock.
            // When this lock is released, the connection's IsConnected flag will be set to false, so
            // all of those writes become no-ops.
            lock( this.macWriter )
            {
                this.OnReadLine(
                    new DisconnectingEventArgs
                    {
                        Server = this.Config.Server,
                        Protocol = ChaskisEventProtocol.IRC
                    }.ToXml()
                );

                // - Close the TCP stream.  If we are waiting for data to come over TCP from the IRC server,
                //   we need to close the IRC connection for the reader thread to abort.
                this.connection.Disconnect();

                // - Once disconnected, the reader thread should exit at some point. A SocketException,
                //   ObjectDisposedException, or some other kind of Exception should occur, and cause
                //   the thread to exit.
                // - Do not set KeepReading to false here; that should only be set by Disconnect().
                //   If KeepReading is set to true, it means we did not want the connection to go down,
                //   so it will trigger our reconnection logic.  However, if KeepReading it set to false,
                //   it means that we WANTED the connection to go down.

                // - Wait for the reader thread to exit.
                this.readerThread.Join();
                this.readerThread = null; // REMEMBER TO SET THIS TO NULL!!!  Connect() requires this to be null.

                this.OnReadLine(
                    new DisconnectedEventArgs
                    {
                        Server = this.Config.Server,
                        Protocol = ChaskisEventProtocol.IRC
                    }.ToXml()
                );

                // We sent our disconnect complete event, we are no longer connected.
                this.IsConnected = false;
            }
        }

        /// <summary>
        /// Disconnects and cleans up everything.
        /// </summary>
        public void Dispose()
        {
            // Stop all scheduled events.  This will prevent any more events
            // from being queued from this perspective.  This should be called
            // regardless if we are connected or not.
            this.eventScheduler.Dispose();

            if( IsConnected != false )
            {
                StaticLogger.Log.WriteLine( "Disconnecting..." );

                // - Next, prevent the reader thread from adding more events.
                //   Set KeepReading to false.  The abort logic in the Reader
                //   thread depends on this.  This will also prevent any more
                //   events being posted to the string parsing queue.
                this.KeepReading = false;

                // - We could be in the reconnecting state.  Trigger that thread to awaken
                //   if its sleeping between reconnects.
                //   Since KeepReading is set to false, the reconnection thread wil exit.
                this.reconnectAbortEvent.Set();

                // Drain our writer queue before disconnecting so everything that needs to go out goes out.
                {
                    ManualResetEvent doneEvent = new ManualResetEvent( false );
                    this.macWriter.BeginInvoke( () => doneEvent.Set() );

                    // Wait for our last event to execute before leaving.
                    doneEvent.WaitOne();
                    this.macWriter.Dispose(); // Dispose now so no more events get executed.
                }

                this.DisconnectHelper();

                StaticLogger.Log.WriteLine( "Disconnect Complete." );
            }

            this.watchDog.Dispose();
            this.connection.Dispose();
            this.macWriter.Dispose();
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
        public int ScheduleRecurringEvent( TimeSpan interval, Action<IIrcWriter> action, bool startRightAway = true )
        {
            Action<IIrcWriter> theAction = action;
            return this.eventScheduler.ScheduleRecurringEvent(
                interval,
                delegate ()
                {
                    this.parsingQueue.BeginInvoke( () => DoScheduledAction( "Recurring Scheduled Event", theAction ) );
                },
                startRightAway
            );
        }

        public void StartEvent( int id )
        {
            this.eventScheduler.StartEvent( id );
        }

        public void StopEvent( int id )
        {
            this.eventScheduler.StopEvent( id );
        }

        public void DisposeEvent( int id )
        {
            this.eventScheduler.DisposeEvent( id );
        }

        private void DoScheduledAction( string context, Action<IIrcWriter> action )
        {
            try
            {
                action( this );
            }
            catch( Exception e )
            {
                throw new EventHandlerException( context, e );
            }
        }

        /// <summary>
        /// Adds the given Chaskis to the event queue.
        /// </summary>
        public void SendInterPluginEvent( InterPluginEvent e )
        {
            string s = e.ToXml();
            this.OnReadLine( s );
        }

        private void OnReadLine( string s )
        {
            this.ReadEvent?.Invoke( s );
        }

        /// <summary>
        /// Issue #27
        /// Handle pings and pongs on the reader thread, NOT the string parsing thread.
        /// This is so if the string parsing thread hangs, we do not end up in a
        /// connection loop if our watchdog fails.
        /// </summary>
        /// <remarks>
        /// Not contained within <see cref="OnReadLine(string)"/> so there isn't
        /// a performance cost when we call <see cref="OnReadLine(string)"/> outside
        /// of the <see cref="ReaderThread"/>.
        /// </remarks>
        private void ResolvePingsAndPongs( string s )
        {
            HandlerArgs args = new HandlerArgs
            {
                BlackListedChannels = null,
                IrcConfig = this.Config,
                IrcWriter = this,
                Line = s,
            };

            this.pingHandler.HandleEvent( args );
            this.pongHandler.HandleEvent( args );
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
                        string s = this.connection.ReadLine();
                        if( ( string.IsNullOrWhiteSpace( s ) == false ) && ( string.IsNullOrEmpty( s ) == false ) )
                        {
                            // If KeepReading is set to false, we want this thread to exit.
                            // Do nothing.
                            if( this.KeepReading )
                            {
                                this.ResolvePingsAndPongs( s );
                                this.OnReadLine( s );
                            }
                        }
                    }
                    catch( Exception err )
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
                } // End While
            }
            catch( Exception err )
            {
                // Fatal Unexpected exception occurred.
                StaticLogger.Log.ErrorWriteLine(
                    "FATAL ERROR: IRC Reader Thread caught unexpected exception!" + Environment.NewLine + err
                );
                StaticLogger.Log.ErrorWriteLine(
                    "If this was because of a watchdog timeout, stand by..."
                );
            }
            finally
            {
                StaticLogger.Log.WriteLine( "ReaderThread Exiting" );
                this.OnReaderThreadExit?.Invoke();
            }
        }

        /// <summary>
        /// Attempts to restablish the IRC connection.
        /// </summary>
        private void AttemptReconnect()
        {
            this.DisconnectHelper();

            // Bad news is when we release the ircWriterLock when DisconnectHelper returns, 
            // any event that wants to write to the IRC Channel
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

                    this.OnReadLine(
                        new ReconnectingEventArgs
                        {
                            Protocol = ChaskisEventProtocol.IRC,
                            Server = this.Config.Server
                        }.ToXml()
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
                        "Reconnect failed, got exception trying again." + Environment.NewLine + e
                    );
                }
            } // End While
        }

        private void EventQueue_OnError( Exception err )
        {
            StringWriter errorMessage = new StringWriter();

            errorMessage.WriteLine( "***************" );
            errorMessage.WriteLine( "Caught Exception in Event Queue (" +  Thread.CurrentThread.Name + "):" );
            errorMessage.WriteLine( err.Message );
            errorMessage.WriteLine( err.StackTrace );
            errorMessage.WriteLine( "***************" );

            StaticLogger.Log.ErrorWriteLine( errorMessage.ToString() );
        }

        private void Watchdog_OnFailure()
        {
            this.OnReadLine(
                new WatchdogFailedEventArgs
                {
                    Protocol = ChaskisEventProtocol.IRC,
                    Server = this.Config.Server
                }.ToXml()
            );

            // This should be the only spot where we attempt a reconnect.
            this.AttemptReconnect();
        }
    }
}