//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public class InterPluginEventFactory
    {
        // ---------------- Fields ----------------

        private readonly Dictionary<string, IInterPluginEventCreator> eventCreators;

        private static InterPluginEventFactory instance;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Only a single instance is allowed to be created.
        /// This way, plugins can't create their own instance and fake events.
        /// Calling this function at all will result in a <see cref="InvalidOperationException"/>
        /// </summary>
        /// <returns>The single instance of this class.</returns>
        public static InterPluginEventFactory CreateInstance( IReadOnlyList<string> pluginNameList )
        {
            if( instance == null )
            {
                instance = new InterPluginEventFactory( pluginNameList );
            }
            else
            {
                throw new InvalidOperationException(
                    nameof( InterPluginEventFactory ) + " instace already created."
                );
            }

            return instance;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        private InterPluginEventFactory( IReadOnlyList<string> pluginNameList )
        {
            ArgumentChecker.IsNotNull( pluginNameList, nameof( pluginNameList ) );

            this.eventCreators = new Dictionary<string, IInterPluginEventCreator>();
            foreach( string s in pluginNameList )
            {
                this.eventCreators.Add( s, new InterPluginEventCreator( s ) );
            }

            this.EventCreators = new ReadOnlyDictionary<string, IInterPluginEventCreator>( this.eventCreators );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Read-only dictionary of event creators 
        /// </summary>
        public IReadOnlyDictionary<string, IInterPluginEventCreator> EventCreators { get; private set; }

        // ---------------- Functions ----------------

        // ---------------- Helper Classes ----------------

        private class InterPluginEventCreator : IInterPluginEventCreator
        {
            // ---------------- Fields ----------------

            private readonly string sourcePlugin;

            // ---------------- Constructor ----------------

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="pluginName">The associated plugin.</param>
            public InterPluginEventCreator( string pluginName )
            {
                this.sourcePlugin = pluginName;
            }

            // ---------------- Functions ----------------

            public InterPluginEvent CreateBcastEvent(
                IReadOnlyDictionary<string, string> args,
                IReadOnlyDictionary<string, string> passThroughArgs = null
            )
            {
                ArgumentChecker.IsNotNull( args, nameof( args ) );

                return new InterPluginEvent(
                    this.sourcePlugin,
                    null,
                    args,
                    passThroughArgs
                );
            }

            public InterPluginEvent CreateTargetedEvent(
                string targetPluginName,
                IReadOnlyDictionary<string, string> args,
                IReadOnlyDictionary<string, string> passThroughArgs = null
            )
            {
                ArgumentChecker.IsNotNull( args, nameof( args ) );
                ArgumentChecker.StringIsNotNullOrEmpty( targetPluginName, nameof( targetPluginName ) );

                return new InterPluginEvent(
                    this.sourcePlugin,
                    targetPluginName,
                    args,
                    passThroughArgs
                );
            }

            public InterPluginEventHandler CreatePluginEventHandler(
                Action<ChaskisEventHandlerLineActionArgs> lineAction
            )
            {
                return new InterPluginEventHandler(
                    null,
                    this.sourcePlugin,
                    lineAction
                );
            }

            public InterPluginEventHandler CreatePluginEventHandler(
                string expectedSourcePlugin,
                Action<ChaskisEventHandlerLineActionArgs> lineAction
            )
            {
                return new InterPluginEventHandler(
                    expectedSourcePlugin,
                    this.sourcePlugin,
                    lineAction
                );
            }
        }
    }

    public interface IInterPluginEventCreator
    {
        /// <summary>
        /// Generates a ChaskisEvent that will be transmitted
        /// to ALL plugins.
        /// 
        /// This does NOT send the event, but simply creates it.
        /// </summary>
        /// <param name="args">The args to pass into the plugin.  Key is the argument name, value is the argument's value.</param>
        /// <param name="passThroughArgs">
        /// Optional parameter.
        /// When these arguments are passed in, they are ignored by a plugin that generates a response.
        /// Instead, they become a part of the response.
        /// Key is the argument name, value is the argument's value.
        /// </param>
        /// <returns>A Chaskis event that is ready to be fired.</returns>
        InterPluginEvent CreateBcastEvent(
            IReadOnlyDictionary<string, string> args,
            IReadOnlyDictionary<string, string> passThroughArgs = null
        );

        /// <summary>
        /// Generates a ChaskisEvent that is meant to be directed to
        /// a specfic plugin.
        /// </summary>
        /// <param name="targetPluginName">The target plugin name</param>
        /// <param name="args">The args to pass into the plugin.  Key is the argument name, value is the argument's value.</param>
        /// <param name="passThroughArgs">
        /// Optional parameter.
        /// When these arguments are passed in, they are ignored by a plugin that generates a response.
        /// Instead, they become a part of the response.
        /// Key is the argument name, value is the argument's value.
        /// </param>
        /// <returns>A Chaskis event that is ready to be fired.</returns>
        InterPluginEvent CreateTargetedEvent(
            string targetPluginName,
            IReadOnlyDictionary<string, string> args,
            IReadOnlyDictionary<string, string> passThroughArgs = null
        );

        /// <summary>
        /// Creates an event handler that waits for a PLUGIN event from
        /// ANY plugin targeted towards this plugin or is a BCAST.
        /// </summary>
        /// <param name="argPattern">
        /// Chaskis events have arguments after the source and plugin.
        /// You can filter events based on its arguments with this parameter
        /// and only capture events whose arguments match this pattern.
        /// </param>
        /// <param name="expectedSourcePlugin">The SPECIFIC plugin we want to listen to.</param>
        /// <param name="lineAction">The action to take when our arg pattern matches.</param>
        /// <returns></returns>
        InterPluginEventHandler CreatePluginEventHandler(
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        );

        /// <summary>
        /// Creates an event handler that waits for a PLUGIN event from
        /// a SPECIFIC Plugin (instead of any) targeted towards this plugin
        /// or is a BCAST.
        /// </summary>
        /// <param name="argPattern">
        /// Chaskis events have arguments after the source and plugin.
        /// You can filter events based on its arguments with this parameter
        /// and only capture events whose arguments match this pattern.
        /// </param>
        /// <param name="expectedSourcePlugin">
        /// The SPECIFIC plugin we expect the event to come from.
        /// Null for ANY plugin.
        /// </param>
        /// <param name="lineAction">The action to take when our arg pattern matches.</param>
        /// <returns></returns>
        InterPluginEventHandler CreatePluginEventHandler(
            string expectedSourcePlugin,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        );
    }
}
