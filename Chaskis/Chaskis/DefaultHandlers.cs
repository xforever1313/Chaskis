//
//          Copyright Seth Hendrick 2016, 2017, 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Chaskis.Core;

namespace Chaskis.Cli
{
    [ChaskisPlugin( DefaultPluginName ) ]
    public class DefaultHandlers : IHandlerConfig, IPlugin
    {
        // ---------------- Fields ----------------

        internal const string DefaultPluginName = "chaskis";

        private const string defaultHelpMessage = "Default Commands: 'plugins', 'admins', 'source [plugin]', 'version [plugin]', 'about [plugin]', 'help [plugin] [arg1] [arg2]...'";

        /// <summary>
        /// The IRC config to use.
        /// </summary>
        private IIrcConfig ircConfig;

        /// <summary>
        /// The plugins we are using.
        /// </summary>
        private IDictionary<string, PluginConfig> plugins;

        private IInterPluginEventSender chaskisEventSender;
        private IInterPluginEventCreator chaskisEventCreator;

        /// <summary>
        /// IRC handlers we will be using.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// The response when a user asks for the plugin list.
        /// </summary>
        private string pluginListResponse;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public DefaultHandlers()
        {
            this.handlers = new List<IIrcHandler>();
            this.Handlers = this.handlers.AsReadOnly();
            this.BlackListedChannels = new List<string>().AsReadOnly();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Read-only list of handlers.
        /// </summary>
        public IList<IIrcHandler> Handlers { get; private set; }

        /// <summary>
        /// Read-only empty list of black-listed channels.
        /// There are none here.
        /// </summary>
        public IList<string> BlackListedChannels { get; private set; }

        public string SourceCodeLocation { get { return "https://github.com/xforever1313/Chaskis/"; } }

        public string Version { get { return Chaskis.VersionStr; } }

        public string About
        {
            get
            {
                return "I am running chaskis, a plugin-based IRC framework written in C#.  Released under the Boost Software License V1.0: https://github.com/xforever1313/Chaskis/blob/master/LICENSE_1_0.txt";
            }
        }

        // ---------------- Functions ----------------

        public void Init( PluginInitor pluginInit )
        {
            this.ircConfig = pluginInit.IrcConfig;
            this.chaskisEventCreator = pluginInit.ChaskisEventCreator;
            this.chaskisEventSender = pluginInit.ChaskisEventSender;

            this.AddPluginListHandler();
            this.AddSourceHandler();
            this.AddVersionHandler();
            this.AddAboutHandler();
            this.AddHelpHandler();
            this.AddAdminHandler();
            this.AddDebugHandlers();
            this.AddCtcpPingHandler();
        }

        public void Init_Stage2( IDictionary<string, PluginConfig> plugins )
        {
            this.plugins = plugins;

            StringBuilder builder = new StringBuilder();
            builder.Append( "List of plugins I am running: " );
            foreach( string pluginName in this.plugins.Keys )
            {
                builder.Append( pluginName.ToLower() );
                builder.Append( " " );
            }

            this.pluginListResponse = builder.ToString();
        }

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            if( helpArgs.Length == 0 )
            {
                // Print default message and return.
                msgArgs.Writer.SendMessage(
                    defaultHelpMessage,
                    msgArgs.Channel
                );
                return;
            }

            string message;
            switch( helpArgs[0] )
            {
                case "plugins":
                case "pluginlist":
                    message = "Gets the list of plugins running.";
                    break;

                case "source":
                    message = "Gets the source code URL of the given plugin.";
                    break;

                case "version":
                    message = "Gets the version of the given plugin.";
                    break;

                case "about":
                    message = "Gets information about the given plugin.";
                    break;

                case "help":
                    message = "Gets help information about the given plugin.";
                    break;

                case "admins":
                    message = "Shows the list of people who are considered admins.";
                    break;

                default:
                    message = "Invalid Command!";
                    break;
            }

            msgArgs.Writer.SendMessage(
                message,
                msgArgs.Channel
            );
        }

        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }

        public void Dispose()
        {
            // Nothing to do.
        }

        // -------- Handlers ---------

        // ---- Plugin List Handler ----

        private void AddPluginListHandler()
        {
            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = "^[!@]" + this.ircConfig.Nick + @":?\s+plugin(s|\s*list)",
                LineAction = this.HandlePluginListCommand
            };

            MessageHandler pluginListHandler = new MessageHandler(
                config
            );

            this.handlers.Add( pluginListHandler );
        }

        /// <summary>
        /// Handles the "list plugin" command.
        /// </summary>
        private void HandlePluginListCommand( MessageHandlerArgs args )
        {
            args.Writer.SendMessage( this.pluginListResponse, args.Channel );
        }

        // ---- Source Command Handler ----

        private void AddSourceHandler()
        {
            string sourceCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+source(\s+(?<pluginName>\w+))?";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = sourceCommand,
                LineAction = this.HandleSourceCommand
            };

            MessageHandler sourceHandler = new MessageHandler(
                config
            );

            this.handlers.Add( sourceHandler );
        }

        /// <summary>
        /// Handles the source command, which returns the source code of the plugin.
        /// </summary>
        private void HandleSourceCommand( MessageHandlerArgs args )
        {
            Match match = args.Match;

            string pluginName = match.Groups["pluginName"].Value.ToLower();

            if( string.IsNullOrEmpty( pluginName ) )
            {
                pluginName = DefaultPluginName;
            }

            if( this.plugins.ContainsKey( pluginName ) )
            {
                string msg = "Source of '" + pluginName + "': " + this.plugins[pluginName].Plugin.SourceCodeLocation;
                args.Writer.SendMessage( msg, args.Channel );
            }
            else
            {
                args.Writer.SendMessage( "'" + pluginName + "' is not a plugin I have loaded...", args.Channel );
            }
        }

        // ---- Version Command Handler ----

        private void AddVersionHandler()
        {
            string versionCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+version(\s+(?<pluginName>\w+))?";

            MessageHandlerConfig messageHandlerConfig = new MessageHandlerConfig
            {
                LineRegex = versionCommand,
                LineAction = this.HandleVersionCommand
            };

            MessageHandler versionHandler = new MessageHandler(
                messageHandlerConfig
            );

            this.handlers.Add( versionHandler );

            InterPluginEventHandler chaskisHandler = this.chaskisEventCreator.CreatePluginEventHandler(
                this.HandleChaskisVersionCommand
            );

            this.handlers.Add( chaskisHandler );
        }

        /// <summary>
        /// Handles the version command, which returns the source code of the plugin.
        /// </summary>
        private void HandleVersionCommand( MessageHandlerArgs args )
        {
            Match match = args.Match;

            string pluginName = match.Groups["pluginName"].Value.ToLower();

            if( string.IsNullOrEmpty( pluginName ) )
            {
                pluginName = DefaultPluginName;
            }

            if( this.plugins.ContainsKey( pluginName ) )
            {
                string msg = "Version of '" + pluginName + "': " + this.plugins[pluginName].Plugin.Version;
                args.Writer.SendMessage( msg, args.Channel );
            }
            else
            {
                args.Writer.SendMessage( "'" + pluginName + "' is not a plugin I have loaded...", args.Channel );
            }
        }

        private void HandleChaskisVersionCommand( ChaskisEventHandlerLineActionArgs args )
        {
            if( args.EventArgs.ContainsKey( "QUERY" ) && args.EventArgs["QUERY"] == "VERSION" )
            {
                Dictionary<string, string> responseArgs = new Dictionary<string, string>();
                if( args.EventArgs.ContainsKey( "PLUGIN" ) )
                {
                    string pluginName = args.EventArgs["PLUGIN"];
                    if( this.plugins.ContainsKey( pluginName ) )
                    {
                        responseArgs["VERSION"] = this.plugins[pluginName].Plugin.Version;
                    }
                    else
                    {
                        responseArgs["ERROR"] = "Plugin Name Not Found";
                    }
                }
                else
                {
                    responseArgs["ERROR"] = "Need Plugin Key";
                }

                InterPluginEvent responseEvent = this.chaskisEventCreator.CreateTargetedEvent(
                    args.PluginName,
                    responseArgs,
                    args.PassThroughArgs
                );

                this.chaskisEventSender.SendInterPluginEvent( responseEvent );
            }
        }

        // ---- About Command Handler ----

        private void AddAboutHandler()
        {
            string aboutCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+about(\s+(?<pluginName>\w+))?";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = aboutCommand,
                LineAction = this.HandleAboutCommand
            };

            MessageHandler aboutHandler = new MessageHandler(
                config
            );

            this.handlers.Add( aboutHandler );
        }

        /// <summary>
        /// Handles the version command, which returns the source code of the plugin.
        /// </summary>
        private void HandleAboutCommand( MessageHandlerArgs args )
        {
            Match match = args.Match;

            string pluginName = match.Groups["pluginName"].Value.ToLower();
            if( string.IsNullOrEmpty( pluginName ) )
            {
                pluginName = DefaultPluginName;
            }

            if( this.plugins.ContainsKey( pluginName ) )
            {
                string msg = "About '" + pluginName + "': " + this.plugins[pluginName].Plugin.About;
                args.Writer.SendMessage( msg, args.Channel );
            }
            else
            {
                args.Writer.SendMessage( "'" + pluginName + "' is not a plugin I have loaded...", args.Channel );
            }
        }

        // ---- Admin Command Handler ----
        private void AddAdminHandler()
        {
            string adminCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+admins";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = adminCommand,
                LineAction = this.HandleAdminCommand
            };

            MessageHandler helpHandler = new MessageHandler(
                config
            );

            this.handlers.Add( helpHandler );
        }

        /// <summary>
        /// Handles the version command, which returns the source code of the plugin.
        /// </summary>
        private void HandleAdminCommand( MessageHandlerArgs args )
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( "People who are admins for me: " );

            foreach( string name in this.ircConfig.Admins )
            {
                builder.Append( name + " " );
            }

            args.Writer.SendMessage(
                builder.ToString(),
                args.Channel
            );
        }

        // ---- Help Command Handler ----

        private void AddHelpHandler()
        {
            string helpCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+help(\s+(?<args>.+))?";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = helpCommand,
                LineAction = this.HandleHelpCommand
            };

            MessageHandler helpHandler = new MessageHandler(
                config
            );

            this.handlers.Add( helpHandler );
        }

        /// <summary>
        /// Handles the version command, which returns the source code of the plugin.
        /// </summary>
        private void HandleHelpCommand( MessageHandlerArgs args )
        {
            Match match = args.Match;

            string argsStr = match.Groups["args"].Value.ToLower();

            if( string.IsNullOrEmpty( argsStr ) )
            {
                // Print default message and return.
                args.Writer.SendMessage(
                    defaultHelpMessage,
                    args.Channel
                );
                return;
            }

            argsStr = Regex.Replace( argsStr, @"\s+", " " ); // Strip multiple white spaces.
            List<string> helpArgs = argsStr.Split( ' ' ).ToList();

            // If the first argument is not a valid plugin name, then the user
            // must be trying to query the default plugin.
            // If the first argument IS a valid plugin name, then the user wants
            // the query the help of that specific plugin.
            string pluginName;
            if( this.plugins.ContainsKey( helpArgs[0] ) == false )
            {
                pluginName = DefaultPluginName;
            }
            else
            {
                pluginName = helpArgs[0];
                helpArgs.RemoveAt( 0 );
            }

            // Handle the help command for the plugin
            this.plugins[pluginName].Plugin.HandleHelp( args, helpArgs.ToArray() );
        }

        // ---- Debug Handlers ----

        private void AddDebugHandlers()
        {
            string debugVerbosityCommand = @"^[!@]" + this.ircConfig.Nick + @":?\s+debug\s+verbosity\s+(?<plugin>\S+)\s+(?<verbose>\d+)";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = debugVerbosityCommand,
                LineAction = this.HandleDebugVerbosityCommand,
                ResponseOption = ResponseOptions.PmsOnly // Debug commands will only be with private messages.
            };

            MessageHandler handler = new MessageHandler(
                config
            );

            this.handlers.Add( handler );
        }

        /// <summary>
        /// Handles the debug verbosity command.
        /// </summary>
        private void HandleDebugVerbosityCommand( MessageHandlerArgs args )
        {
            if( this.ircConfig.Admins.Contains( args.User ) )
            {
                string pluginName = args.Match.Groups["plugin"].Value;
                string verboseLevel = args.Match.Groups["verbose"].Value;

                string message;
                if ( this.plugins.ContainsKey( pluginName ) == false )
                {
                    message = string.Format(
                        "'{0}' is not a plugin that is activated.",
                        pluginName
                    );
                }
                else if ( int.TryParse( verboseLevel, out int level ) == false )
                {
                    message = string.Format(
                        "'{0}' is an invalid integer.",
                        level
                    );
                }
                else
                {
                    this.plugins[pluginName].Log.Verbosity = level;
                    message = string.Format(
                        "'{0}' log verbosity has been set to '{1}'",
                        pluginName,
                        level
                    );
                }

                args.Writer.SendMessage(
                    message,
                    args.Channel
                );
            }
            // Otherwise, quietly ignore...
        }

        private void AddCtcpPingHandler()
        {
            CtcpPingHandlerConfig config = new CtcpPingHandlerConfig
            {
                LineRegex = ".+",
                LineAction = this.HandlerCtcpPingHandler,
                ResponseOption = ResponseOptions.PmsOnly
            };

            CtcpPingHandler handler = new CtcpPingHandler( config );
            this.handlers.Add( handler );
        }

        private void HandlerCtcpPingHandler( CtcpPingHandlerArgs args )
        {
            args.Writer.SendCtcpPong(
                args.Message,
                args.Channel
            );
        }
    }
}
