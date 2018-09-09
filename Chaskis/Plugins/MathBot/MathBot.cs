//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chaskis.Core;

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

        internal const string VersionStr = "0.3.0";

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
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            MessageHandlerConfig msgConfig = new MessageHandlerConfig
            {
                LineRegex = handlerRegex,
                LineAction = MathHandler
            };

            MessageHandler handler = new MessageHandler(
                msgConfig
            );

            this.handlers.Add( handler );
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, MessageHandlerArgs response, string[] args )
        {
            writer.SendMessage(
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
            // Nothing to Dispose.
        }

        /// <summary>
        /// Handles the response when the user wants to calculate something.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="response"></param>
        private static void MathHandler( IIrcWriter writer, MessageHandlerArgs response )
        {
            Match match = response.Match;
            string expression = match.Groups["expression"].Value;
            try
            {
                string answer = MathBotCalculator.Calculate( expression );
                writer.SendMessage(
                    "'" + expression + "' calculates to '" + answer + "'",
                    response.Channel
                );
            }
            catch( Exception )
            {
                writer.SendMessage(
                    "'" + expression + "' is not something I can calculate :(",
                    response.Channel
                );
            }
        }
    }
}