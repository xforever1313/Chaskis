//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SethCS.Exceptions;

namespace ChaskisCore
{
    public class ChaskisEventFactory
    {
        // ---------------- Fields ----------------

        private Dictionary<string, IChaskisEventCreator> eventCreators;

        private static ChaskisEventFactory instance;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Only a single instance is allowed to be created.
        /// This way, plugins can't create their own instance and fake events.
        /// Calling this function at all will result in a <see cref="InvalidOperationException"/>
        /// </summary>
        /// <returns>The single instance of this class.</returns>
        public static ChaskisEventFactory CreateInstance( IList<string> pluginNameList )
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
        private ChaskisEventFactory( IList<string> pluginNameList )
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

            private string sourcePlugin;

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

            /// <summary>
            /// <see cref="IChaskisEventCreator.CreateBcastEvent(IList{string})"/>
            /// </summary>
            public ChaskisEvent CreateBcastEvent( IList<string> args )
            {
                ArgumentChecker.IsNotNull( args, nameof( args ) );

                return new ChaskisEvent(
                    ChaskisEventSource.PLUGIN,
                    this.sourcePlugin,
                    null,
                    new List<string>( args )
                );
            }

            /// <summary>
            /// <see cref="IChaskisEventCreator.CreateTargetedEvent(string, IList{string})"/>
            /// </summary>
            public ChaskisEvent CreateTargetedEvent( string targetPluginName, IList<string> args )
            {
                ArgumentChecker.IsNotNull( args, nameof( args ) );
                ArgumentChecker.StringIsNotNullOrEmpty( targetPluginName, nameof( targetPluginName ) );

                return new ChaskisEvent(
                    ChaskisEventSource.PLUGIN,
                    this.sourcePlugin,
                    targetPluginName,
                    new List<string>( args )
                );
            }

            /// <summary>
            /// <see cref="IChaskisEventCreator.CreateCoreEventHandler(string, ChaskisEventProtocol, Action{ChaskisEventHandlerLineActionArgs})"/>
            /// </summary>
            public ChaskisEventHandler CreateCoreEventHandler(
                string argPattern,
                ChaskisEventProtocol? expectedProtocol,
                Action<ChaskisEventHandlerLineActionArgs> lineAction
            )
            {
                return new ChaskisEventHandler(
                    argPattern,
                    expectedProtocol,
                    this.sourcePlugin,
                    lineAction
                );
            }

            /// <summary>
            /// <see cref="IChaskisEventCreator.CreatePluginEventHandler(string, Action{ChaskisEventHandlerLineActionArgs})"/>
            /// </summary>
            public ChaskisEventHandler CreatePluginEventHandler(
                string argPattern,
                Action<ChaskisEventHandlerLineActionArgs> lineAction
            )
            {
                return new ChaskisEventHandler(
                    argPattern,
                    ChaskisEventSource.PLUGIN,
                    null,
                    this.sourcePlugin,
                    lineAction
                );
            }

            /// <summary>
            /// <see cref="IChaskisEventCreator.CreatePluginEventHandler(string, string, Action{ChaskisEventHandlerLineActionArgs})"/>
            /// </summary>
            public ChaskisEventHandler CreatePluginEventHandler(
                string argPattern,
                string expectedSourcePlugin,
                Action<ChaskisEventHandlerLineActionArgs> lineAction
            )
            {
                return new ChaskisEventHandler(
                    argPattern,
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
        /// <param name="args">The args to pass into the plugin.</param>
        /// <returns>A Chaskis event that is ready to be fired.</returns>
        ChaskisEvent CreateBcastEvent( IList<string> args );

        /// <summary>
        /// Generates a ChaskisEvent that is meant to be directed to
        /// a specfic plugin.
        /// </summary>
        /// <param name="targetPluginName">The target plugin name</param>
        /// <param name="args">The args to pass into the plugin.</param>
        /// <returns>A Chaskis event that is ready to be fired.</returns>
        ChaskisEvent CreateTargetedEvent( string targetPluginName, IList<string> args );

        /// <summary>
        /// Creates an event handler that waits for a CORE event
        /// that targets this plugin or is a broadcast.
        /// </summary>
        /// <param name="argPattern">
        /// Chaskis events have arguments after the source and plugin.
        /// You can filter events based on its arguments with this parameter
        /// and only capture events whose arguments match this pattern.
        /// </param>
        /// <param name="expectedProtocol">
        /// The protocol to trigger on.  Null for "Don't Care."
        /// </param>
        /// <param name="lineAction">The action to take when our arg pattern matches.</param>
        /// <returns></returns>
        ChaskisEventHandler CreateCoreEventHandler(
            string argPattern,
            ChaskisEventProtocol? expectedProtocol,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
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
            string argPattern,
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
            string argPattern,
            string expectedSourcePlugin,
            Action<ChaskisEventHandlerLineActionArgs> lineAction
        );
    }
}
