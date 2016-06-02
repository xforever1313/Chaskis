
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericIrcBot;

namespace Chaskis.Plugins.KarmaBot
{
    public class KarmaBot : IPlugin
    {
        // -------- Fields --------

        /// <summary>
        /// List of handlers.
        /// </summary>
        private List<IIrcHandler> handlers;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public KarmaBot()
        {
            this.handlers = new List<IIrcHandler>();
        }

        // -------- Functions --------

        /// <summary>
        /// Inits this plugin.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin DLL</param>
        /// <param name="ircConfig">The IRC config being used.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
        }

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The list of handlers to awtch.</returns>
        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }
    }
}
