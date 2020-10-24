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
    public delegate void ConnectedHandlerAction( ConnectedEventArgs args );

    /// <summary>
    /// Event that gets fired when the bot joins a server.
    /// </summary>
    public class ConnectedEventHandler : IIrcHandler
    {
        // ---------------- Fields ----------------

        private readonly ConnectedEventConfig config;

        private static readonly Regex regex = new Regex(
            @"<chaskis_connect_event>.+</chaskis_connect_event>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled
        );

        // ---------------- Constructor ----------------

        public ConnectedEventHandler( ConnectedEventConfig config )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );

            config.Validate();
            this.config = config.Clone();

            this.KeepHandling = true;
        }

        // ---------------- Properties ----------------

        /// <inheritdoc/>
        public bool KeepHandling { get; set; }

        // ---------------- Functions ----------------

        public void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            Match match = regex.Match( args.Line );
            if( match.Success )
            {
                ConnectedEventArgs connectionArgs = ConnectedEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
                this.config.ConnectedAction( connectionArgs );
            }
        }
    }
}
