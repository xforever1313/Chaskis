//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using ChaskisCore;

namespace Chaskis.Plugins.WelcomeBotPlugin
{
    /// <summary>
    /// This plugin welcomes a user to the channel when he or she joins.
    /// </summary>
    [ChaskisPlugin( "welcomebot" )]
    public class WelcomeBot : IPlugin
    {
        // -------- Fields --------

        /// <summary>
        /// Whether or not the plugin is loaded an ready to go.
        /// </summary>
        private bool isLoaded;

        /// <summary>
        /// The handlers to return to the main bot.
        /// </summary>
        private List<IIrcHandler> handlers;

        // -------- Constructor -------

        /// <summary>
        /// Constructor.
        /// </summary>
        public WelcomeBot()
        {
            isLoaded = false;
            this.handlers = new List<IIrcHandler>();
        }

        /// <summary>
        /// Returns the source code location of this plugin.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/WelcomeBotPlugin";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin.</param>
        /// <param name="ircConfig">The IRC config to use.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
            if( this.isLoaded == false )
            {
                this.handlers.Add( new JoinHandler( JoinMessage ) );
                this.handlers.Add( new PartHandler( PartMessage ) );
                this.isLoaded = true;
            }
        }

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown in Init() was not called prior to this being called.</exception>
        /// <returns>The list of handlers to awtch.</returns>
        public IList<IIrcHandler> GetHandlers()
        {
            if( isLoaded == false )
            {
                throw new InvalidOperationException(
                    "Welcome bot has not been initalized yet. Call Init() before calling this function."
                );
            }

            return this.handlers.AsReadOnly();
        }

        /// <summary>
        /// Tears down this plugin.
        /// </summary>
        public void Teardown()
        {
        }

        // ---- Handlers ----

        /// <summary>
        /// Ran when someone joins the channel.
        /// </summary>
        /// <param name="writer">The means to write to an IRC channel.</param>
        /// <param name="response">The command from the server.</param>
        private static void JoinMessage( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessageToUser(
                "Greetings " + response.RemoteUser + ", welcome to " + response.Channel + "!",
                response.RemoteUser
            );
            writer.SendCommandToChannel( response.RemoteUser + " has joined " + response.Channel );
        }

        /// <summary>
        /// Ran when someone parts the channel.
        /// </summary>
        /// <param name="writer">The means to write to an IRC channel.</param>
        /// <param name="response">The command from the server.</param>
        private static void PartMessage( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessageToUser(
                "Thanks for visiting " + response.Channel + "!  Please come back soon!",
                response.RemoteUser
            );
            writer.SendCommandToChannel(
                response.RemoteUser + " has left " + response.Channel
            );
        }
    }
}