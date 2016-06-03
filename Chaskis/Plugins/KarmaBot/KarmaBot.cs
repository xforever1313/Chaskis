
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.IO;
using GenericIrcBot;

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

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public KarmaBot()
        {
            this.handlers = new List<IIrcHandler>();
        }

        // -------- Functions --------

        /// <summary>
        /// Inits this plugin.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin DLL</param>
        /// <param name="ircConfig">The IRC config being used.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
            string configPath = Path.Combine(
                Path.GetDirectoryName( pluginPath ),
                "KarmaBotConfig.xml"
            );

            if ( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.config = XmlLoader.LoadKarmaBotConfig( configPath );

            MessageHandler increaseHandler = new MessageHandler(
                this.config.IncreaseCommandRegex,
                HandleIncreaseCommand
            );

            MessageHandler decreaseCommand = new MessageHandler(
                this.config.DecreaseCommandRegex,
                HandleDecreaseCommand
            );

            MessageHandler queryCommand = new MessageHandler(
                this.config.DecreaseCommandRegex,
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

        // ---- Handlers ----

        /// <summary>
        /// Handles the increase command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleIncreaseCommand( IIrcWriter writer, IrcResponse response )
        {
        }

        /// <summary>
        /// Handles the decrease command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleDecreaseCommand( IIrcWriter writer, IrcResponse response )
        {
        }

        /// <summary>
        /// Handles the query command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleQueryCommand( IIrcWriter writer, IrcResponse response )
        {
        }
    }
}
