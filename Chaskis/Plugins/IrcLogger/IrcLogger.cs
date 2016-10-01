//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using System.IO;
using ChaskisCore;

namespace Chaskis.Plugins.IrcLogger
{
    /// <summary>
    /// IRC Logger logs all messages from the IRC channel to
    /// the log file.
    /// </summary>
    public class IrcLogger : IPlugin
    {
        // -------- Fields --------

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// The thing that managers the logs.
        /// </summary>
        private LogManager logManager;

        // -------- Constructor ---------

        /// <summary>
        /// Constructor.
        /// </summary>
        public IrcLogger()
        {
            this.handlers = new List<IIrcHandler>();
        }

        // -------- Properties --------

        /// <summary>
        /// The location of the source code.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/IrcLogger";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginPath">The absolute path to the plugin dll.</param>
        /// <param name="ircConfig">The IRC config we are using.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
            string pluginDir = Path.GetDirectoryName( pluginPath );

            IrcLoggerConfig config = XmlLoader.LoadIrcLoggerConfig(
                Path.Combine( pluginDir, "IrcLoggerConfig.xml" )
            );

            if( string.IsNullOrEmpty( config.LogFileLocation ) )
            {
                config.LogFileLocation = Path.Combine( pluginDir, "Logs" );
            }
            if( string.IsNullOrEmpty( config.LogName ) )
            {
                config.LogName = "irclog";
            }

            this.logManager = new LogManager( config );

            AllHandler handler = new AllHandler(
                this.HandleLogEvent
            );

            this.handlers.Add( handler );
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
        /// Tears down the plugin.
        /// </summary>
        public void Teardown()
        {
            this.logManager?.Dispose();
        }

        /// <summary>
        /// Handles writing an event to the log.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleLogEvent( IIrcWriter writer, IrcResponse response )
        {
            this.logManager.LogToFile( response.Message );
        }
    }
}