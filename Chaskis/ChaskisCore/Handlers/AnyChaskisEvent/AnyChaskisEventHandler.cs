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
    public delegate void AnyChaskisEventHandlerAction( AnyChaskisEventHandlerArgs args );

    /// <summary>
    /// This class will fire for ALL chaskis events that are triggered.
    /// 
    /// Note, this should really only be used when you want to get ALL output
    /// from the chaskis event without any filtering.  Really only meant to be used for debugging.
    /// </summary>
    public sealed class AnyChaskisEventHandler : BaseIrcHandler
    {
        // ---------------- Fields ----------------

        private readonly AnyChaskisEventHandlerConfig config;

        internal static readonly Regex Regex = new Regex(
            @"^<chaskis_",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        // ---------------- Constructor ----------------

        public AnyChaskisEventHandler( AnyChaskisEventHandlerConfig allConfig ) :
            base()
        {
            ArgumentChecker.IsNotNull( allConfig, nameof( allConfig ) );

            allConfig.Validate();

            this.config = allConfig.Clone();
        }

        // ---------------- Properties ----------------

        public AnyChaskisEventHandlerAction LineAction
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

            if( Regex.IsMatch( args.Line ) == false )
            {
                return;
            }

            AnyChaskisEventHandlerArgs allArgs = new AnyChaskisEventHandlerArgs( args.IrcWriter, args.Line );
            this.LineAction( allArgs );
        }
    }
}