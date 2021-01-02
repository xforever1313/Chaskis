//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using Chaskis.Core;

namespace Chaskis.Plugins.ServerDiagnostics
{
    [ChaskisPlugin( "serverdiagnostics" )]
    public class ServerDiagnostics : IPlugin
    {
        // -------- Fields --------

        internal const string VersionStr = "1.0.0";

        /// <summary>
        /// List of IRC handlers.
        /// </summary>
        private readonly List<IIrcHandler> handlerList;

        private ServerDiagnosticsConfig config;

        private IReadOnlyIrcConfig ircConfig;

        /// <summary>
        /// The cool down for the bot for each command.
        /// </summary>
        private const int coolDown = 60;

        /// <summary>
        /// The time stamp of when this plugin was loaded.
        /// </summary>
        private readonly DateTime startTime;

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
                initor.ChaskisConfigPluginRoot,
                "ServerDiagnostics",
                "ServerDiagnosticsConfig.xml"
            );

            this.config = XmlLoader.LoadConfig( configPath );
            this.ircConfig = initor.IrcConfig;

            if( string.IsNullOrEmpty( config.UpTimeCmd ) == false )
            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = config.UpTimeCmd,
                    LineAction = HandleUpTimeCmd,
                    CoolDown = coolDown
                };

                MessageHandler utimeHandler = new MessageHandler(
                    msgConfig
                );
                this.handlerList.Add( utimeHandler );
            }

            if( string.IsNullOrEmpty( config.OsVersionCmd ) == false )
            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = config.OsVersionCmd,
                    LineAction = HandleOsVersionCmd,
                    CoolDown = coolDown
                };

                MessageHandler osHandler = new MessageHandler(
                    msgConfig
                );
                this.handlerList.Add( osHandler );
            }

            if( string.IsNullOrEmpty( config.ProcessorCountCmd ) == false )
            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = config.ProcessorCountCmd,
                    LineAction = HandleProcessorCountCmd,
                    CoolDown = coolDown
                };

                MessageHandler procCoundHandler = new MessageHandler(
                    msgConfig
                );
                this.handlerList.Add( procCoundHandler );
            }

            if( string.IsNullOrEmpty( config.TimeCmd ) == false )
            {
                MessageHandlerConfig msgConfig = new MessageHandlerConfig
                {
                    LineRegex = config.TimeCmd,
                    LineAction = HandleTimeCmd,
                    CoolDown = coolDown
                };

                MessageHandler timeHandler = new MessageHandler(
                    msgConfig
                );
                this.handlerList.Add( timeHandler );
            }
        }

        /// <summary>
        /// Handles the help command.
        /// </summary>
        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            string message = "@" + msgArgs.User + ": ";
            if( helpArgs.Length == 0 )
            {
                message += "Append 'uptime', 'os', 'processors', or 'time' to the help message you just sent to get more information about each command.";
            }
            else if( helpArgs[0] == "uptime" )
            {
                message += "To how long I've been running for, you must match this regex: " + this.config.UpTimeCmd.Replace( "{%nick%}", this.ircConfig.Nick );
            }
            else if( helpArgs[0] == "os" )
            {
                message += "To see what os I'm running, you must match this regex: " + this.config.OsVersionCmd.Replace( "{%nick%}", this.ircConfig.Nick );
            }
            else if( helpArgs[0] == "processors" )
            {
                message += "To see how many processors I'm running, you must match this regex: " + this.config.ProcessorCountCmd.Replace( "{%nick%}", this.ircConfig.Nick );
            }
            else if( helpArgs[0] == "time" )
            {
                message += "To see the time of my server, you must match this regex: " + this.config.ProcessorCountCmd.Replace( "{%nick%}", this.ircConfig.Nick );
            }
            else
            {
                message += "that is not a valid help command.  I can do 'uptime', 'os', 'processors', or 'time'";
            }

            msgArgs.Writer.SendMessage(
                message,
                msgArgs.Channel
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
            // Nothing to dispose.
        }

        // ---- Handlers ----

        /// <summary>
        /// Handles the up time command.
        /// </summary>
        private void HandleUpTimeCmd( MessageHandlerArgs args )
        {
            TimeSpan span = DateTime.UtcNow - startTime;

            string str = string.Format(
                "has been running for {0} Day(s), {1} Hour(s), {2} Minute(s), and {3} Second(s).",
                span.Days,
                span.Hours,
                span.Minutes,
                span.Seconds
            );

            args.Writer.SendAction(
                str,
                args.Channel
            );
        }

        /// <summary>
        /// Handles the OS version command.
        /// </summary>
        private static void HandleOsVersionCmd( MessageHandlerArgs args )
        {
            args.Writer.SendAction(
                "is running on " + Environment.OSVersion.ToString() + ".",
                args.Channel
            );
        }

        /// <summary>
        /// Handles the processor count command.
        /// </summary>
        private static void HandleProcessorCountCmd( MessageHandlerArgs args )
        {
            args.Writer.SendAction(
                "has " + Environment.ProcessorCount + " processors running on its system.",
                args.Channel
            );
        }

        /// <summary>
        /// Handles the time command.
        /// </summary>
        private static void HandleTimeCmd( MessageHandlerArgs args )
        {
            DateTime time = DateTime.UtcNow;
            args.Writer.SendAction(
                "has its system time set to " + time.ToString( "yyyy-MM-dd hh:mm:ss" ) + " UTC.",
                args.Channel
            );
        }
    }
}