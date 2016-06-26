
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using GenericIrcBot;

namespace Chaskis.Plugins.ConsoleOut
{
    /// <summary>
    /// This plugin takes everything it gets from IRC and
    /// prints it to Console.Out. Only really useful using the Console Program and
    /// for debuggin purposes.
    /// </summary>
    public class ConsoleOut : IPlugin
    {
        // -------- Fields --------

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        // -------- Constructor ---------

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConsoleOut()
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
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/ConsoleOut";
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
            AllHandler handler = new AllHandler(
                delegate ( IIrcWriter writer, IrcResponse response )
                {
                    Console.WriteLine( response.Message );
                    Console.Out.Flush();
                }
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

    }
}
