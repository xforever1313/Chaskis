//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ChaskisCore;

namespace Chaskis.Plugins.CrossChannel
{
    /// <summary>
    /// This plugin allows people to broadcast to all channels
    /// the bot is in, or send messages from one channel
    /// to another.
    /// </summary>
    [ChaskisPlugin( "crosschannel" )]
    public class CrossChannel : IPlugin
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "0.1.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private IIrcConfig ircConfig;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor
        /// </summary>
        public CrossChannel()
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
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/CrossChannel";
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
                return "I allow a user to send a message to a different channel or broadcast to all channels.";
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            this.ircConfig = initor.IrcConfig;

            MessageHandler bcastHandler = new MessageHandler(
                @"^!broadcast\s+(?<bcastmsg>.+)",
                this.BroadcastHandler
            );
            this.handlers.Add( bcastHandler );

            MessageHandler ccHandler = new MessageHandler(
                @"^!cc\s+\<(?<channel>\S+)\>\s+(?<ccmessage>.+)",
                this.CCHandler
            );
            this.handlers.Add( ccHandler );
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
                builder.Append( "Append 'broadcast', or 'cc' to the help message you just sent to get more information about each command." );
            }
            else if( args[0] == "broadcast" )
            {
                builder.Append( "Broadcasts a message to ALL channels I am in. Command: !broadcast Your Message Here."  );
            }
            else if( args[0] == "cc" )
            {
                builder.Append( "Sends a message to another channel I am in from this one.  Command: !cc <#channelname> Your Message Here." );
            }
            else
            {
                builder.Append( "That is not a command I know..." );
            }

            writer.SendMessage(
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
        public void Dispose()
        {
        }

        // ---------------- Handlers ----------------

        /// <summary>
        /// Handles the broadcast command.
        /// </summary>
        private void BroadcastHandler( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;
            string message = match.Groups["bcastmsg"].Value;

            string msg = string.Format(
                "<{0}@{1}> {2}",
                response.RemoteUser,
                response.Channel,
                message
            );

            writer.SendBroadcastMessage( msg );
        }

        /// <summary>
        /// Handles the CC Command.
        /// </summary>
        private void CCHandler( IIrcWriter writer, IrcResponse response )
        {
            Match match = response.Match;
            string channel = match.Groups["channel"].Value.ToLower();

            // Can only send a message to a channel we are actually in.
            if( this.ircConfig.Channels.Contains( channel ) == false )
            {
                string msg = string.Format(
                    "@{0}: I am not in {1}, sorry :(",
                    response.RemoteUser,
                    channel
                );
                writer.SendMessage(
                    msg,
                    response.Channel
                );
            }
            else
            {
                string message = match.Groups["ccmessage"].Value;

                string msg = string.Format(
                    "<{0}@{1}> {2}",
                    response.RemoteUser,
                    response.Channel,
                    message
                );

                writer.SendMessage(
                    msg,
                    channel
                );
            }
        }
    }
}
