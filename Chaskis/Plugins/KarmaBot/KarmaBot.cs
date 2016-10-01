//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ChaskisCore;

namespace Chaskis.Plugins.KarmaBot
{
    public class KarmaBot : IPlugin
    {
        // -------- Fields --------

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

        // -------- Functions --------

        /// <summary>
        /// Inits this plugin.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin DLL</param>
        /// <param name="ircConfig">The IRC config being used.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
            string dbPath = Path.Combine(
                Path.GetDirectoryName( pluginPath ),
                "karmabot.db"
            );

            string configPath = Path.Combine(
                Path.GetDirectoryName( pluginPath ),
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

            this.handlers.Add( increaseHandler );
            this.handlers.Add( decreaseCommand );
            this.handlers.Add( queryCommand );
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
        public void Teardown()
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
            Match match = Regex.Match( response.Message, this.config.IncreaseCommandRegex );
            if( match.Success )
            {
                string userName = match.Groups["name"].Value;
                int karma = await this.dataBase.IncreaseKarma( userName );

                writer.SendMessageToUser( userName + " has had their karma increased to " + karma, response.Channel );
            }
        }

        /// <summary>
        /// Handles the decrease command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private async void HandleDecreaseCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = Regex.Match( response.Message, this.config.DecreaseCommandRegex );
            if( match.Success )
            {
                string userName = match.Groups["name"].Value;
                int karma = await this.dataBase.DecreaseKarma( userName );

                writer.SendMessageToUser( userName + " has had their karma decreased to " + karma, response.Channel );
            }
        }

        /// <summary>
        /// Handles the query command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private async void HandleQueryCommand( IIrcWriter writer, IrcResponse response )
        {
            Match match = Regex.Match( response.Message, this.config.QueryCommand );
            if( match.Success )
            {
                string userName = match.Groups["name"].Value;
                int karma = await this.dataBase.QueryKarma( userName );

                writer.SendMessageToUser( userName + " has " + karma + " karma.", response.Channel );
            }
        }
    }
}