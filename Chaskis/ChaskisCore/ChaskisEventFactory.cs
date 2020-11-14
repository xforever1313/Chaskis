//
//          Copyright Seth Hendrick 2017-2018.
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
    public class ChaskisEventFactory
    {
        // ---------------- Fields ----------------

        private readonly Dictionary<string, IChaskisEventCreator> eventCreators;

        private static ChaskisEventFactory instance;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Only a single instance is allowed to be created.
        /// This way, plugins can't create their own instance and fake events.
        /// Calling this function at all will result in a <see cref="InvalidOperationException"/>
        /// </summary>
        /// <returns>The single instance of this class.</returns>
        public static ChaskisEventFactory CreateInstance( IReadOnlyList<string> pluginNameList )
        {
            if( instance == null )
            {
                instance = new ChaskisEventFactory( pluginNameList );
            }
            else
            {
                throw new InvalidOperationException(
                    nameof( ChaskisEventFactory ) + " instace already created."
                );
            }

            return instance;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        private ChaskisEventFactory( IReadOnlyList<string> pluginNameList )
        {
            ArgumentChecker.IsNotNull( pluginNameList, nameof( pluginNameList ) );

            this.eventCreators = new Dictionary<string, IChaskisEventCreator>();
            foreach( string s in pluginNameList )
            {
                this.eventCreators.Add( s, new ChaskisEventCreator( s ) );
            }

            this.EventCreators = new ReadOnlyDictionary<string, IChaskisEventCreator>( this.eventCreators );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Read-only dictionary of event creators 
        /// </summary>
        public IReadOnlyDictionary<string, IChaskisEventCreator> EventCreators { get; private set; }

        // ---------------- Functions ----------------

        // ---------------- Helper Classes ----------------

        private class ChaskisEventCreator : IChaskisEventCreator
        {
            // ---------------- Fields ----------------

            private readonly string sourcePlugin;

            // ---------------- Constructor ----------------

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="pluginName">The associated plugin.</param>
            public ChaskisEventCreator( string pluginName )
            {
                this.sourcePlugin = pluginName;
            }

            // ---------------- Functions ----------------

            public ChaskisEvent CreateBcastEvent( IDictionary<string, string> args, IDictionary<string, string> passThroughArgs = null )
            {
                ArgumentChecker.IsNotNull( args, nameof( args ) );

                return new ChaskisEvent(
                    ChaskisEventSource.PLUGIN,
                    this.sourcePlugin,
                    null,
                    new Dictionary<string, string>( args ),
                    ( passThroughArgs != null ) ? new Dictionary<string, string>( passThroughArgs ) : null
                );
            }

            public ChaskisEvent CreateTargetedEvent( string targetPluginName, IDictionary<string, string> args, IDictionary<string, string> passThroughArgs = null )
            {
                ArgumentChecker.IsNotNull( args, nameof( args ) );
                ArgumentChecker.StringIsNotNullOrEmpty( targetPluginName, nameof( targetPluginName ) );

                return new ChaskisEvent(
                    ChaskisEventSource.PLUGIN,
                    this.sourcePlugin,
                    targetPluginName,
                    new Dictionary<string, string>( args ),
                    ( passThroughArgs != null ) ? new Dictionary<string, string>( passThroughArgs ) : null
                );
            }

            public ChaskisEventHandler CreatePluginEventHandler(
                Action<ChaskisEventHandlerLineActionArgs> lineAction
            )
            {
                return new ChaskisEventHandler(
                    ChaskisEventSource.PLUGIN,
                    null,
                    this.sourcePlugin,
                    lineAction
                );
            }

            public ChaskisEventHandler CreatePluginEventHandler(
                string expectedSourcePlugin,
                Action<ChaskisEventHandlerLineActionArgs> lineAction
            )
            {
                return new ChaskisEventHandler(
                    ChaskisEventSource.PLUGIN,
                    expectedSourcePlugin,
                    this.sourcePlugin,
                    lineAction
                );
            }
        }
    }

    public interface IChaskisEventCreator
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
        ChaskisEvent CreateBcastEvent( IDictionary<string, string> args, IDictionary<string, string> passThroughArgs = null );

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
        ChaskisEvent CreateTargetedEvent(
            string targetPluginName,
            IDictionary<string, string> args,
            IDictionary<string, string> passThroughArgs = null
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
        ChaskisEventHandler CreatePluginEventHandler(
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
        ChaskisEventHandler CreatePluginEventHandler(
            string expectedSourcePlugin,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        );
    }
}
