//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
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
        private const string chaskisPattern = @"^\<\?xml\s+version=""1.0""\s+encoding=""utf-16""\?\>\<chaskis_event.+\</chaskis_event\>$";

        private static readonly Regex chaskisRegex = new Regex(
            chaskisPattern,
            RegexOptions.Compiled
        );

        private readonly ChaskisEventSource expectedSource;
        private readonly string expectedPlugin;
        private readonly string creatorPlugin;
        private readonly Action<ChaskisEventHandlerLineActionArgs> lineAction;

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
            ChaskisEventProtocol? expectedProtocol,
            string creatorPluginName,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        ) : this(
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

            ArgumentChecker.IsNotNull( lineAction, nameof( lineAction ) );

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
            if( chaskisRegex.IsMatch( args.Line ) == false )
            {
                return;
            }

            ChaskisEvent e = ChaskisEvent.FromXml( args.Line );
            string targetPlugin = e.DestinationPlugin;

            // We'll handle the event if it is targeted specifically to this plugin, OR it is a broadcast event.
            bool sendEvent = ( this.creatorPlugin == targetPlugin ) || ( string.IsNullOrEmpty( targetPlugin ) );

            if( this.expectedPlugin != null )
            {
                sendEvent &= ( e.SourcePlugin == this.expectedPlugin );
            }
            // BCAST Events MUST be subscribed to a specific source plugin.
            // Otherwise, what happens if this happens (remember, an empty string for dest_plugin is a bcast):
            // <chaskis_event source_type="PLUGIN" source_plugin="plugin1" dest_plugin="">
            // <chaskis_event source_type="PLUGIN" source_plugin="plugin2" dest_plugin="">
            // Both events will trigger even though they came from two different plugins...
            // That is probably not good.
            // For BCAST events, the handler should subscribe to a specific source plugin.
            // This way, plugin1 will trigger one handler, while plugin 2 will
            // trigger a different handler.
            else if( string.IsNullOrEmpty( targetPlugin ) )
            {
                sendEvent = false;
            }

            if( sendEvent )
            {
                ChaskisEventHandlerLineActionArgs eventArgs = new ChaskisEventHandlerLineActionArgs(
                    e.SourcePlugin,
                    e.Args,
                    e.PassThroughArgs,
                    args.IrcWriter
                );

                this.lineAction( eventArgs );
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
            IDictionary<string, string> eventArgs,
            IDictionary<string, string> passThroughArgs,
            IIrcWriter ircWriter
        )
        {
            this.PluginName = pluginName;
            this.EventArgs = eventArgs;
            this.PassThroughArgs = passThroughArgs;
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
        public IDictionary<string, string> EventArgs { get; private set; }

        public IDictionary<string, string> PassThroughArgs { get; private set; }

        /// <summary>
        /// The IRC writer that can be used to send messages to
        /// the IRC channel.
        /// </summary>
        public IIrcWriter IrcWriter { get; private set; }
    }
}
