//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using ChaskisCore;

namespace Chaskis.Plugins.WelcomeBot
{
    /// <summary>
    /// This plugin welcomes a user to the channel when he or she joins.
    /// </summary>
    [ChaskisPlugin( "welcomebot" )]
    public class WelcomeBot : IPlugin
    {
        // -------- Fields --------

        public const string VersionStr = "0.3.0";

        /// <summary>
        /// Whether or not the plugin is loaded an ready to go.
        /// </summary>
        private bool isLoaded;

        /// <summary>
        /// The handlers to return to the main bot.
        /// </summary>
        private List<IIrcHandler> handlers;

        private IChaskisEventCreator eventCreator;

        private IChaskisEventSender eventSender;

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

        /// <summary>
        /// The version of this plugin.
        /// </summary>
        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        /// <summary>
        /// What this plugin does.
        /// </summary>
        public string About
        {
            get
            {
                return "I welcome people to our channel when they join!";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            if( this.isLoaded == false )
            {
                this.eventCreator = initor.ChaskisEventCreator;
                this.eventSender = initor.ChaskisEventSender;

                ChaskisEventHandler karmaHandler = this.eventCreator.CreatePluginEventHandler(
                    "karmabot",
                    this.HandleKarmaQuery
                );

                this.handlers.Add( new JoinHandler( JoinMessage ) );
                this.handlers.Add( new PartHandler( PartMessage ) );
                this.handlers.Add( karmaHandler );
                this.isLoaded = true;
            }
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            writer.SendMessage(
                this.About,
                response.Channel
            );
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
        public void Dispose()
        {
        }

        // ---- Handlers ----

        /// <summary>
        /// Ran when someone joins the channel.
        /// </summary>
        /// <param name="writer">The means to write to an IRC channel.</param>
        /// <param name="response">The command from the server.</param>
        private void JoinMessage( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessage(
                "Greetings " + response.RemoteUser + ", welcome to " + response.Channel + "!",
                response.RemoteUser
            );
            writer.SendMessage(
                response.RemoteUser + " has joined " + response.Channel,
                response.Channel
            );

            ChaskisEvent e = this.eventCreator.CreateTargetedEvent(
                "karmabot",
                new Dictionary<string, string>()
                {
                    ["ACTION"] = "QUERY",
                    ["NAME"] = response.RemoteUser
                },
                new Dictionary<string, string>()
                {
                    ["CHANNEL"] = response.Channel,
                    ["NAME"] = response.RemoteUser
                }
            );

            this.eventSender.SendChaskisEvent( e );
        }

        /// <summary>
        /// Ran when someone parts the channel.
        /// </summary>
        /// <param name="writer">The means to write to an IRC channel.</param>
        /// <param name="response">The command from the server.</param>
        private void PartMessage( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessage(
                "Thanks for visiting " + response.Channel + "!  Please come back soon!",
                response.RemoteUser
            );
            writer.SendMessage(
                response.RemoteUser + " has left " + response.Channel,
                response.Channel
            );
        }

        private void HandleKarmaQuery( ChaskisEventHandlerLineActionArgs args )
        {
            if( args.PluginName == "karmabot" )
            {
                this.HandleKarmaBotResponse( args );
            }
        }

        private void HandleKarmaBotResponse( ChaskisEventHandlerLineActionArgs args )
        {
            if( args.EventArgs.ContainsKey( "ERROR" ) == false )
            {
                string channel = args.PassThroughArgs["CHANNEL"];
                string karma = args.EventArgs["KARMA"];
                string user = args.PassThroughArgs["NAME"];

                args.IrcWriter.SendMessage(
                    "User " + user + " has " + karma + " karma",
                    channel
                );
            }
        }
    }
}