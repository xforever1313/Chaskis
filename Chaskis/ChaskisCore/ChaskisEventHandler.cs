//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace ChaskisCore
{
    /// <summary>
    /// This handler are for events that chaskis fires.
    /// Plugins can also fire this event.
    /// 
    /// This class allows plugins to talk to other plugins.  Plugins can create these events
    /// and other plugins can "subscribe" to those events.
    /// </summary>
    public class ChaskisEventHandler : IIrcHandler
    {
        // ---------------- Fields ---------------

        /// <summary>
        /// Pattern to watch for Chaskis Events.
        /// </summary>
        //                                         1                2                       3                        4                5
        // 1 - Denotes that we are a CHASKIS event.
        // 2 - Did this event come from the CORE or from a Plugin?
        // 3 - What fired this event?
        // 4 - What plugin was the event trying to get the attention of? BCAST for ALL plugins.
        // 5 - Arguments.
        private const string chaskisPattern = @"CHASKIS\s+(?<source>CORE|PLUGIN)\s+(?<sourcePlugin>\S+)\s+(?<targetPlugin>\S+)\s+(?<args>.+)";

        private static readonly Regex chaskisRegex = new Regex(
            chaskisPattern,
            RegexOptions.Compiled
        );

        private ChaskisEventSource expectedSource;
        private string expectedPlugin;
        private string creatorPlugin;
        private Action<ChaskisEventHandlerLineActionArgs> lineAction;
        private Regex argRegex;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.  Use this to capture a CORE event.
        /// </summary>
        /// <param name="argPattern">
        /// Chaskis events have arguments after the source and plugin.
        /// You can filter events based on its arguments with this parameter
        /// and only capture events whose arguments match this pattern.
        /// </param>
        /// <param name="expectedProtocol">The protocol we expected.</param>
        /// <param name="creatorPluginName">The plugin that created this event.</param>
        internal ChaskisEventHandler(
            string argPattern,
            ChaskisEventProtocol? expectedProtocol,
            string creatorPluginName,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        ) : this(
            argPattern,
            ChaskisEventSource.CORE,
            expectedProtocol.HasValue ? expectedProtocol.Value.ToString() : null,
            creatorPluginName,
            lineAction
        )
        {
        }

        /// <summary>
        /// Constructor.  Use this to capture a PLUGIN event.
        /// </summary>
        /// <param name="argPattern">
        /// Chaskis events have arguments after the source and plugin.
        /// You can filter events based on its arguments with this parameter
        /// and only capture events whose arguments match this pattern.
        /// </param>
        /// <param name="expectedSource">
        /// The expected source.
        /// </param>
        /// <param name="expectedSourcePlugin">
        /// The expected plugin.
        /// Must be the ToString value of <see cref="ChaskisEventProtocol"/> 
        /// if expectedSource is set to <see cref="ChaskisEventSource.CORE"/>.
        /// </param>
        /// <param name="creatorPluginName">The plugin that created this event.</param>
        internal ChaskisEventHandler(
            string argPattern,
            ChaskisEventSource expectedSource,
            string expectedSourcePlugin,
            string creatorPluginName,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        )
        {
            if( expectedSource == ChaskisEventSource.CORE )
            {
                ChaskisEventProtocol protocol;
                if( Enum.TryParse( expectedSourcePlugin, out protocol ) == false )
                {
                    throw new ArgumentException(
                        "Invalid protocol passed into constructor.  Ensure you want a CORE event. Got: " + expectedSourcePlugin
                    );
                }
            }

            ArgumentChecker.IsNotNull( argPattern, nameof( argPattern ) );
            ArgumentChecker.IsNotNull( lineAction, nameof( lineAction ) );

            this.argRegex = new Regex( argPattern, RegexOptions.IgnoreCase ); // TODO: Maybe add a regex option?
            this.expectedSource = expectedSource;

            if( expectedSourcePlugin != null )
            {
                this.expectedPlugin = expectedSourcePlugin.ToUpper();
            }
            else
            {
                // Handle events from ALL plugins.
                this.expectedPlugin = null;
            }

            this.creatorPlugin = creatorPluginName.ToUpper();

            this.lineAction = lineAction;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// <see cref="IIrcHandler.KeepHandling"/>
        /// </summary>
        public bool KeepHandling { get; set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// <see cref="IIrcHandler.HandleEvent(HandlerArgs)"/>
        /// </summary>
        public void HandleEvent( HandlerArgs args )
        {
            Match match = chaskisRegex.Match( args.Line );
            if( match.Success )
            {
                ChaskisEventSource source;
                bool isMatch = Enum.TryParse( match.Groups["source"].Value, out source );
                if( isMatch )
                {
                    string pluginName;
                    if( source == ChaskisEventSource.CORE )
                    {
                        ChaskisEventProtocol protocol;
                        isMatch &= Enum.TryParse( match.Groups["sourcePlugin"].Value, out protocol );

                        pluginName = protocol.ToString().ToUpper();
                    }
                    else
                    {
                        pluginName = match.Groups["sourcePlugin"].Value.ToUpper();
                    }

                    isMatch &= ( this.expectedSource == source );

                    // Handle event if the event targets this plugin, OR it is a broadcast event.
                    string targetPlugin = match.Groups["targetPlugin"].Value.ToUpper();
                    isMatch &= ( ( this.creatorPlugin == targetPlugin ) || ( ChaskisEvent.BroadcastEventStr == targetPlugin ) );

                    if( this.expectedPlugin != null )
                    {
                        isMatch &= ( this.expectedPlugin == pluginName );
                    }
                    // BCast events MUST be subcribed to a specific source plugin.
                    // Otherwise, what happens if we get something like this:
                    // CHASKIS PLUGIN Plugin1 BCAST Hello
                    // CHASKIS PLUGIN Plugin2 BCAST Hello
                    // BOTH will trigger even though they came from two different
                    // plugins.  That is a problem...
                    // For BCAST events, the handler should subscribe to a specific
                    // source plugin.  This way Plugin1 will trigger one handler,
                    // while Plugin2 will trigger a different handler.
                    else if( targetPlugin == ChaskisEvent.BroadcastEventStr )
                    {
                        isMatch = false;
                    }
                    // Otherwise, if we are not a BCAST, we'll handle the event if our expected plugin
                    // can be any (is null).

                    if( isMatch )
                    {
                        string eventArgsStr = match.Groups["args"].Value;
                        Match argMatch = this.argRegex.Match( eventArgsStr );

                        if( argMatch.Success )
                        {
                            ChaskisEventHandlerLineActionArgs eventArgs = new ChaskisEventHandlerLineActionArgs(
                                pluginName,
                                match.Groups["args"].Value,
                                this.argRegex,
                                argMatch,
                                args.IrcWriter
                            );

                            this.lineAction( eventArgs );
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Event arguments for <see cref="ChaskisEventHandler"/> action if the line matches
    /// a regex.
    /// </summary>
    public class ChaskisEventHandlerLineActionArgs
    {
        // ---------------- Constructor ----------------

        public ChaskisEventHandlerLineActionArgs(
            string pluginName,
            string eventArgs,
            Regex regex,
            Match argMatch,
            IIrcWriter ircWriter
        )
        {
            this.PluginName = pluginName;
            this.EventArgs = eventArgs;
            this.Regex = regex;
            this.Match = argMatch;
            this.IrcWriter = ircWriter;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The plugin that triggered the chaskis event.
        /// </summary>
        public string PluginName { get; private set; }

        /// <summary>
        /// Event args
        /// </summary>
        public string EventArgs { get; private set; }

        /// <summary>
        /// The regex that was used to find this event.
        /// </summary>
        public Regex Regex { get; private set; }

        /// <summary>
        /// The regex match that was used to find this event.
        /// </summary>
        public Match Match { get; private set; }

        /// <summary>
        /// The IRC writer that can be used to send messages to
        /// the IRC channel.
        /// </summary>
        public IIrcWriter IrcWriter { get; private set; }
    }
}
