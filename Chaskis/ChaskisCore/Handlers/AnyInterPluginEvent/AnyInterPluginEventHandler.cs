//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public delegate void AnyInterPluginEventHandlerAction( AnyInterPluginEventHandlerArgs args );

    /// <summary>
    /// This class will fire for ALL inter-plugin events that are triggered.
    /// 
    /// Note, this should really only be used when you want to get ALL output
    /// from the inter-plugin event without any filtering.  Really only meant to be used for debugging.
    /// </summary>
    public sealed class AnyInterPluginEventHandler : IIrcHandler
    {
        // ---------------- Fields ----------------

        private readonly AnyInterPluginEventHandlerConfig config;

        private static readonly Regex chaskisEventRegex = new Regex(
            $@"^\<{InterPluginEventExtensions.XmlRootName}.+",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        // ---------------- Constructor ----------------

        public AnyInterPluginEventHandler( AnyInterPluginEventHandlerConfig allConfig )
        {
            ArgumentChecker.IsNotNull( allConfig, nameof( allConfig ) );

            allConfig.Validate();

            this.config = allConfig.Clone();
            this.KeepHandling = true;
        }

        // ---------------- Properties ----------------
        
        public AnyInterPluginEventHandlerAction LineAction
        {
            get
            {
                return this.config.LineAction;
            }
        }

        /// <summary>
        /// Whether or not the handler should keep handling or not.
        /// Set to true to keep handling the event when it appears in the chat.
        /// Set to false so when the current IRC message is finished processing being,
        /// it leaves the event queue and never
        /// happens again.   Useful for events that only need to happen once.
        ///
        /// This is a public get/set.  Either classes outside of the handler can
        /// tell the handler to cancel the event, or it can cancel itself.
        ///
        /// Note: when this is set to false, there must be one more IRC message that appears
        /// before it is removed from the queue.
        ///
        /// Defaulted to true.
        /// </summary>
        public bool KeepHandling { get; set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Handles the event.
        /// </summary>
        public void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            if( chaskisEventRegex.IsMatch( args.Line ) == false )
            {
                return;
            }

            AnyInterPluginEventHandlerArgs allArgs = new AnyInterPluginEventHandlerArgs( args.IrcWriter, args.Line );
            this.LineAction( allArgs );
        }
    }
}