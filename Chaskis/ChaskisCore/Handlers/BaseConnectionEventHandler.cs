//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public abstract class BaseConnectionEventHandler<TConfig> : IIrcHandler
        where TConfig : IConnectionEventConfig<TConfig>
    {
        // ---------------- Fields ----------------

        protected readonly TConfig config;

        private readonly Regex regex;

        // ---------------- Constructor ----------------

        protected BaseConnectionEventHandler( TConfig config, Regex regex )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );
            ArgumentChecker.IsNotNull( regex, nameof( regex ) );

            config.Validate();
            this.config = config.Clone();
            this.regex = regex;

            this.KeepHandling = true;
        }

        // ---------------- Properties ----------------

        /// <inheritdoc/>
        /// <remarks>
        /// Defaulted to true.  Override to false in the constructor
        /// of the child class to not do that.
        /// </remarks>
        public bool KeepHandling { get; set; }

        // ---------------- Functions ----------------

        public void HandleEvent( HandlerArgs args )
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
