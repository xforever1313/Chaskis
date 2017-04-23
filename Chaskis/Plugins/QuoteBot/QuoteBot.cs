//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Text;
using ChaskisCore;

namespace Chaskis.Plugins.QuoteBot
{
    /// <summary>
    /// This plugin saves quotes from IRC to a sqlite database on the server.
    /// Also allows users to view quotes.
    /// </summary>
    [ChaskisPlugin( "quotebot" )]
    public class QuoteBot : IPlugin
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "0.1.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public QuoteBot()
        {
            this.handlers = new List<IIrcHandler>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The location of the source code.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/QuoteBot";
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
                return "I save memorable quotes from the IRC channel for you!";
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginPath">The absolute path to the plugin dll.</param>
        /// <param name="ircConfig">The IRC config we are using.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            StringBuilder builder = new StringBuilder();

            builder.Append( "@" + response.RemoteUser + ": " );

            if( args.Length == 0 )
            {
                builder.Append( "Append 'add', 'delete', 'random', or 'get' to the help message you just sent to get more information about each command." );
            }
            else if( args[0] == "add" )
            {
                builder.Append( "Adds a quote to the database.  Usage: TODO " );
            }
            else if( args[0] == "delete" )
            {
                builder.Append( "Deletes a quote from the database. Must be a bot admin. Usage: TODO " );
            }
            else if( args[0] == "random" )
            {
                builder.Append( "Posts a random quote from the database. Usage:" );
            }
            else if( args[0] == "get" )
            {
                builder.Append( "Posts the given quote from the database. Usage:" );
            }

            writer.SendMessageToUser(
                builder.ToString(),
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
        public void Teardown()
        {
        }
    }
}
