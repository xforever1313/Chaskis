//
//          Copyright Seth Hendrick 2016-2017.
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
    public class DefaultHandlers
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// The IRC config to use.
        /// </summary>
        private IIrcConfig ircConfig;

        /// <summary>
        /// The plugins we are using.
        /// </summary>
        private IDictionary<string, IPlugin> plugins;

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

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public DefaultHandlers( IIrcConfig config, IDictionary<string, IPlugin> plugins )
        {
            this.ircConfig = config;
            this.plugins = plugins;
            this.handlers = new List<IIrcHandler>();
            this.Handlers = this.handlers.AsReadOnly();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Read-only list of handlers.
        /// </summary>
        public IList<IIrcHandler> Handlers { get; private set; }

        // ---------------- Functions ----------------

        public void Init()
        {
            this.AddPluginListHandler();
            this.AddSourceHandler();
            this.AddVersionHandler();
            this.AddAboutHandler();
            this.AddHelpHandler();
            this.AddAdminHandler();

            // Must always check for pings.
            this.handlers.Add( new PingHandler() );

            // Must always handle pongs.
            this.handlers.Add( new PongHandler() );
        }

        // -------- Handlers ---------

        // ---- Plugin List Handler ----

        private void AddPluginListHandler()
        {
            this.pluginListResponse = "List of plugins I am running: ";
            foreach( string pluginName in this.plugins.Keys )
            {
                this.pluginListResponse += pluginName.ToLower() + " ";
            }

            MessageHandler pluginListHandler = new MessageHandler(
                "^[!@]" + this.ircConfig.Nick + @":?\s+plugin(s|list)",
                HandlePluginListCommand,
                30
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
                this.HandleSourceCommand,
                3
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
            if( this.plugins.ContainsKey( pluginName ) )
            {
                string msg = "Source of the plugin '" + pluginName + "': " + this.plugins[pluginName].SourceCodeLocation;
                writer.SendMessage( msg, response.Channel );
            }
            else if( ( pluginName == "chaskis" ) || string.IsNullOrEmpty( pluginName ) )
            {
                string msg = "My source code is located here: https://github.com/xforever1313/Chaskis";
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
                this.HandleVersionCommand,
                3
            );

            this.handlers.Add( versionHandler );
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
            if( this.plugins.ContainsKey( pluginName ) )
            {
                string msg = "Version of the plugin '" + pluginName + "': " + this.plugins[pluginName].Version;
                writer.SendMessage( msg, response.Channel );
            }
            else if( ( pluginName == "chaskis" ) || string.IsNullOrEmpty( pluginName ) )
            {
                string msg = "I am running version Chaskis " + Chaskis.VersionStr;
                writer.SendMessage( msg, response.Channel );
            }
            else
            {
                writer.SendMessage( "'" + pluginName + "' is not a plugin I have loaded...", response.Channel );
            }
        }

        // ---- About Command Handler ----

        private void AddAboutHandler()
        {
            this.aboutCommand = "^[!@]" + this.ircConfig.Nick + @":?\s+about(\s+(?<pluginName>\w+))?";
            MessageHandler aboutHandler = new MessageHandler(
                this.aboutCommand,
                this.HandleAboutCommand,
                3
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
            if( this.plugins.ContainsKey( pluginName ) )
            {
                string msg = "About '" + pluginName + "': " + this.plugins[pluginName].About;
                writer.SendMessage( msg, response.Channel );
            }
            else if( ( pluginName == "chaskis" ) || string.IsNullOrEmpty( pluginName ) )
            {
                string msg = "I am running chaskis, a plugin-based IRC framework written in C#.  Released under the Boost Software License V1.0 http://www.boost.org/LICENSE_1_0.txt.";
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
            const string defaultMessage = "Default Commands: 'plugins', 'admins', 'source [plugin]', 'version [plugin]', 'about [plugin]', 'help [plugin] [arg1] [arg2]...'";

            Match match = response.Match;

            string argsStr = match.Groups["args"].Value.ToLower();

            if( string.IsNullOrEmpty( argsStr ) )
            {
                // Print default message and return.
                writer.SendMessage(
                    defaultMessage,
                    response.Channel
                );
                return;
            }

            argsStr = Regex.Replace( argsStr, @"\s+", " " ); // Strip multiple white spaces.
            List<string> args = argsStr.Split( ' ' ).ToList();

            if( this.plugins.ContainsKey( args[0] ) )
            {
                string pluginName = args[0];
                args.RemoveAt( 0 );

                // Handle the help command for the plugin
                this.plugins[pluginName].HandleHelp( writer, response, args.ToArray() );
            }
            else
            {
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
                        message = "Shows the list of people who are consided admins.";
                        break;

                    default:
                        message = "Invalid default command. " + defaultMessage;
                        break;
                }

                writer.SendMessage(
                    message,
                    response.Channel
                );
            }
        }
    }
}
