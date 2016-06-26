
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using SethCS.Basic;
using System.Collections.Generic;
using SethCS.Exceptions;

namespace GenericIrcBot
{
    public class IrcBot : IDisposable
    {
        // -------- Fields --------
        
        /// <summary>
        /// Version in the form of a string.
        /// </summary>
        public const string VersionString = "0.0.1";

        /// <summary>
        /// Semantic Version of the bot.
        /// </summary>
        public static readonly SemanticVersion Version = SemanticVersion.Parse( VersionString );

        /// <summary>
        /// The irc config.
        /// </summary>
        private readonly IIrcConfig ircConfig;

        /// <summary>
        /// The line configs to use.
        /// </summary>
        private readonly IList<IIrcHandler> ircHandlers;

        /// <summary>
        /// The IRC Connection.
        /// </summary>
        private readonly IConnection ircConnection;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ircConfig">The irc config object to use.  This will be cloned after being passed in.</param>
        /// <param name="ircHandlers">The line configs to be used in this bot.</param>
        /// <param name="infoLogEvent">The event to do if we want to log information.</param>
        /// <param name="errorLogEvent">The event to do if an error occurrs.</param>
        public IrcBot( IIrcConfig ircConfig, IList<IIrcHandler> ircHandlers, Action<string> infoLogEvent, Action<string> errorLogEvent )
        {
            ArgumentChecker.IsNotNull( ircConfig, nameof( ircConfig ) );
            ArgumentChecker.IsNotNull( ircHandlers, nameof( ircHandlers ) );

            this.ircConfig = ircConfig.Clone();
            this.IrcConfig = new ReadOnlyIrcConfig( this.ircConfig );

            this.ircHandlers = ircHandlers;

            IrcConnection connection = new IrcConnection( ircConfig );
            connection.ReadEvent = delegate( string line )
            {
                foreach( IIrcHandler config in this.ircHandlers )
                {
                    config.HandleEvent( line, this.ircConfig, connection );
                }
            };
            connection.InfoLogEvent = infoLogEvent;
            connection.ErrorLogEvent = errorLogEvent;

            this.ircConnection = connection;
        }

        // -------- Properties --------

        /// <summary>
        /// Read-only reference to the irc config object.
        /// </summary>
        public IIrcConfig IrcConfig { get; private set; }

        // -------- Functions -------

        /// <summary>
        /// Starts the IRC Connection.  No-op if already started.
        /// </summary>
        public void Start()
        {
            if( this.ircConnection.IsConnected == false )
            {
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
                this.ircConnection.Disconnect();
            }
        }
    }
}
