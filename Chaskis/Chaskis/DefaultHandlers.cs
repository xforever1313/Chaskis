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
using ChaskisCore;

namespace Chaskis
{
    [ChaskisPlugin( DefaultPluginName ) ]
    public class DefaultHandlers : IHandlerConfig, IPlugin
    {
        // ---------------- Fields ----------------

        public const string DefaultPluginName = "chaskis";

        private const string defaultHelpMessage = "Default Commands: 'plugins', 'admins', 'source [plugin]', 'version [plugin]', 'about [plugin]', 'help [plugin] [arg1] [arg2]...'";

        /// <summary>
        /// The IRC config to use.
        /// </summary>
        private IIrcConfig ircConfig;

        /// <summary>
        /// The plugins we are using.
        /// </summary>
        private IDictionary<string, PluginConfig> plugins;

        private IChaskisEventSender chaskisEventSender;
        private IChaskisEventCreator chaskisEventCreator;

        /// <summary>
        /// IRC handlers we will be using.
        /// </summary>
        private List<IIrcHandler> handlers;

        /// <summary>
        /// The response when a user asks for the plugin list.
        /// </summary>
        private string pluginListResponse;

        // *** IMPORTANT ***
        // Make sure all the commands start with '^' for the regex.  Otherwise,
        // someone can do "hello world !mybot plugins" and we will print the plugins.

        /// <summary>
        /// The command for getting the source of a plugin.
        /// </summary>
        private string sourceCommand;

        /// <summary>
        /// The command for getting the version of a plugin.
        /// </summary>
        private string versionCommand;

        /// <summary>
        /// The command for getting information about a help command.
        /// </summary>
        private string aboutCommand;

        /// <summary>
        /// The command for getting help.
        /// </summary>
        private string helpCommand;

        /// <summary>
        /// The command for getting info on who are admins.
        /// </summary>
        private string adminCommand;

        /// <summary>
        /// The command for setting the verbosity of a plugin's output.
        /// </summary>
        private string debugVerbosityCommand;

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
                return "I am running chaskis, a plugin-based IRC framework written in C#.  Released under the Boost Software License V1.0 http://www.boost.org/LICENSE_1_0.txt.";
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

            // Must always check for pings.
            this.handlers.Add( new PingHandler() );

            // Must always handle pongs.
            this.handlers.Add( new PongHandler() );
        }

        public void Init_Stage2( IDictionary<string, PluginConfig> plugins )
        {
            this.plugins = plugins;

            this.pluginListResponse = "List of plugins I am running: ";
            foreach( string pluginName in this.plugins.Keys )
            {
                this.pluginListResponse += pluginName.ToLower() + " ";
            }
        }

        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            if( args.Length == 0 )
            {
                // Print default message and return.
                writer.SendMessage(
                    defaultHelpMessage,
                    response.Channel
                );
                return;
            }

            string message;
            switch( args[0] )
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

            writer.SendMessage(
                message,
                response.Channel
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
            MessageHandler pluginListHandler = new MessageHandler(
                "^[!@]" + this.ircConfig.Nick + @":?\s+plugin(s|\s*list)",
                HandlePluginListCommand
            );

            this.handlers.Add( pluginListHandler );
        }

        /// <summary>
        /// Handles the "list plugin" command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandlePluginListCommand( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessage( this.pluginListResponse, response.Channel );
        }

        // ---- Source Command Handler ----

        private void AddSourceHandler()
        {
            this.sourceCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+source(\s+(?<pluginName>\w+))?";
            MessageHandler sourceHandler = new MessageHandler(
                this.sourceCommand,
                this.HandleSourceCommand
            );

            this.handlers.Add( sourceHandler );
        }

        /// <summary>
        /// Handles the source command, which returns the source code of the plugin.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleSourceCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;

            string pluginName = match.Groups["pluginName"].Value.ToLower();

            if( string.IsNullOrEmpty( pluginName ) )
            {
                pluginName = DefaultPluginName;
            }

            if( this.plugins.ContainsKey( pluginName ) )
            {
                string msg = "Source of '" + pluginName + "': " + this.plugins[pluginName].Plugin.SourceCodeLocation;
                writer.SendMessage( msg, response.Channel );
            }
            else
            {
                writer.SendMessage( "'" + pluginName + "' is not a plugin I have loaded...", response.Channel );
            }
        }

        // ---- Version Command Handler ----

        private void AddVersionHandler()
        {
            this.versionCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+version(\s+(?<pluginName>\w+))?";
            MessageHandler versionHandler = new MessageHandler(
                this.versionCommand,
                this.HandleVersionCommand
            );

            this.handlers.Add( versionHandler );

            ChaskisEventHandler chaskisHandler = this.chaskisEventCreator.CreatePluginEventHandler(
                @"QUERY=VERSION\s+PLUGIN=(?<pluginName>\w+)",
                this.HandleChaskisVersionCommand
            );

            this.handlers.Add( chaskisHandler );
        }

        /// <summary>
        /// Handles the version command, which returns the source code of the plugin.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleVersionCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;

            string pluginName = match.Groups["pluginName"].Value.ToLower();

            if( string.IsNullOrEmpty( pluginName ) )
            {
                pluginName = DefaultPluginName;
            }

            if( this.plugins.ContainsKey( pluginName ) )
            {
                string msg = "Version of '" + pluginName + "': " + this.plugins[pluginName].Plugin.Version;
                writer.SendMessage( msg, response.Channel );
            }
            else
            {
                writer.SendMessage( "'" + pluginName + "' is not a plugin I have loaded...", response.Channel );
            }
        }

        private void HandleChaskisVersionCommand( ChaskisEventHandlerLineActionArgs args )
        {
            string pluginName = args.Match.Groups["pluginName"].Value.ToLower();

            List<string> responseArgs = new List<string>();
            responseArgs.Add( "QUERY=VERSION" );
            responseArgs.Add( "PLUGIN=" + pluginName.ToUpper() );

            if( this.plugins.ContainsKey( pluginName ) == false )
            {
                responseArgs.Add( "ERROR=PLUGIN_NAME_NOT_FOUND" );
            }
            else
            {
                responseArgs.Add( "VERSION=" + this.plugins[pluginName].Plugin.Version );
            }

            ChaskisEvent responseEvent = this.chaskisEventCreator.CreateTargetedEvent(
                args.PluginName,
                responseArgs
            );

            this.chaskisEventSender.SendChaskisEvent( responseEvent );
        }

        // ---- About Command Handler ----

        private void AddAboutHandler()
        {
            this.aboutCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+about(\s+(?<pluginName>\w+))?";
            MessageHandler aboutHandler = new MessageHandler(
                this.aboutCommand,
                this.HandleAboutCommand
            );

            this.handlers.Add( aboutHandler );
        }

        /// <summary>
        /// Handles the version command, which returns the source code of the plugin.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleAboutCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;

            string pluginName = match.Groups["pluginName"].Value.ToLower();
            if( string.IsNullOrEmpty( pluginName ) )
            {
                pluginName = DefaultPluginName;
            }

            if( this.plugins.ContainsKey( pluginName ) )
            {
                string msg = "About '" + pluginName + "': " + this.plugins[pluginName].Plugin.About;
                writer.SendMessage( msg, response.Channel );
            }
            else
            {
                writer.SendMessage( "'" + pluginName + "' is not a plugin I have loaded...", response.Channel );
            }
        }

        // ---- Admin Command Handler ----
        private void AddAdminHandler()
        {
            this.adminCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+admins";

            MessageHandler helpHandler = new MessageHandler(
                this.adminCommand,
                this.HandleAdminCommand,
                0
            );

            this.handlers.Add( helpHandler );
        }

        /// <summary>
        /// Handles the version command, which returns the source code of the plugin.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleAdminCommand( IIrcWriter writer, IrcResponse response )
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( "People who are admins for me: " );

            foreach( string name in this.ircConfig.Admins )
            {
                builder.Append( name + " " );
            }

            writer.SendMessage(
                builder.ToString(),
                response.Channel
            );
        }

        // ---- Help Command Handler ----

        private void AddHelpHandler()
        {
            this.helpCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+help(\s+(?<args>.+))?";
            MessageHandler helpHandler = new MessageHandler(
                this.helpCommand,
                this.HandleHelpCommand,
                0
            );

            this.handlers.Add( helpHandler );
        }

        /// <summary>
        /// Handles the version command, which returns the source code of the plugin.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleHelpCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;

            string argsStr = match.Groups["args"].Value.ToLower();

            if( string.IsNullOrEmpty( argsStr ) )
            {
                // Print default message and return.
                writer.SendMessage(
                    defaultHelpMessage,
                    response.Channel
                );
                return;
            }

            argsStr = Regex.Replace( argsStr, @"\s+", " " ); // Strip multiple white spaces.
            List<string> args = argsStr.Split( ' ' ).ToList();

            // If the first argument is not a valid plugin name, then the user
            // must be trying to query the default plugin.
            // If the first argument IS a valid plugin name, then the user wants
            // the query the help of that specific plugin.
            string pluginName;
            if( this.plugins.ContainsKey( args[0] ) == false )
            {
                pluginName = DefaultPluginName;
            }
            else
            {
                pluginName = args[0];
                args.RemoveAt( 0 );
            }

            // Handle the help command for the plugin
            this.plugins[pluginName].Plugin.HandleHelp( writer, response, args.ToArray() );
        }

        // ---- Debug Handlers ----

        private void AddDebugHandlers()
        {
            this.debugVerbosityCommand = @"^[!@]" + this.ircConfig.Nick + @":?\s+debug\s+verbosity\s+(?<plugin>\S+)\s+(?<verbose>\d+)";

            MessageHandler handler = new MessageHandler(
                this.debugVerbosityCommand,
                this.HandleDebugVerbosityCommand,
                0,
                ResponseOptions.PmsOnly // Debug commands will only be with private messages.
            );

            this.handlers.Add( handler );
        }

        /// <summary>
        /// Handles the debug verbosity command.
        /// </summary>
        private void HandleDebugVerbosityCommand( IIrcWriter writer, IrcResponse response )
        {
            if( this.ircConfig.Admins.Contains( response.RemoteUser ) )
            {
                string pluginName = response.Match.Groups["plugin"].Value;
                string verboseLevel = response.Match.Groups["verbose"].Value;

                string message;
                int level = 0;

                if( this.plugins.ContainsKey( pluginName ) == false )
                {
                    message = string.Format(
                        "'{0}' is not a plugin that is activated.",
                        pluginName
                    );
                }
                else if( int.TryParse( verboseLevel, out level ) == false )
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

                writer.SendMessage(
                    message,
                    response.Channel
                );
            }
            // Otherwise, quietly ignore...
        }
    }
}
