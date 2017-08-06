//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Text;

namespace ChaskisCore
{
    /// <summary>
    /// This class represents an event that can be sent
    /// to all plugins, without the need of plugins
    /// to be aware of each other.
    /// </summary>
    public class ChaskisEvent
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// String for a broadcast event.
        /// </summary>
        public const string BroadcastEventStr = "BCAST";

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// Only classes inside of ChaskisCore can create instances of this class directly.
        /// All plugins must use <see cref="ChaskisEventFactory"/>
        /// </summary>
        /// <param name="sourcePlugin">The plugin that generated this event.</param>
        /// <param name="destinationPlugin">
        /// The plugin this event wishes to talk to.
        /// 
        /// Null for a "broadcast" (all plugins may listen).
        /// </param>
        internal ChaskisEvent(
            ChaskisEventSource sourceType,
            string sourcePlugin,
            string destinationPlugin,
            IList<string> args
        )
        {
            this.SourceType = sourceType;
            this.SourcePlugin = sourcePlugin;
            if( destinationPlugin == null )
            {
                this.DestinationPlugin = BroadcastEventStr;
            }
            else
            {
                this.DestinationPlugin = destinationPlugin;
            }
            this.Args = args;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Did this event come from the CORE or from a Plugin?
        /// </summary>
        public ChaskisEventSource SourceType { get; private set; }

        /// <summary>
        /// Which plugin created the event.
        /// </summary>
        public string SourcePlugin { get; private set; }

        /// <summary>
        /// Which plugin are we directing the event to?
        /// BCAST for a broadcast for all plugins.
        /// </summary>
        public string DestinationPlugin { get; private set; }

        /// <summary>
        /// Arguments for the event.
        /// </summary>
        public IList<string> Args { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Creates the event string.
        /// </summary>
        /// <returns>CHASKIS PLUGIN SourcePluginName DestPluginName Arg1 Arg2 Arg3</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            // We are a CHASKIS EVENT.
            builder.Append( "CHASKIS" );
            builder.Append( " " );

            // Source type comes next.
            builder.Append( this.SourceType );
            builder.Append( " " );

            // Source plugin comes next.
            builder.Append( this.SourcePlugin );
            builder.Append( " " );

            // Destination Plugin goes next.  If null, this becomes "BCAST" for broadcast.
            builder.Append( this.DestinationPlugin );
            builder.Append( " " );

            // Last comes the args.
            if( this.Args != null )
            {
                foreach( string arg in this.Args )
                {
                    builder.Append( arg );
                    builder.Append( " " );
                }
            }

            return builder.ToString().TrimEnd();
        }
    }
}
