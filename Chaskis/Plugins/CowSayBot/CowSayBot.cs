//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ChaskisCore;

namespace Chaskis.Plugins.CowSayBot
{
    [ChaskisPlugin( "cowsaybot" )]
    public class CowSayBot : IPlugin
    {
        // -------- Fields --------

        public const string VersionStr = "1.0.0";

        /// <summary>
        /// Process Start info
        /// </summary>
        private readonly ProcessStartInfo cowSayInfo;

        // ---- Commands ----

        /// <summary>
        /// Command the user uses to use the default cow.
        /// </summary>
        private const string defaultCommand = "cowsay";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// The cowsay bot config.
        /// </summary>
        private CowSayBotConfig cowSayConfig;

        /// <summary>
        /// Regex to look for to respond to.
        /// </summary>
        private string cowsayRegex;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public CowSayBot()
        {
            this.cowSayInfo = new ProcessStartInfo();
            this.cowSayInfo.RedirectStandardInput = true;
            this.cowSayInfo.RedirectStandardOutput = true;
            this.cowSayInfo.UseShellExecute = false;

            this.handlers = new List<IIrcHandler>();
        }

        // -------- Properties --------

        /// <summary>
        /// Returns the source code location of this plugin.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/CowSayBot";
            }
        }

        /// <summary>
        /// The version of this plugin.
        /// </summary>
        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        /// <summary>
        /// What I do.
        /// </summary>
        public string About
        {
            get
            {
                return "I echo back messages in the form of a cow saying something";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>

        public void Init( PluginInitor initor )
        {
            string configPath = Path.Combine(
                initor.PluginDirectory,
                "CowSayBotConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.cowSayConfig = XmlLoader.LoadCowSayBotConfig( configPath );

            if( File.Exists( cowSayConfig.ExeCommand ) == false )
            {
                throw new InvalidOperationException( "Can not load cowsay program from " + cowSayConfig.ExeCommand );
            }

            this.cowSayInfo.FileName = cowSayConfig.ExeCommand;
            this.cowsayRegex = ConstructRegex( this.cowSayConfig );

            Console.WriteLine( "CowSayBot: Using Regex '" + this.cowsayRegex + "'" );

            IIrcHandler cowSayHandler = new MessageHandler(
                this.cowsayRegex,
                HandleCowsayCommand,
                (int)cowSayConfig.CoolDownTimeSeconds,
                ResponseOptions.ChannelOnly
            );

            this.handlers.Add(
                cowSayHandler
            );
        }

        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( "My avaiable commands are: " );
            foreach( string command in this.cowSayConfig.CowFileInfoList.CommandList.Keys )
            {
                builder.Append( command + ", " );
            }

            // Remove ',' and space.
            builder.Remove( builder.Length - 2, builder.Length - 2 );

            // Limit our help message.
            if( builder.Length >= 451 )
            {
                builder.Remove( 450, builder.Length );
            }

            writer.SendMessage(
                builder.ToString(),
                response.Channel
            );
        }

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The read-only list of handlers to awtch.</returns>
        public IList<IIrcHandler> GetHandlers()
        {
            return handlers.AsReadOnly();
        }

        /// <summary>
        /// Tears down this plugin.  No-Op.
        /// </summary>
        public void Dispose()
        {
            // No-op.
        }

        /// <summary>
        /// Constructs the regex the that calls the Cow Say Handler.
        /// </summary>
        /// <param name="config">The cowsay bot config to use.</param>
        /// <returns>The regex that calls the cowsay handler.</returns>
        private string ConstructRegex( CowSayBotConfig config )
        {
            string commandRegex = @"(?<command>(";
            foreach( string command in config.CowFileInfoList.CommandList.Keys )
            {
                commandRegex += command + ")|(";
            }
            commandRegex = commandRegex.TrimEnd( '|', '(' );
            commandRegex += ")";

            string cowsayRegex = config.ListenRegex.Replace( "{%saycmd%}", commandRegex );
            return cowsayRegex;
        }

        /// <summary>
        /// Launches the cowsay sub-process
        /// </summary>
        /// <param name="messageToCowsay">The message to put through cowsay.</param>
        /// <param name="processOutput">The standard-output from the cowsay process.</param>
        /// <param name="cowFile">The cowfile to use.  Null for none.</param>
        /// <returns>ExitCode of cowsay.  0 for success.</returns>
        private int LaunchCowsay( string messageToCowsay, out string processOutput, string cowFile = null )
        {
            this.cowSayInfo.Arguments = string.Empty;
            if( string.IsNullOrEmpty( cowFile ) == false )
            {
                this.cowSayInfo.Arguments = "-f " + cowFile;
            }

            int exitCode = -1;
            using( Process cowsayProc = Process.Start( cowSayInfo ) )
            {
                using( StreamReader stdout = cowsayProc.StandardOutput )
                {
                    using( StreamWriter stdin = cowsayProc.StandardInput )
                    {
                        stdin.Write( messageToCowsay );
                        stdin.Flush();
                    }

                    processOutput = stdout.ReadToEnd();
                }

                // If we hang for more than 15 seconds, abort.
                if( cowsayProc.WaitForExit( 15 * 1000 ) == false )
                {
                    cowsayProc.Kill();
                }
                exitCode = cowsayProc.ExitCode;
            }

            return exitCode;
        }

        /// <summary>
        /// Handles the cowsay command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleCowsayCommand( IIrcWriter writer, IrcResponse response )
        {
            try
            {
                Match cowMatch = response.Match;

                string cowFile = this.cowSayConfig.CowFileInfoList.CommandList[cowMatch.Groups["command"].Value];
                if( cowFile == "DEFAULT" )
                {
                    cowFile = null;
                }

                string cowSayedMessage;
                int exitCode = LaunchCowsay( cowMatch.Groups["msg"].Value, out cowSayedMessage, cowFile );

                if( ( string.IsNullOrEmpty( cowSayedMessage ) == false ) && ( exitCode == 0 ) )
                {
                    writer.SendMessage( cowSayedMessage, response.Channel );
                }
                else if( exitCode != 0 )
                {
                    Console.Error.WriteLine( "CowSayBot: Exit code not 0.  Got: " + exitCode );
                }
                else if( string.IsNullOrEmpty( cowSayedMessage ) )
                {
                    Console.Error.WriteLine( "CowSayBot: Nothing returned from cowsay process." );
                }
            }
            catch( Exception e )
            {
                Console.Error.WriteLine( "*********************" );
                Console.Error.WriteLine( "CowSayBot: Caught Exception:" );
                Console.Error.WriteLine( e.Message );
                Console.Error.WriteLine( e.StackTrace );
                Console.Error.WriteLine( "**********************" );
            }
        }
    }
}