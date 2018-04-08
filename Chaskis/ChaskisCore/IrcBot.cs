//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using SethCS.Basic;
using SethCS.Exceptions;

namespace ChaskisCore
{
    public class IrcBot : IDisposable
    {
        // -------- Fields --------

        /// <summary>
        /// Version in the form of a string.
        /// </summary>
        public const string VersionString = "0.6.1";

        /// <summary>
        /// Copyright information.
        /// </summary>
        public const string CopyRight = "Copyright © Seth Hendrick 2016-2017";

        /// <summary>
        /// Semantic Version of the bot.
        /// </summary>
        public static readonly SemanticVersion Version = SemanticVersion.Parse( VersionString );

        /// <summary>
        /// The irc config.
        /// </summary>
        private readonly IIrcConfig ircConfig;

        /// <summary>
        /// The IRC Connection.
        /// </summary>
        private readonly IrcConnection ircConnection;

        /// <summary>
        /// Reference to the global parsing queue.
        /// </summary>
        private INonDisposableStringParsingQueue parsingQueue;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ircConfig">The irc config object to use.  This will be cloned after being passed in.</param>
        /// <param name="parsingQueue">The global parsing queue we are using.</param>
        public IrcBot( IIrcConfig ircConfig, INonDisposableStringParsingQueue parsingQueue )
        {
            ArgumentChecker.IsNotNull( ircConfig, nameof( ircConfig ) );
            ArgumentChecker.IsNotNull( parsingQueue, nameof( parsingQueue ) );

            this.ircConfig = ircConfig.Clone();
            this.IrcConfig = new ReadOnlyIrcConfig( this.ircConfig );

            IrcConnection connection = new IrcConnection( ircConfig, parsingQueue );
            this.ircConnection = connection;

            this.parsingQueue = parsingQueue;
        }

        // -------- Properties --------

        /// <summary>
        /// Read-only reference to the irc config object.
        /// </summary>
        public IIrcConfig IrcConfig { get; private set; }

        /// <summary>
        /// Access to our scheduler.
        /// </summary>
        public IChaskisEventScheduler Scheduler => this.ircConnection;

        /// <summary>
        /// Access to our chaskis event sender.
        /// </summary>
        public IChaskisEventSender ChaskisEventSender => this.ircConnection;

        // -------- Functions -------

        /// <summary>
        /// Inits the IRC bot.
        /// </summary>
        /// <param name="ircHandlers">IRC handlers.  Key is the plugin name, value is the corresponding handlers.</param>
        public void Init()
        {
            this.ircConnection.Init();
        }

        /// <summary>
        /// Starts the IRC Connection.  No-op if already started.
        /// </summary>
        public void Start()
        {
            if( this.ircConnection.IsConnected == false )
            {
                this.ircConnection.ReadEvent += this.IrcConnection_ReadEvent;
                this.ircConnection.Connect();
            }
        }

        /// <summary>
        /// Stops the IRC Connection.
        /// </summary>
        public void Dispose()
        {
            if( this.ircConnection.IsConnected )
            {
                try
                {
                    // Stop reacting to strings from the server, we don't care any more.
                    this.ircConnection.ReadEvent -= this.IrcConnection_ReadEvent;

                    foreach( string channel in this.ircConfig.Channels )
                    {
                        this.ircConnection.SendPart( this.IrcConfig.QuitMessage, channel );
                    }

                    this.parsingQueue.WaitForAllEventsToExecute();
                }
                finally
                {
                    this.ircConnection.Disconnect();
                }
            }
        }

        private void IrcConnection_ReadEvent( string line )
        {
            HandlerArgs args = new HandlerArgs();
            args.IrcConfig = this.ircConfig;
            args.IrcWriter = this.ircConnection;
            args.Line = line;

            this.parsingQueue.ParseAndRunEvent( args );
        }
    }
}