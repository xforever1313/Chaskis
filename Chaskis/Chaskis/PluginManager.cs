//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Chaskis.Core;
using SethCS.Basic;

namespace Chaskis.Cli
{
    /// <summary>
    /// This class loads plugins.
    /// </summary>
    public class PluginManager
    {
        // -------- Fields --------

        /// <summary>
        /// The plugins loaded thus far.
        /// </summary>
        private readonly Dictionary<string, PluginConfig> plugins;

        private InterPluginEventFactory eventFactory;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Where to log to.  Null for Console.out</param>
        public PluginManager()
        {
            this.plugins = new Dictionary<string, PluginConfig>();
            this.Plugins = new ReadOnlyDictionary<string, PluginConfig>( this.plugins );
        }

        // -------- Properties --------

        /// <summary>
        /// Dictionary of all the loaded plugins.
        /// String is the plugin name in all lower case, value is the IPlugin object.
        /// Its recommended that this is not called unless you are done loading plugins.
        /// </summary>
        public IDictionary<string, PluginConfig> Plugins { get; private set; }

        // -------- Functions --------

        /// <summary>
        /// Loads the given list of plugins.
        /// Any errors are logged to the passed in logger.
        /// </summary>
        /// <param name="assemblyList">List of assemblies we are to load.</param>
        /// <param name="existingPlugins">Already created plugins that do not need to be inited via reflection.</param>
        /// <param name="ircConfig">The irc config we are using.</param>
        /// <param name="chaskisConfigRoot">The root of the chaskis config.</param>
        public void LoadPlugins(
            IList<AssemblyConfig> assemblyList,
            IList<PluginConfig> existingPlugins,
            IReadOnlyIrcConfig ircConfig,
            IChaskisEventScheduler scheduler,
            IInterPluginEventSender eventSender,
            HttpClient httpClient,
            string chaskisConfigRoot
        )
        {
            List<Exception> exceptions = new List<Exception>();

            foreach( AssemblyConfig assemblyConfig in assemblyList )
            {
                try
                {
                    Assembly dll = Assembly.LoadFrom( assemblyConfig.AssemblyPath );

                    // Grab all the plugins, which have the ChaskisPlugin Attribute attached to them.
                    var types = from type in dll.GetTypes()
                                where type.IsDefined( typeof( ChaskisPlugin ), false )
                                select type;

                    foreach( Type type in types )
                    {
                        // Make instance
                        object instance = Activator.CreateInstance( type );
                        IPlugin plugin = instance as IPlugin;
                        if( plugin == null )
                        {
                            string errorString = string.Format(
                                "Can not cast {0} to {1}, make sure your {0} class implements {1}",
                                type.Name,
                                nameof( IPlugin )
                            );

                            throw new InvalidCastException( errorString );
                        }

                        ChaskisPlugin chaskisPlugin = type.GetCustomAttribute<ChaskisPlugin>();

                        this.plugins.Add(
                            chaskisPlugin.PluginName,
                            new PluginConfig(
                                assemblyConfig.AssemblyPath,
                                chaskisPlugin.PluginName,
                                assemblyConfig.BlackListedChannels,
                                plugin,
                                new GenericLogger()
                            )
                        );

                        StaticLogger.Log.WriteLine( "Successfully loaded plugin: " + chaskisPlugin.PluginName );
                    }
                }
                catch( Exception e )
                {
                    StringBuilder errorString = new StringBuilder();
                    errorString.AppendLine( "*************" );
                    errorString.AppendLine( "Error when loading assembly " + assemblyConfig.AssemblyPath + ":" );
                    errorString.AppendLine( e.Message );
                    errorString.AppendLine();
                    errorString.AppendLine( e.StackTrace );
                    errorString.AppendLine();
                    if( e.InnerException != null )
                    {
                        errorString.AppendLine( "\tInner Exception:" );
                        errorString.AppendLine( "\t\t" + e.InnerException.Message );
                        errorString.AppendLine( "\t\t" + e.InnerException.StackTrace );
                    }
                    errorString.AppendLine( "*************" );

                    StaticLogger.Log.ErrorWriteLine( errorString.ToString() );

                    exceptions.Add( e );
                }
            }

            if( exceptions.Count != 0 )
            {
                throw new AggregateException(
                    "Exceptions found when loading plugins:",
                    exceptions
                );
            }

            foreach( PluginConfig existingPlugin in existingPlugins )
            {
                this.plugins.Add( existingPlugin.Name, existingPlugin );
            }

            this.eventFactory = InterPluginEventFactory.CreateInstance( this.plugins.Keys.ToList() );
            foreach( KeyValuePair<string, PluginConfig> plugin in this.plugins )
            {
                try
                {
                    PluginInitor initor = new PluginInitor
                    {
                        PluginPath = plugin.Value.AssemblyPath,
                        IrcConfig = ircConfig,
                        EventScheduler = scheduler,
                        ChaskisEventSender = eventSender,
                        ChaskisConfigRoot = chaskisConfigRoot,
                        ChaskisEventCreator = this.eventFactory.EventCreators[plugin.Key],
                        HttpClient = httpClient,
                        Log = plugin.Value.Log
                    };

                    initor.Log.OnWriteLine += delegate ( string msg )
                    {
                        StaticLogger.Log.WriteLine( "{0}> {1}", plugin.Value.Name, msg );
                    };

                    initor.Log.OnErrorWriteLine += delegate ( string msg )
                    {
                        StaticLogger.Log.ErrorWriteLine( "{0}> {1}", plugin.Value.Name, msg );
                    };

                    plugin.Value.Plugin.Init( initor );

                    StaticLogger.Log.WriteLine( "Successfully inited plugin: " + plugin.Value.Name );
                }
                catch( Exception e )
                {
                    StringBuilder errorString = new StringBuilder();
                    errorString.AppendLine( "*************" );
                    errorString.AppendLine( "Warning! Error when initing plugin " + plugin.Key + ":" );
                    errorString.AppendLine( e.Message );
                    errorString.AppendLine();
                    errorString.AppendLine( e.StackTrace );
                    errorString.AppendLine();

                    Exception innerException = e.InnerException;

                    if( innerException != null )
                    {
                        errorString.AppendLine( "\tInner Exception:" );
                        errorString.AppendLine( "\t\t" + e.InnerException.Message );
                        errorString.AppendLine( "\t\t" + e.InnerException.StackTrace );
                        innerException = innerException.InnerException;
                    }
                    errorString.AppendLine( "*************" );

                    StaticLogger.Log.ErrorWriteLine( errorString.ToString() );

                    exceptions.Add( e );
                }
            }

            if( exceptions.Count != 0 )
            {
                throw new AggregateException(
                    "Exceptions found when initializing plugins:",
                    exceptions
                );
            }
        }
    }
}