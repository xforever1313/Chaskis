//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using ChaskisCore;
using SethCS.Basic;

namespace Chaskis
{
    /// <summary>
    /// The main class.
    /// </summary>
    public class Chaskis : IDisposable
    {
        // -------- Fields --------

        /// <summary>
        /// The IRC Bot.
        /// </summary>
        private IrcBot ircBot;

        /// <summary>
        /// The IRC config to use.
        /// </summary>
        private IIrcConfig ircConfig;

        /// <summary>
        /// The plugins we are using.
        /// </summary>
        private IDictionary<string, IPlugin> plugins;

        /// <summary>
        /// IRC handlers we will be using.
        /// </summary>
        private List<IIrcHandler> handlers;

        private DefaultHandlers defaultHandlers;

        /// <summary>
        /// Whether or not this class was fully initialized or not.
        /// </summary>
        private bool fullyLoaded;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public Chaskis()
        {
            this.plugins = null;
            this.defaultHandlers = null;
            this.handlers = new List<IIrcHandler>();
            this.fullyLoaded = false;
        }

        // -------- Functions --------

        /// <summary>
        /// Loads the IRC config from the given XML path.
        /// </summary>
        /// <param name="xmlFilePath">The path to the IRC config XML.</param>
        public void InitState1_LoadIrcConfig( string xmlFilePath )
        {
            this.ircConfig = XmlLoader.ParseIrcConfig( xmlFilePath );
        }

        /// <summary>
        /// Loads the Plugins from the given XML path.
        /// The IRC config MUST be loaded first.
        /// </summary>
        /// <param name="xmlFilePath">The path to the plugin config XML.</param>
        /// <returns>True if load was successful, else false.</returns>
        public bool InitStage2_LoadPlugins( string xmlFilePath )
        {
            IList<AssemblyConfig> pluginList = XmlLoader.ParsePluginConfig( xmlFilePath );
            return InitStage2_LoadPlugins( pluginList );
        }

        /// <summary>
        /// Loads plugins via a plugin list.
        /// </summary>
        /// <param name="pluginList">The list of plugins to load.</param>
        /// <returns>True if load was successful, else false.</returns>
        public bool InitStage2_LoadPlugins( IList<AssemblyConfig> pluginList )
        {
            if( this.ircConfig == null )
            {
                throw new InvalidOperationException(
                    nameof( this.ircConfig ) + " is null.  Ensure " + nameof( this.InitState1_LoadIrcConfig ) + " was call prior to this function."
                );
            }

            PluginManager manager = new PluginManager();

            if( manager.LoadPlugins( pluginList, this.ircConfig ) )
            {
                this.plugins = manager.Plugins;
                foreach( IPlugin plugin in this.plugins.Values )
                {
                    this.handlers.AddRange( plugin.GetHandlers() );
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the default irc handlers into the program.
        /// Plugins must have been loaded first.
        /// </summary>
        public void InitStage3_DefaultHandlers()
        {
            if( this.plugins == null )
            {
                throw new InvalidOperationException(
                    nameof( this.plugins ) + " is null.  Ensure " + nameof( this.InitStage2_LoadPlugins ) + " was call prior to this function."
                );
            }

            this.defaultHandlers = new DefaultHandlers( this.ircConfig, this.plugins );
            this.defaultHandlers.Init();

            this.handlers.AddRange( this.defaultHandlers.Handlers );
        }

        /// <summary>
        /// Starts the IRC connection.
        /// </summary>
        public void InitStage4_OpenConnection()
        {
            if( this.ircConfig == null )
            {
                throw new InvalidOperationException(
                    nameof( this.ircConfig ) + " is null.  Ensure " + nameof( this.InitState1_LoadIrcConfig ) + " was call prior to this function."
                );
            }
            else if( this.handlers.Count == 0 )
            {
                throw new InvalidOperationException(
                    nameof( this.handlers ) + " is empty.  Ensure " +
                    nameof( this.InitStage3_DefaultHandlers ) + " and/or " +
                    nameof( this.InitStage2_LoadPlugins ) + " was call prior to this function."
                );
            }

            this.ircBot = new IrcBot( this.ircConfig, this.handlers );
            this.ircBot.Start();
            this.fullyLoaded = true;
        }

        /// <summary>
        /// Tearsdown this class.
        /// </summary>
        public void Dispose()
        {
            if( this.fullyLoaded )
            {
                foreach( IPlugin plugin in plugins.Values )
                {
                    try
                    {
                        plugin.Teardown();
                    }
                    catch( Exception err )
                    {
                        StaticLogger.ErrorWriteLine( "Error when tearing down plugin:" + Environment.NewLine + err.ToString() );
                    }
                }
                this.ircBot.Dispose();
            }
        }
    }
}