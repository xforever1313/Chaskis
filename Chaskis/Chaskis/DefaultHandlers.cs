//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        /// <summary>
        /// The command for getting the source of a plugin.
        /// </summary>
        private string sourceCommand;

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
                "[!@]" + this.ircConfig.Nick + @":?\s+plugins",
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
            writer.SendMessageToUser( this.pluginListResponse, response.Channel );
        }

        // ---- Source Command Handler ----

        private void AddSourceHandler()
        {
            this.sourceCommand = "[!@]" + this.ircConfig.Nick + @":?\s+source(\s+(?<pluginName>\w+))?";
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
            Match match = Regex.Match( response.Message, this.sourceCommand, RegexOptions.IgnoreCase );
            if( match.Success )
            {
                string pluginName = match.Groups["pluginName"].Value.ToLower();
                if( this.plugins.ContainsKey( pluginName ) )
                {
                    string msg = "Source of the plugin '" + pluginName + "': " + this.plugins[pluginName].SourceCodeLocation;
                    writer.SendMessageToUser( msg, response.Channel );
                }
                else if( ( pluginName == "chaskis" ) || string.IsNullOrEmpty( pluginName ) )
                {
                    string msg = "My source code is located here: https://github.com/xforever1313/Chaskis";
                    writer.SendMessageToUser( msg, response.Channel );
                }
                else
                {
                    writer.SendMessageToUser( "'" + pluginName + "' is not a plugin I have loaded...", response.Channel );
                }
            }
        }
    }
}
