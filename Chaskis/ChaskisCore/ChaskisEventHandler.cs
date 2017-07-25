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

        private const string chaskisPattern = @"CHASKIS\s+(?<source>CORE|PLUGIN)\s+(?<plugin>\S+)\s+(?<args>.+)";

        private static readonly Regex chaskisRegex = new Regex(
            chaskisPattern,
            RegexOptions.Compiled
        );

        private ChaskisEventSource expectedSource;
        private string expectedPlugin;
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
        public ChaskisEventHandler(
            string argPattern,
            ChaskisEventProtocol expectedProtocol,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        ) : this( argPattern, ChaskisEventSource.CORE, expectedProtocol.ToString(), lineAction )
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
        /// <param name="expectedPlugin">
        /// The expected plugin.
        /// Must be the ToString value of <see cref="ChaskisEventProtocol"/> 
        /// if expectedSource is set to <see cref="ChaskisEventSource.CORE"/>.
        /// </param>
        public ChaskisEventHandler(
            string argPattern,
            ChaskisEventSource expectedSource,
            string expectedPlugin,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        )
        {
            if( expectedSource == ChaskisEventSource.CORE )
            {
                ChaskisEventProtocol protocol;
                if( Enum.TryParse( expectedPlugin, out protocol ) == false )
                {
                    throw new ArgumentException(
                        "Invalid protocol passed into constructor.  Ensure you want a CORE event. Got: " + expectedPlugin
                    );
                }
            }

            ArgumentChecker.IsNotNull( argPattern, nameof( argPattern ) );
            ArgumentChecker.IsNotNull( lineAction, nameof( lineAction ) );
            ArgumentChecker.StringIsNotNullOrEmpty( expectedPlugin, nameof( expectedPlugin ) );

            this.argRegex = new Regex( argPattern );
            this.expectedSource = expectedSource;
            this.expectedPlugin = expectedPlugin.ToUpper();
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
                        isMatch &= Enum.TryParse( match.Groups["plugin"].Value, out protocol );

                        pluginName = protocol.ToString().ToUpper();
                    }
                    else
                    {
                        pluginName = match.Groups["plugin"].Value.ToUpper();
                    }

                    isMatch &= ( this.expectedSource == source );
                    isMatch &= ( this.expectedPlugin == pluginName );

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
                                argMatch
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
            Match argMatch
        )
        {
            this.PluginName = pluginName;
            this.EventArgs = eventArgs;
            this.Regex = regex;
            this.Match = argMatch;
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
    }
}
