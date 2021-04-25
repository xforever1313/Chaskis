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
    public abstract class BaseCoreEventHandler<TConfig> : BaseIrcHandler
        where TConfig : ICoreEventConfig<TConfig>
    {
        // ---------------- Fields ----------------

        protected readonly TConfig config;

        private readonly Regex regex;

        // ---------------- Constructor ----------------

        protected BaseCoreEventHandler( TConfig config, Regex regex ) :
            base()
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );
            ArgumentChecker.IsNotNull( regex, nameof( regex ) );

            config.Validate();
            this.config = config.Clone();
            this.regex = regex;
        }

        // ---------------- Functions ----------------

        public override void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );
            Match match = regex.Match( args.Line );
            if( match.Success )
            {
                HandleEventInternal( args, match );
            }
        }

        /// <summary>
        /// Child class implements this to handle the actual parsing and handling of the event.
        /// </summary>
        protected abstract void HandleEventInternal( HandlerArgs args, Match match );
    }
}
