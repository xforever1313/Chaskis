//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Chaskis.Core;

namespace Chaskis.Plugins.CrossChannel
{
    /// <summary>
    /// This plugin allows people to broadcast to all channels
    /// the bot is in, or send messages from one channel
    /// to another.
    /// </summary>
    [ChaskisPlugin( "crosschannel" )]
    public class CrossChannelPlugin : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string VersionStr = "0.3.2";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private IReadOnlyIrcConfig ircConfig;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor
        /// </summary>
        public CrossChannelPlugin()
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

            {
                MessageHandlerConfig config = new MessageHandlerConfig
                {
                    LineRegex = @"^!broadcast\s+(?<bcastmsg>.+)",
                    LineAction = this.BroadcastHandler
                };

                MessageHandler bcastHandler = new MessageHandler(
                    config
                );
                this.handlers.Add( bcastHandler );
            }

            {
                MessageHandlerConfig config = new MessageHandlerConfig
                {
                    LineRegex = @"^!cc\s+\<(?<channel>\S+)\>\s+(?<ccmessage>.+)",
                    LineAction = this.CCHandler
                };

                MessageHandler ccHandler = new MessageHandler(
                    config
                );
                this.handlers.Add( ccHandler );
            }
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            StringBuilder builder = new StringBuilder();

            builder.Append( "@" + msgArgs.User + ": " );

            if( helpArgs.Length == 0 )
            {
                builder.Append( "Append 'broadcast', or 'cc' to the help message you just sent to get more information about each command." );
            }
            else if( helpArgs[0] == "broadcast" )
            {
                builder.Append( "Broadcasts a message to ALL channels I am in. Command: !broadcast Your Message Here."  );
            }
            else if( helpArgs[0] == "cc" )
            {
                builder.Append( "Sends a message to another channel I am in from this one.  Command: !cc <#channelname> Your Message Here." );
            }
            else
            {
                builder.Append( "That is not a command I know..." );
            }

            msgArgs.Writer.SendMessage(
                builder.ToString(),
                msgArgs.Channel
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

        // ---------------- Handlers ----------------

        /// <summary>
        /// Handles the broadcast command.
        /// </summary>
        private void BroadcastHandler( MessageHandlerArgs args )
        {
            Match match = args.Match;
            string message = match.Groups["bcastmsg"].Value;

            string msg = string.Format(
                "<{0}@{1}> {2}",
                args.User,
                args.Channel,
                message
            );

            args.Writer.SendBroadcastMessage( msg );
        }

        /// <summary>
        /// Handles the CC Command.
        /// </summary>
        private void CCHandler( MessageHandlerArgs args )
        {
            Match match = args.Match;
            string channel = match.Groups["channel"].Value.ToLower();

            // Can only send a message to a channel we are actually in.
            if( this.ircConfig.Channels.Contains( channel ) == false )
            {
                string msg = string.Format(
                    "@{0}: I am not in {1}, sorry :(",
                    args.User,
                    channel
                );
                args.Writer.SendMessage(
                    msg,
                    args.Channel
                );
            }
            else
            {
                string message = match.Groups["ccmessage"].Value;

                string msg = string.Format(
                    "<{0}@{1}> {2}",
                    args.User,
                    args.Channel,
                    message
                );

                args.Writer.SendMessage(
                    msg,
                    channel
                );
            }
        }
    }
}
