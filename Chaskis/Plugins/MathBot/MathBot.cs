//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ChaskisCore;

namespace Chaskis.Plugins.MathBot
{
    /// <summary>
    /// This plugin gets a math equation or boolean equation from IRC
    /// and calculates the result and reports it back.
    /// </summary>
    [ChaskisPlugin( "mathbot" )]
    public class MathBot : IPlugin
    {
        // -------- Fields --------

        public const string VersionStr = "1.0.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// The regex this bot watches for.
        /// </summary>
        private const string handlerRegex = @"^!calc(ulate)?\s+(?<expression>.+)";

        // -------- Constructor ---------

        /// <summary>
        /// Constructor.
        /// </summary>
        public MathBot()
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
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/MathBot";
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
        /// About this plugin.
        /// </summary>
        public string About
        {
            get
            {
                return "I can calculate math problems for you!  Addition, subtraction, multiplication, division, modulo, or even boolean logic!";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginPath">The absolute path to the plugin dll.</param>
        /// <param name="ircConfig">The IRC config we are using.</param>
        /// <param name="eventScheduler">The event scheduler (not used in this plugin).</param>
        public void Init( string pluginPath, IIrcConfig ircConfig, IChaskisEventScheduler eventScheduler )
        {
            MessageHandler handler = new MessageHandler(
                handlerRegex,
                MathHandler
            );

            this.handlers.Add( handler );
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            writer.SendMessageToUser(
                "Usage: !calc " + MathBotCalculator.calculatorRegex,
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
        }

        /// <summary>
        /// Handles the response when the user wants to calculate something.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="response"></param>
        private static void MathHandler( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;
            string expression = match.Groups["expression"].Value;
            try
            {
                string answer = MathBotCalculator.Calculate( expression );
                writer.SendMessageToUser(
                    "'" + expression + "' calculates to '" + answer + "'",
                    response.Channel
                );
            }
            catch( Exception )
            {
                writer.SendMessageToUser(
                    "'" + expression + "' is not something I can calculate :(",
                    response.Channel
                );
            }
        }
    }
}