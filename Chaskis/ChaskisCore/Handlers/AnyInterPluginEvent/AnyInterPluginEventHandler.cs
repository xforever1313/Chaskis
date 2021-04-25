//
//          Copyright Seth Hendrick 2016-2021.
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
    public sealed class AnyInterPluginEventHandler : BaseIrcHandler
    {
        // ---------------- Fields ----------------

        private readonly AnyInterPluginEventHandlerConfig config;

        private static readonly Regex chaskisEventRegex = new Regex(
            $@"^\<{InterPluginEventExtensions.XmlRootName}.+",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        // ---------------- Constructor ----------------

        public AnyInterPluginEventHandler( AnyInterPluginEventHandlerConfig allConfig ) :
            base()
        {
            ArgumentChecker.IsNotNull( allConfig, nameof( allConfig ) );

            allConfig.Validate();

            this.config = allConfig.Clone();
        }

        // ---------------- Properties ----------------
        
        public AnyInterPluginEventHandlerAction LineAction
        {
            get
            {
                return this.config.LineAction;
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Handles the event.
        /// </summary>
        public override void HandleEvent( HandlerArgs args )
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