//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
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
        public const string VersionString = "0.2.0";

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
        /// The handlers
        /// string is plugin name, value is the list of corresponding handlers. 
        /// </summary>
        IReadOnlyDictionary<string, IHandlerConfig> ircHandlers;

        /// <summary>
        /// The IRC Connection.
        /// </summary>
        private readonly IrcConnection ircConnection;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ircConfig">The irc config object to use.  This will be cloned after being passed in.</param>
        /// <param name="ircHandlers">The line configs to be used in this bot.</param>
        /// <param name="infoLogEvent">The event to do if we want to log information.</param>
        /// <param name="errorLogEvent">The event to do if an error occurrs.</param>
        public IrcBot( IIrcConfig ircConfig )
        {
            ArgumentChecker.IsNotNull( ircConfig, nameof( ircConfig ) );

            this.ircConfig = ircConfig.Clone();
            this.IrcConfig = new ReadOnlyIrcConfig( this.ircConfig );

            IrcConnection connection = new IrcConnection( ircConfig );
            this.ircConnection = connection;
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

        // -------- Functions -------

        /// <summary>
        /// Inits the IRC bot.
        /// </summary>
        /// <param name="ircHandlers">IRC handlers.  Key is the plugin name, value is the corresponding handlers.</param>
        public void Init( IReadOnlyDictionary<string, IHandlerConfig> ircHandlers )
        {
            ArgumentChecker.IsNotNull( ircHandlers, nameof( ircHandlers ) );

            this.ircHandlers = ircHandlers;

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
                    this.ircConnection.SendPart( this.IrcConfig.QuitMessage );
                }
                finally
                {
                    this.ircConnection.Disconnect();
                    this.ircConnection.ReadEvent -= this.IrcConnection_ReadEvent;
                }
            }
        }

        private void IrcConnection_ReadEvent( string line )
        {
            HandlerArgs args = new HandlerArgs();
            args.IrcConfig = this.ircConfig;
            args.IrcWriter = this.ircConnection;
            args.Line = line;

            foreach( KeyValuePair<string, IHandlerConfig> handlers in this.ircHandlers )
            {
                args.BlackListedChannels = handlers.Value.BlackListedChannels;
                foreach( IIrcHandler handler in handlers.Value.Handlers )
                {
                    handler.HandleEvent( args );
                }
            }
        }
    }
}