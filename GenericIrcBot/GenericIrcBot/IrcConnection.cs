
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

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
        /// Used to mutex-protect keepReading.
        /// </summary>
        private object keepReadingObject;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public IrcConnection ( IIrcConfig config )
        {
            this.Config = new ReadOnlyIrcConfig( config );
            this.IsConnected = false;

            this.connection = null;
            this.ircWriter = null;
            this.ircReader = null;

            this.keepReadingObject = new object();
            this.KeepReading = false;
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
        /// Whether or not to keep reading.
        /// </summary>
        private bool KeepReading
        {
            get
            {
                lock ( this.keepReadingObject )
                {
                    return this.keepReading;
                }
            }

            set
            {
                lock ( this.keepReadingObject )
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
            if ( this.IsConnected == true )
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
            if ( string.IsNullOrEmpty( this.Config.Password ) == false )
            {
                this.ircWriter.WriteLine( "/msg nickserv identify {0}", this.Config.Password );
            }

            this.IsConnected = true;
        }

        /// <summary>
        /// Sends the given command to the channel.
        /// Thread-safe.
        /// Throws InvalidOperationException if not connected.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        public void SendCommand( string msg )
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

            lock ( this.ircWriter )
            {
                // PRIVMSG < msgtarget > < message >
                this.ircWriter.WriteLine( "PRIVMSG {0} :{1}", userNick, msg );
                this.ircWriter.Flush();
            }
        }

        /// <summary>
        /// Sends a pong to the given url.
        /// </summary>
        /// <param name="response">The response we need to send.</param>
        public void SendPong( string response )
        {
            lock ( this.ircWriter )
            {
                // PONG :response
                this.ircWriter.WriteLine( "PONG :{0}", response );
            }
        }

        /// <summary>
        /// Disconnected.
        /// No-Op if already disconnected.
        /// </summary>
        public void Disconnect()
        {
            if ( IsConnected != false )
            {
                this.KeepReading = false;
                // This will caluse the reader to close, and throw an IOException.
                // this will be caught in the thread, which will gracefully join.
                this.ircReader.Close();
                this.readerThread.Join();

                // Disconnect.
                this.ircWriter.Close();
                this.connection.Close();

                // Reset everything to null.
                this.ircWriter = null;
                this.ircReader = null;
                this.connection = null;

                // We are not connected.
                this.IsConnected = false;
            }
        }

        /// <summary>
        /// Cleans up everything.
        /// Calls close.
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
            while ( this.KeepReading )
            {
                try
                {
                    // ReadLine blocks until we call Close().
                    string s = this.ircReader.ReadLine();
                    if ( ( string.IsNullOrWhiteSpace( s ) == false ) && ( string.IsNullOrEmpty( s ) == false ) )
                    {
                        if ( this.ReadEvent != null )
                        {
                            ReadEvent( s );
                        }

                    }
                } catch ( IOException )
                {
                    // If keep reading is still true, the exception
                    // was from not aborting.  Throw.
                    if ( this.KeepReading )
                    {
                        throw;
                    }
                }
            }
        }
    }
}
