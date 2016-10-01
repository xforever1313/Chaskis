//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GenericIrcBot;

namespace Chaskis.Plugins.MathBot
{
    /// <summary>
    /// This plugin gets a math equation or boolean equation from IRC
    /// and calculates the result and reports it back.
    /// </summary>
    public class MathBot : IPlugin
    {
        // -------- Fields --------

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        /// <summary>
        /// The regex this bot watches for.
        /// </summary>
        private const string handlerRegex = @"!calc(ulate)?\s+(?<expression>.+)";

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

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginPath">The absolute path to the plugin dll.</param>
        /// <param name="ircConfig">The IRC config we are using.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
            MessageHandler handler = new MessageHandler(
                handlerRegex,
                MathHandler
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
        }

        /// <summary>
        /// Handles the response when the user wants to calculate something.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="response"></param>
        private static void MathHandler( IIrcWriter writer, IrcResponse response )
        {
            Match match = Regex.Match( response.Message, handlerRegex );
            string expression = match.Groups["expression"].Value;

            if( match.Success )
            {
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
            else
            {
                writer.SendMessageToUser(
                    "'" + expression + "' is not something I can calculate :(",
                    response.Channel
                );
            }
        }
    }
}