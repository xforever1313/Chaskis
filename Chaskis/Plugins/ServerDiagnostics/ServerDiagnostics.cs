//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

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

        public const string VersionStr = "1.0.0";

        /// <summary>
        /// List of IRC handlers.
        /// </summary>
        private List<IIrcHandler> handlerList;

        private ServerDiagnosticsConfig config;

        private IIrcConfig ircConfig;

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
        /// Info about this plugin.
        /// </summary>
        public string About
        {
            get
            {
                return "Get information about the server I'm running on such as uptime, operating system, number of processors, and time.";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Inits this plugin.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            string configPath = Path.Combine(
                initor.PluginDirectory,
                "ServerDiagnosticsConfig.xml"
            );

            this.config = XmlLoader.LoadConfig( configPath );
            this.ircConfig = initor.IrcConfig;

            if( string.IsNullOrEmpty( config.UpTimeCmd ) == false )
            {
                MessageHandler utimeHandler = new MessageHandler(
                    config.UpTimeCmd,
                    HandleUpTimeCmd,
                    coolDown
                );
                this.handlerList.Add( utimeHandler );
            }

            if( string.IsNullOrEmpty( config.OsVersionCmd ) == false )
            {
                MessageHandler osHandler = new MessageHandler(
                    config.OsVersionCmd,
                    HandleOsVersionCmd,
                    coolDown
                );
                this.handlerList.Add( osHandler );
            }

            if( string.IsNullOrEmpty( config.ProcessorCountCmd ) == false )
            {
                MessageHandler procCoundHandler = new MessageHandler(
                    config.ProcessorCountCmd,
                    HandleProcessorCountCmd,
                    coolDown
                );
                this.handlerList.Add( procCoundHandler );
            }

            if( string.IsNullOrEmpty( config.TimeCmd ) == false )
            {
                MessageHandler timeHandler = new MessageHandler(
                    config.TimeCmd,
                    HandleTimeCmd,
                    coolDown
                );
                this.handlerList.Add( timeHandler );
            }
        }

        /// <summary>
        /// Handles the help command.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            string message = "@" + response.RemoteUser + ": ";
            if( args.Length == 0 )
            {
                message += "Append 'uptime', 'os', 'processors', or 'time' to the help message you just sent to get more information about each command.";
            }
            else if( args[0] == "uptime" )
            {
                message += "To how long I've been running for, you must match this regex: " + this.config.UpTimeCmd.Replace( "{%nick%}", this.ircConfig.Nick );
            }
            else if( args[0] == "os" )
            {
                message += "To see what os I'm running, you must match this regex: " + this.config.OsVersionCmd.Replace( "{%nick%}", this.ircConfig.Nick );
            }
            else if( args[0] == "processors" )
            {
                message += "To see how many processors I'm running, you must match this regex: " + this.config.ProcessorCountCmd.Replace( "{%nick%}", this.ircConfig.Nick );
            }
            else if( args[0] == "time" )
            {
                message += "To see the time of my server, you must match this regex: " + this.config.ProcessorCountCmd.Replace( "{%nick%}", this.ircConfig.Nick );
            }
            else
            {
                message += "that is not a valid help command.  I can do 'uptime', 'os', 'processors', or 'time'";
            }

            writer.SendMessage(
                message,
                response.Channel
            );
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
        public void Dispose()
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

            writer.SendMessage(
                str,
                response.Channel
            );
        }

        /// <summary>
        /// Handles the OS version command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private static void HandleOsVersionCmd( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessage(
                "My system is " + Environment.OSVersion.ToString() + ".",
                response.Channel
            );
        }

        /// <summary>
        /// Handles the processor count command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private static void HandleProcessorCountCmd( IIrcWriter writer, IrcResponse response )
        {
            writer.SendMessage(
                "My system has " + Environment.ProcessorCount + " processors.",
                response.Channel
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
            writer.SendMessage(
                "My time is " + time.ToString( "yyyy-MM-dd hh:mm:ss" ) + " UTC.",
                response.Channel
            );
        }
    }
}