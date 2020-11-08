//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using Chaskis.Core;

namespace Chaskis.Plugins.ConsoleOut
{
    /// <summary>
    /// This plugin takes everything it gets from IRC and
    /// prints it to Console.Out. Only really useful using the Console Program and
    /// for debugging purposes.
    /// </summary>
    [ChaskisPlugin( "consoleout" )]
    public class ConsoleOut : IPlugin
    {
        // -------- Fields --------

        internal const string VersionStr = "2.0.2";

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
        /// What this plugin does.
        /// </summary>
        public string About
        {
            get
            {
                return "I only print stuff to Console Out on my server. I'm really only good for debugging. I'm not sure why I'm turned on to be honest...";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            {
                ReceiveHandlerConfig allHandlerConfig = new ReceiveHandlerConfig
                {
                    LineAction = LineAction
                };

                ReceiveHandler handler = new ReceiveHandler(
                    allHandlerConfig
                );

                this.handlers.Add( handler );
            }

            {
                SendEventConfig onSendConfig = new SendEventConfig
                {
                    LineAction = LineAction
                };

                SendEventHandler handler = new SendEventHandler( onSendConfig );
                this.handlers.Add( handler );
            }
        }

        /// <summary>
        /// Handles help message.
        /// </summary>
        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            msgArgs.Writer.SendMessage( this.About, msgArgs.Channel );
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
            // Nothing to dispose.
        }

        private static void LineAction( ReceiveHandlerArgs args )
        {

            Console.WriteLine( args.Line );
        }

        private static void LineAction( SendEventArgs args )
        {
            Console.WriteLine( args.Command );
        }
    }
}