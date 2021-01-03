//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using SethCS.Exceptions;

namespace Chaskis.Core
{
    public delegate void ReceiveHandlerAction( ReceiveHandlerArgs args );

    /// <summary>
    /// This class will fire for ALL IRC messages that are RECEIVED
    /// and pass in the raw IRC message as the message string.
    /// 
    /// Note, this should really only be used when you want to get ALL output
    /// from the server without any filtering, or there is syntax you expect
    /// from the server that you want but the bot does not support.  To filter
    /// use any of the other handlers.
    /// </summary>
    public sealed class ReceiveHandler : BaseIrcHandler
    {
        // ---------------- Fields ----------------

        private readonly ReceiveHandlerConfig config;

        // ---------------- Constructor ----------------

        public ReceiveHandler( ReceiveHandlerConfig allConfig ) :
            base()
        {
            ArgumentChecker.IsNotNull( allConfig, nameof( allConfig ) );

            allConfig.Validate();

            this.config = allConfig.Clone();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The action to take when ANY message appears from IRC (JOIN, PART, PRIVMSG, PING, etc).
        /// As far as the passed in IrcResponse to the action goes, the channel and remote user
        /// will be String.Empty, since this class does no parsing of the IRC message.
        /// It just grabs the line from the IRC channel and passes it into the AllAction
        /// with no parsing.  It is up to the AllAction to parse the channel and user
        /// name if they so desire.
        /// </summary>
        public ReceiveHandlerAction LineAction
        {
            get
            {
                return this.config.LineAction;
            }
        }

        // ---------------- Functions ----------------

        public override void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            // Do not handle Chaskis Events or inter-plugin events.  This handler is only
            // for receiving messages via IRC.
            if( AnyChaskisEventHandler.Regex.IsMatch( args.Line ) )
            {
                return;
            }
            else if( InterPluginEventHandler.Regex.IsMatch( args.Line ) )
            {
                return;
            }

            ReceiveHandlerArgs allArgs = new ReceiveHandlerArgs( args.IrcWriter, args.Line );

            this.LineAction( allArgs );
        }
    }
}