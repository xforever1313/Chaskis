
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
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
        /// Regex to look for while in IRC.
        /// </summary>
        private const string cowsayRegex = 
            @"!(?<cmd>(" +
            defaultCommand + ")|(" +
            tuxCommand + @")|(" +
            vaderCommand + @")|(" +
            mooseCommand + @")|(" +
            lionCommand + @"))\s+(?<cowsayMsg>.+)";

        /// <summary>
        /// Path to the cowsay binary.
        /// </summary>
        private static readonly string cowsayProgram = "/usr/bin/cowsay";

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
        /// Command user uses to have tux appear.
        /// </summary>
        private const string tuxCommand = "tuxsay";

        /// <summary>
        /// Command user uses to have vader appear.
        /// </summary>
        private const string vaderCommand = "vadersay";

        /// <summary>
        /// Command user uses to have a moose appear.
        /// </summary>
        private const string mooseCommand = "moosesay";

        /// <summary>
        /// Command user uses to have a lion appear.
        /// </summary>
        private const string lionCommand = "lionsay";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

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
            this.cowSayInfo.FileName = cowsayProgram;

            this.handlers = new List<IIrcHandler>();
        }

        // -------- Functions --------

        /// <summary>
        /// Validates that the environment is good for the plugin.
        /// This must have cowsay installed in /usr/bin/cowsay in order to work properly.
        /// </summary>
        /// <param name="error">The errors that occurred if any.  string.Empty if none.</param>
        /// <returns>True if its okay to load this plugin, else false.</returns>
        public bool Validate( out string error )
        {
            if( File.Exists( cowsayProgram ) == false )
            {
                error = "Could not load Cowsay Program." + Environment.NewLine;
                error += "Cowsay not installed in " + cowsayProgram;
                return false;
            }
            else
            {
                error = string.Empty;
                return true;
            }
        }

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.
        /// </summary>
        public void Init()
        {
            IIrcHandler cowSayConfig = 
                new MessageHandler(
                    cowsayRegex,
                    HandleCowsayCommand,
                    5,
                    ResponseOptions.RespondOnlyToChannel
                );

            this.handlers.Add(
                cowSayConfig
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
        /// Handles the cowsay command.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleCowsayCommand( IIrcWriter writer, IrcResponse response )
        {
            try
            {
                Match cowMatch = Regex.Match( response.Message, cowsayRegex );
                if( cowMatch.Success )
                {
                    string messageToCowsay = cowMatch.Groups["cowsayMsg"].Value;

                    // Run the cowsay subprocess.
                    cowSayInfo.Arguments = GetArguments( cowMatch.Groups["cmd"].Value );

                    string cowSayedMessage = string.Empty;
                    using( Process cowsayProc = Process.Start( cowSayInfo ) )
                    {
                        using( StreamReader stdout = cowsayProc.StandardOutput )
                        {
                            using( StreamWriter stdin = cowsayProc.StandardInput )
                            {
                                stdin.Write( messageToCowsay );
                                stdin.Flush();
                            }

                            cowSayedMessage = stdout.ReadToEnd();
                        }

                        // If we hang for more than 15 seconds, abort.
                        if( cowsayProc.WaitForExit( 15 * 1000 ) == false )
                        {
                            cowsayProc.Kill();
                        }
                    }

                    if( string.IsNullOrEmpty( cowSayedMessage ) == false )
                    {
                        writer.SendCommand( cowSayedMessage );
                    }
                }
                else
                {
                    Console.Error.WriteLine( "Saw unknown line:" + response.Message );
                }
            }
            catch( Exception e )
            {
                Console.Error.WriteLine( "*********************" );
                Console.Error.WriteLine( "Caught Exception:" );
                Console.Error.WriteLine( e.Message );
                Console.Error.WriteLine( e.StackTrace );
                Console.Error.WriteLine( "**********************" );
            }
        }

        /// <summary>
        /// Gets the arguments based on the command the user gave us.
        /// </summary>
        /// <param name="commandString">The command string the user gave.  e.g. !tuxsay</param>
        /// <returns>The arguments.</returns>
        private static string GetArguments( string commandString )
        {
            switch( commandString )
            {
                case defaultCommand:
                    return string.Empty;

                case tuxCommand:
                    return "-f tux";

                case vaderCommand:
                    return "-f vader";

                case mooseCommand:
                    return "-f moose";

                case lionCommand:
                    return "-f moofasa";

                default:
                    return string.Empty;
            }
        }
    }
}

