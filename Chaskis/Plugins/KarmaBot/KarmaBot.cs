//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Chaskis.Core;

namespace Chaskis.Plugins.KarmaBot
{
    [ChaskisPlugin( "karmabot" )]
    public class KarmaBot : IPlugin
    {
        // -------- Fields --------

        internal const string VersionStr = "0.4.3";

        /// <summary>
        /// List of handlers.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// The Karmabot config to use.
        /// </summary>
        private KarmaBotConfig config;

        /// <summary>
        /// The database to talk to.
        /// </summary>
        private KarmaBotDatabase dataBase;

        private IInterPluginEventSender eventSender;

        private IInterPluginEventCreator eventCreator;

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

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = this.config.IncreaseCommandRegex,
                    LineAction = HandleIncreaseCommand
                };

                MessageHandler increaseHandler = new MessageHandler(
                    msgConfig
                );
                this.handlers.Add( increaseHandler );
            }

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = this.config.DecreaseCommandRegex,
                    LineAction = HandleDecreaseCommand
                };

                MessageHandler decreaseCommand = new MessageHandler(
                    msgConfig
                );
                this.handlers.Add( decreaseCommand );
            }

            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = this.config.QueryCommand,
                    LineAction = HandleQueryCommand
                };

                MessageHandler queryCommand = new MessageHandler(
                    msgConfig
                );
                this.handlers.Add( queryCommand );
            }

            {
                InterPluginEventHandler chaskisQuery = initor.ChaskisEventCreator.CreatePluginEventHandler(
                    this.HandleChaskisQueryCommand
                );
                this.handlers.Add( chaskisQuery );
            }
        }

        /// <summary>
        /// Handles the help command.
        /// </summary>
        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            string message = "@" + msgArgs.User + ": ";
            if( helpArgs.Length == 0 )
            {
                message += "Append 'increase', 'decrease', or 'query' to the help message you just sent to get more information about each command.";
            }
            else if( helpArgs[0] == "increase" )
            {
                message += "To increase a user's karma, you must match this regex: " + this.config.IncreaseCommandRegex;
            }
            else if( helpArgs[0] == "decrease" )
            {
                message += "To decrease a user's karma, you must match this regex: " + this.config.DecreaseCommandRegex;
            }
            else if( helpArgs[0] == "query" )
            {
                message += "To query a user's karma, you must match this regex: " + this.config.QueryCommand;
            }
            else
            {
                message += "that is not a valid help command.  I can do 'increase', 'decrease', or 'query'";
            }

            msgArgs.Writer.SendMessage(
                message,
                msgArgs.Channel
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
        private async void HandleIncreaseCommand( MessageHandlerArgs args )
        {
            Match match = args.Match;
            string userName = match.Groups["name"].Value;
            int karma = await this.dataBase.IncreaseKarma( userName );

            args.Writer.SendMessage( userName + " has had their karma increased to " + karma, args.Channel );
        }

        /// <summary>
        /// Handles the decrease command.
        /// </summary>
        private async void HandleDecreaseCommand( MessageHandlerArgs args )
        {
            Match match = args.Match;
            string userName = match.Groups["name"].Value;
            int karma = await this.dataBase.DecreaseKarma( userName );

            args.Writer.SendMessage( userName + " has had their karma decreased to " + karma, args.Channel );
        }

        /// <summary>
        /// Handles the query command.
        /// </summary>
        private async void HandleQueryCommand( MessageHandlerArgs args )
        {
            Match match = args.Match;
            string userName = match.Groups["name"].Value;
            int karma = await this.dataBase.QueryKarma( userName );

            args.Writer.SendMessage( userName + " has " + karma + " karma.", args.Channel );
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

                InterPluginEvent e = this.eventCreator.CreateTargetedEvent(
                    args.PluginName,
                    responseArgs,
                    args.PassThroughArgs
                );

                this.eventSender.SendInterPluginEvent( e );
            }
        }
    }
}