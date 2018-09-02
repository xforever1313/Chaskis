//
//          Copyright Seth Hendrick 2017-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using Chaskis.Core;

namespace Chaskis.Plugins.MessageFixerBot
{
    [ChaskisPlugin( "messagefixerbot" )]
    public class MessageFixerBot : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string VersionStr = "0.2.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private readonly MessageFixer msgFixer;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageFixerBot()
        {
            this.handlers = new List<IIrcHandler>();
            this.msgFixer = new MessageFixer();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The location of the source code.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/SearchAndReplaceBot";
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
                return "I fix people's previous message if they request me to.";
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.  Allowed to throw Exceptions.
        ///
        /// This function should be used to validates that the environment is good for the plugin.
        /// For example, it has all dependencies installed, config files are in the correct spot, etc.
        /// It should also load GetHandlers() with the handlers.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            // Search for EVERY message.  Our MessageFixer will then determine
            // what to do with it.
            MessageHandler handler = new MessageHandler(
                ".+",
                this.HandleMessage
            );

            this.handlers.Add( handler );
        }

        /// <summary>
        /// Handles the help command.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            writer.SendMessage(
                "To fix your previous message, send 's/findRegex/replace' to the IRC channel.  I will replace all matches of 'findRegex' in your previous message and replace it with 'replace'. Use '\\/' to escape '/' characters.",
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
            // Nothing to dispose.
        }

        /// <summary>
        /// Handles a message from the channel.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private void HandleMessage( IIrcWriter writer, IrcResponse response )
        {
            MessageFixerResult result = this.msgFixer.RecordNewMessage( response.RemoteUser, response.Message );
            if( result.Success )
            {
                string msg = string.Format(
                    "{0}'s updated message: '{1}'",
                    response.RemoteUser,
                    result.Message
                );

                writer.SendMessage( msg, response.Channel );
            }
            else if( string.IsNullOrEmpty( result.Message ) == false )
            {
                string msg = string.Format(
                    "{0}: error when trying to fix your message: '{1}'",
                    response.RemoteUser,
                    result.Message
                );

                writer.SendMessage( msg, response.Channel );
            }
        }
    }
}
