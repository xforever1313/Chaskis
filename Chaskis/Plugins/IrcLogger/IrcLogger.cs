//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using ChaskisCore;

namespace Chaskis.Plugins.IrcLogger
{
    /// <summary>
    /// IRC Logger logs all messages from the IRC channel to
    /// the log file.
    /// </summary>
    [ChaskisPlugin( "irclogger" )]
    public class IrcLogger : IPlugin
    {
        // -------- Fields --------

        public const string VersionStr = "0.2.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// The thing that managers the logs.
        /// </summary>
        private LogManager logManager;

        private IrcLoggerConfig config;

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

        /// <summary>
        /// This plugin's version.
        /// </summary>
        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        /// <summary>
        /// A description of this plugin.
        /// </summary>
        public string About
        {
            get
            {
                return "I log the IRC chat to the server.  That's all.  I have no commands you can use.";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            string pluginDir = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "IrcLogger"
            );

            string configPath = Path.Combine(
                pluginDir,
                "IrcLoggerConfig.xml"
            );

            this.config = XmlLoader.LoadIrcLoggerConfig(
                configPath
            );

            if( string.IsNullOrEmpty( config.LogFileLocation ) )
            {
                config.LogFileLocation = Path.Combine( pluginDir, "Logs" );
            }
            if( string.IsNullOrEmpty( config.LogName ) )
            {
                config.LogName = "irclog";
            }

            this.logManager = new LogManager( config, initor.Log );

            AllHandler handler = new AllHandler(
                this.HandleLogEvent
            );

            this.handlers.Add( handler );
        }

        /// <summary>
        /// Handles the help command.
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
        /// <returns>The list of handlers to awtch.</returns>
        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }

        /// <summary>
        /// Tears down the plugin.
        /// </summary>
        public void Dispose()
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
            bool log = true;
            for( int i = 0; i < ( this.config.IgnoreRegexes.Count ) && log; ++i )
            {
                if( config.IgnoreRegexes[i].IsMatch( response.Message ) )
                {
                    log = false;
                }
            }

            if( log )
            {
                this.logManager.AsyncLogToFile( response.Message );
            }
        }
    }
}