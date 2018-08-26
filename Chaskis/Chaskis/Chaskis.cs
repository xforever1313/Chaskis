//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reflection;
using ChaskisCore;
using SethCS.Basic;

namespace Chaskis
{
    /// <summary>
    /// The main class.
    /// </summary>
    public class Chaskis : IDisposable
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "0.8.0";

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
        private IDictionary<string, PluginConfig> plugins;

        private DefaultHandlers defaultHandlers;

        /// <summary>
        /// Whether or not this class was fully initialized or not.
        /// </summary>
        private bool fullyLoaded;

        /// <summary>
        /// The chaskis config root directory.
        /// </summary>
        private readonly string chaskisRoot;

        private readonly StringParsingQueue parsingQueue;

        private readonly HttpClient httpClient;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="chaskisRoot">Where to find the configuration files.</param>
        public Chaskis( string chaskisRoot )
        {
            this.plugins = null;
            this.defaultHandlers = null;
            this.plugins = new Dictionary<string, PluginConfig>();
            this.fullyLoaded = false;
            this.chaskisRoot = chaskisRoot;
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add( "User-Agent", "Chaskis IRC Bot" );
            this.httpClient.Timeout = new TimeSpan( 0, 0, 10 );

            this.parsingQueue = new StringParsingQueue();
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Chaskis()
        {
            DefaultRootDirectory = Path.Combine(
                Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
                "Chaskis"
            );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The default chaskis root directory.
        /// </summary>
        public static string DefaultRootDirectory { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Loads the IRC config from the chaskis root.
        /// </summary>
        /// <param name="xmlFilePath">The path to the IRC config XML.</param>
        public void InitState1_LoadIrcConfig()
        {
            string ircConfigFile = Path.Combine( this.chaskisRoot, "IrcConfig.xml" );

            StaticLogger.Log.WriteLine( "Using IRC config file '{0}'", ircConfigFile );

            this.ircConfig = XmlLoader.ParseIrcConfig( ircConfigFile );
            this.ircBot = new IrcBot( this.ircConfig, this.parsingQueue );
        }

        /// <summary>
        /// Loads the Plugins from the chaskis root.
        /// The IRC config MUST be loaded first.
        /// </summary>
        /// <returns>True if load was successful, else false.</returns>
        public bool InitStage2_LoadPlugins()
        {
            string pluginConfigFile = Path.Combine( this.chaskisRoot, "PluginConfig.xml" );

            StaticLogger.Log.WriteLine( "Using Plugin config file '{0}'", pluginConfigFile );

            IList<AssemblyConfig> pluginList = XmlLoader.ParsePluginConfig( pluginConfigFile );
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

            this.defaultHandlers = new DefaultHandlers();
            PluginConfig defaultChaskisPlugin = new PluginConfig(
                Assembly.GetExecutingAssembly().Location,
                DefaultHandlers.DefaultPluginName,
                new List<string>(), // No blacklisted channels
                this.defaultHandlers,
                new GenericLogger()
            );
               

            if( manager.LoadPlugins(
                    pluginList,
                    new List<PluginConfig>() { defaultChaskisPlugin },
                    this.ircConfig,
                    this.ircBot.Scheduler,
                    this.ircBot.ChaskisEventSender,
                    this.httpClient,
                    this.chaskisRoot
                )
            )
            {
                this.plugins = manager.Plugins;
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

            this.defaultHandlers.Init_Stage2( this.plugins );
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
            else if( this.defaultHandlers == null )
            {
                throw new InvalidOperationException(
                    nameof( this.defaultHandlers ) + " is null.  Ensure " +
                    nameof( this.InitStage3_DefaultHandlers ) + " and/or " +
                    nameof( this.InitStage2_LoadPlugins ) + " was call prior to this function."
                );
            }

            Dictionary<string, IHandlerConfig> handlers = new Dictionary<string, IHandlerConfig>();
            foreach( KeyValuePair<string, PluginConfig> plugin in this.plugins )
            {
                handlers.Add( plugin.Key, plugin.Value );
            }

            // Start the parsing queue before we open any connections.
            // Don't want to miss anything!
            this.parsingQueue.Start( new ReadOnlyDictionary<string, IHandlerConfig>( handlers ) );

            this.ircBot.Init();
            this.ircBot.Start();
            this.fullyLoaded = true;
        }

        /// <summary>
        /// Tearsdown this class.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if( this.fullyLoaded )
                {
                    foreach( PluginConfig plugin in plugins.Values )
                    {
                        try
                        {
                            plugin.Plugin.Dispose();
                        }
                        catch( Exception err )
                        {
                            StaticLogger.Log.ErrorWriteLine( "Error when tearing down plugin:" + Environment.NewLine + err );
                        }
                    }
                    this.ircBot.Dispose();
                    this.parsingQueue.WaitForAllEventsToExecute();
                }
            }
            finally
            {
                this.parsingQueue.Dispose();
                this.httpClient.Dispose();
            }
        }
    }
}