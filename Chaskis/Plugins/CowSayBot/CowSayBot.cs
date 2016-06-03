
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using GenericIrcBot;

namespace Chaskis.Plugins.CowSayBot
{
    public class CowSayBot : IPlugin
    {
        // -------- Fields --------

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
        /// The irc config to use.
        /// </summary>
        private IIrcConfig ircConfig;

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

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin dll.</param>
        /// <param name="config">The irc config to use.</param>
        public void Init( string pluginPath, IIrcConfig config )
        {
            string configPath = Path.Combine(
                Path.GetDirectoryName( pluginPath ),
                "CowSayBotConfig.xml"
            );

            if ( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.ircConfig = config;

            this.cowSayConfig = XmlLoader.LoadCowSayBotConfig( configPath );

            if ( File.Exists( cowSayConfig.ExeCommand ) == false )
            {
                throw new InvalidOperationException( "Can not load cowsay program from " + cowSayConfig.ExeCommand );
            }

            this.cowSayInfo.FileName = cowSayConfig.ExeCommand;
            this.cowsayRegex = ConstructRegex( this.cowSayConfig );

            Console.WriteLine( "CowSayBot: Using Regex '" + this.cowsayRegex + "'" );

            IIrcHandler cowSayHandler = new MessageHandler(
                this.cowsayRegex,
                HandleCowsayCommand,
                ( int ) cowSayConfig.CoolDownTimeSeconds,
                ResponseOptions.RespondOnlyToChannel
            );

            this.handlers.Add(
                cowSayHandler
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
        public void Teardown()
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
            foreach ( string command in config.CowFileInfoList.CommandList.Keys )
            {
                commandRegex += command + ")|(";
            }
            commandRegex = commandRegex.TrimEnd( '|', '(' );
            commandRegex += ")";

            string cowsayRegex = config.ListenRegex.Replace( "{%saycmd%}", commandRegex );
            cowsayRegex = cowsayRegex.Replace( "{%channel%}", this.ircConfig.Channel );
            cowsayRegex = cowsayRegex.Replace( "{%nick%}", this.ircConfig.Nick );

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
            if ( string.IsNullOrEmpty( cowFile ) == false )
            {
                this.cowSayInfo.Arguments = "-f " + cowFile;
            }

            int exitCode = -1;
            using ( Process cowsayProc = Process.Start( cowSayInfo ) )
            {
                using ( StreamReader stdout = cowsayProc.StandardOutput )
                {
                    using ( StreamWriter stdin = cowsayProc.StandardInput )
                    {
                        stdin.Write( messageToCowsay );
                        stdin.Flush();
                    }

                    processOutput = stdout.ReadToEnd();
                }

                // If we hang for more than 15 seconds, abort.
                if ( cowsayProc.WaitForExit( 15 * 1000 ) == false )
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
                Match cowMatch = Regex.Match( response.Message, this.cowsayRegex );
                if( cowMatch.Success )
                {
                    string cowFile = this.cowSayConfig.CowFileInfoList.CommandList[cowMatch.Groups["command"].Value];
                    if ( cowFile == "DEFAULT" )
                    {
                        cowFile = null;
                    }

                    string cowSayedMessage;
                    int exitCode = LaunchCowsay( cowMatch.Groups["msg"].Value, out cowSayedMessage, cowFile );

                    if ( ( string.IsNullOrEmpty( cowSayedMessage ) == false ) && ( exitCode == 0 ) )
                    {
                        writer.SendCommand( cowSayedMessage );
                    }
                    else if ( exitCode != 0 )
                    {
                        Console.Error.WriteLine( "CowSayBot: Exit code not 0.  Got: " +  exitCode );
                    }
                    else if ( string.IsNullOrEmpty( cowSayedMessage ) )
                    {
                        Console.Error.WriteLine( "CowSayBot: Nothing returned from cowsay process." );
                    }
                }
                else
                {
                    Console.Error.WriteLine( "CowSayBot: Saw unknown line:" + response.Message );
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

