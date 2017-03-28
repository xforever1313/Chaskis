//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using ChaskisCore;
using SethCS.Exceptions;

namespace Chaskis.Plugins.ServerDiagnostics
{
    [ChaskisPlugin( "serverdiagnostics" )]
    public class ServerDiagnostics : IPlugin
    {
        // -------- Fields --------

        /// <summary>
        /// List of IRC handlers.
        /// </summary>
        private List<IIrcHandler> handlerList;

        /// <summary>
        /// The cool down for the bot for each command.
        /// </summary>
        private const int coolDown = 60;

        /// <summary>
        /// The time stamp of when this plugin was loaded.
        /// </summary>
        private static DateTime startTime;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public ServerDiagnostics()
        {
            this.handlerList = new List<IIrcHandler>();
            startTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns the source code location of this plugin.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/ServerDiagnostics";
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
            ArgumentChecker.StringIsNotNullOrEmpty( pluginPath, nameof( pluginPath ) );
            ArgumentChecker.IsNotNull( ircConfig, nameof( ircConfig ) );

            string configPath = Path.Combine(
                Path.GetDirectoryName( pluginPath ),
                "ServerDiagnosticsConfig.xml"
            );

            ServerDiagnosticsConfig config = XmlLoader.LoadConfig( configPath );

            if( string.IsNullOrEmpty( config.UpTimeCmd ) == false )
            {
                MessageHandler utimeHandler = new MessageHandler(
                    config.UpTimeCmd.Replace( "{%nick%}", ircConfig.Nick ),
                    HandleUpTimeCmd,
                    coolDown
                );
                this.handlerList.Add( utimeHandler );
            }

            if( string.IsNullOrEmpty( config.OsVersionCmd ) == false )
            {
                MessageHandler osHandler = new MessageHandler(
                    config.OsVersionCmd.Replace( "{%nick%}", ircConfig.Nick ),
                    HandleOsVersionCmd,
                    coolDown
                );
                this.handlerList.Add( osHandler );
            }

            if( string.IsNullOrEmpty( config.ProcessorCountCmd ) == false )
            {
                MessageHandler procCoundHandler = new MessageHandler(
                    config.ProcessorCountCmd.Replace( "{%nick%}", ircConfig.Nick ),
                    HandleProcessorCountCmd,
                    coolDown
                );
                this.handlerList.Add( procCoundHandler );
            }

            if( string.IsNullOrEmpty( config.TimeCmd ) == false )
            {
                MessageHandler timeHandler = new MessageHandler(
                    config.TimeCmd.Replace( "{%nick%}", ircConfig.Nick ),
                    HandleTimeCmd,
                    coolDown
                );
                this.handlerList.Add( timeHandler );
            }
        }

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The list of handlers to awtch.</returns>
        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlerList.AsReadOnly();
        }

        /// <summary>
        /// Tears down this plugin.
        /// </summary>
        public void Teardown()
        {
        }

        // ---- Handlers ----

        /// <summary>
        /// Handles the up time command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private static void HandleUpTimeCmd( IIrcWriter writer, IrcResponse response )
        {
            TimeSpan span = DateTime.UtcNow - startTime;

            string str = string.Format(
                "I have been running for {0} Day(s), {1} Hour(s), {2} Minute(s), and {3} Second(s).",
                span.Days,
                span.Hours,
                span.Minutes,
                span.Seconds
            );

            writer.SendCommandToChannel(
                str
            );
        }

        /// <summary>
        /// Handles the OS version command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private static void HandleOsVersionCmd( IIrcWriter writer, IrcResponse response )
        {
            writer.SendCommandToChannel(
                "My system is " + Environment.OSVersion.ToString() + "."
            );
        }

        /// <summary>
        /// Handles the processor count command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private static void HandleProcessorCountCmd( IIrcWriter writer, IrcResponse response )
        {
            writer.SendCommandToChannel(
                "My system has " + Environment.ProcessorCount + " processors."
            );
        }

        /// <summary>
        /// Handles the time command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private static void HandleTimeCmd( IIrcWriter writer, IrcResponse response )
        {
            DateTime time = DateTime.UtcNow;
            writer.SendCommandToChannel(
                "My time is " + time.ToString( "yyyy-MM-dd hh:mm:ss" ) + " UTC."
            );
        }
    }
}