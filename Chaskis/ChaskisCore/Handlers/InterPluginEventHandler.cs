//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    /// <summary>
    /// This handler are for events that chaskis fires.
    /// Plugins can also fire this event.
    /// 
    /// This class allows plugins to talk to other plugins.  Plugins can create these events
    /// and other plugins can "subscribe" to those events.
    /// </summary>
    public class InterPluginEventHandler : BaseIrcHandler
    {
        // ---------------- Fields ---------------

        private static string interPluginHelper = $@"^\<{InterPluginEventExtensions.XmlRootName}.+\</{InterPluginEventExtensions.XmlRootName}\>$";

        internal static readonly Regex Regex = new Regex(
            interPluginHelper,
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        private readonly string expectedPlugin;
        private readonly string creatorPlugin;
        private readonly Action<ChaskisEventHandlerLineActionArgs> lineAction;

        // ---------------- Constructor ----------------

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
        internal InterPluginEventHandler(
            string expectedSourcePlugin,
            string creatorPluginName,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        )
        {
            ArgumentChecker.IsNotNull( lineAction, nameof( lineAction ) );

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

        // ---------------- Functions ----------------

        public override void HandleEvent( HandlerArgs args )
        {
            if( Regex.IsMatch( args.Line ) == false )
            {
                return;
            }
            
            InterPluginEvent e = InterPluginEventExtensions.FromXml( args.Line );
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
    /// Event arguments for <see cref="InterPluginEventHandler"/> action if the line matches
    /// a regex.
    /// </summary>
    public class ChaskisEventHandlerLineActionArgs
    {
        // ---------------- Constructor ----------------

        public ChaskisEventHandlerLineActionArgs(
            string pluginName,
            IReadOnlyDictionary<string, string> eventArgs,
            IReadOnlyDictionary<string, string> passThroughArgs,
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
        public IReadOnlyDictionary<string, string> EventArgs { get; private set; }

        public IReadOnlyDictionary<string, string> PassThroughArgs { get; private set; }

        /// <summary>
        /// The IRC writer that can be used to send messages to
        /// the IRC channel.
        /// </summary>
        public IIrcWriter IrcWriter { get; private set; }
    }
}
