//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ChaskisCore;

namespace Chaskis.Plugins.KarmaBot
{
    [ChaskisPlugin( "karmabot" )]
    public class KarmaBot : IPlugin
    {
        // -------- Fields --------

        public const string VersionStr = "0.4.0";

        /// <summary>
        /// List of handlers.
        /// </summary>
        private List<IIrcHandler> handlers;

        /// <summary>
        /// The Karmabot config to use.
        /// </summary>
        private KarmaBotConfig config;

        /// <summary>
        /// The database to talk to.
        /// </summary>
        private KarmaBotDatabase dataBase;

        private IChaskisEventSender eventSender;

        private IChaskisEventCreator eventCreator;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public KarmaBot()
        {
            this.handlers = new List<IIrcHandler>();
        }

        /// <summary>
        /// Returns the source code location of this plugin.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/KarmaBot";
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
        /// About this plugin.
        /// </summary>
        public string About
        {
            get
            {
                return "I keep track of the karma of users and/or things.  Users of the channel can increase/decrease karma once per message.";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Inits this plugin.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            string karmaBotRoot = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "KarmaBot"
            );

            string dbPath = Path.Combine(
                karmaBotRoot,
                "karmabot.ldb"
            );

            string configPath = Path.Combine(
                karmaBotRoot,
                "KarmaBotConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.config = XmlLoader.LoadKarmaBotConfig( configPath );
            this.dataBase = new KarmaBotDatabase( dbPath );
            this.eventSender = initor.ChaskisEventSender;
            this.eventCreator = initor.ChaskisEventCreator;

            MessageHandler increaseHandler = new MessageHandler(
                this.config.IncreaseCommandRegex,
                HandleIncreaseCommand
            );

            MessageHandler decreaseCommand = new MessageHandler(
                this.config.DecreaseCommandRegex,
                HandleDecreaseCommand
            );

            MessageHandler queryCommand = new MessageHandler(
                this.config.QueryCommand,
                HandleQueryCommand
            );

            ChaskisEventHandler chaskisQuery = initor.ChaskisEventCreator.CreatePluginEventHandler(
                this.HandleChaskisQueryCommand
            );

            this.handlers.Add( increaseHandler );
            this.handlers.Add( decreaseCommand );
            this.handlers.Add( queryCommand );
            this.handlers.Add( chaskisQuery );
        }

        /// <summary>
        /// Handles the help command.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            string message = "@" + response.RemoteUser + ": ";
            if( args.Length == 0 )
            {
                message += "Append 'increase', 'decrease', or 'query' to the help message you just sent to get more information about each command.";
            }
            else if( args[0] == "increase" )
            {
                message += "To increase a user's karma, you must match this regex: " + this.config.IncreaseCommandRegex;
            }
            else if( args[0] == "decrease" )
            {
                message += "To decrease a user's karma, you must match this regex: " + this.config.DecreaseCommandRegex;
            }
            else if( args[0] == "query" )
            {
                message += "To query a user's karma, you must match this regex: " + this.config.QueryCommand;
            }
            else
            {
                message += "that is not a valid help command.  I can do 'increase', 'decrease', or 'query'";
            }

            writer.SendMessage(
                message,
                response.Channel
            );
        }

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The list of handlers to awtch.</returns>
        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }

        /// <summary>
        /// Tearsdown this plugin.
        /// </summary>
        public void Dispose()
        {
            this.dataBase?.Dispose();
        }

        // ---- Handlers ----

        /// <summary>
        /// Handles the increase command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private async void HandleIncreaseCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;
            string userName = match.Groups["name"].Value;
            int karma = await this.dataBase.IncreaseKarma( userName );

            writer.SendMessage( userName + " has had their karma increased to " + karma, response.Channel );
        }

        /// <summary>
        /// Handles the decrease command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private async void HandleDecreaseCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;
            string userName = match.Groups["name"].Value;
            int karma = await this.dataBase.DecreaseKarma( userName );

            writer.SendMessage( userName + " has had their karma decreased to " + karma, response.Channel );
        }

        /// <summary>
        /// Handles the query command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private async void HandleQueryCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;
            string userName = match.Groups["name"].Value;
            int karma = await this.dataBase.QueryKarma( userName );

            writer.SendMessage( userName + " has " + karma + " karma.", response.Channel );
        }

        private async void HandleChaskisQueryCommand( ChaskisEventHandlerLineActionArgs args )
        {
            Dictionary<string, string> responseArgs = new Dictionary<string, string>();
            if( args.EventArgs.ContainsKey( "ACTION" ) )
            {
                switch( args.EventArgs["ACTION"] )
                {
                    case "QUERY":
                        if( args.EventArgs.ContainsKey( "NAME" ) == false )
                        {
                            responseArgs["ERROR"] = "'NAME' argument missing";
                        }
                        else
                        {
                            string userName = args.EventArgs["NAME"];
                            int karma = await this.dataBase.QueryKarma( userName );
                            responseArgs["KARMA"] = karma.ToString();
                        }
                        break;

                    default:
                        responseArgs["ERROR"] = "NOT IMPLEMENTED YET";
                        return;
                }

                ChaskisEvent e = this.eventCreator.CreateTargetedEvent(
                    args.PluginName,
                    responseArgs,
                    args.PassThroughArgs
                );

                this.eventSender.SendChaskisEvent( e );
            }
        }
    }
}