//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using ChaskisCore;
using SethCS.Basic;

namespace Chaskis
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
        private Dictionary<string, PluginConfig> plugins;

        private ChaskisEventFactory eventFactory;

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
        /// <param name="ircConfig">The irc config we are using.</param>
        /// <param name="chaskisConfigRoot">The root of the chaskis config.</param>
        public bool LoadPlugins(
            IList<AssemblyConfig> assemblyList,
            IIrcConfig ircConfig,
            IChaskisEventScheduler scheduler,
            IChaskisEventSender eventSender,
            string chaskisConfigRoot
        )
        {
            bool success = true;

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
                    errorString.AppendLine( "Warning! Error when loading assembly " + assemblyConfig.AssemblyPath + ":" );
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

                    success = false;

                    StaticLogger.Log.ErrorWriteLine( errorString.ToString() );
                }
            }

            this.eventFactory = ChaskisEventFactory.CreateInstance( this.plugins.Keys.ToList() );
            foreach( KeyValuePair<string, PluginConfig> plugin in this.plugins )
            {
                try
                {
                    PluginInitor initor = new PluginInitor();
                    initor.PluginPath = plugin.Value.AssemblyPath;
                    initor.IrcConfig = ircConfig;
                    initor.EventScheduler = scheduler;
                    initor.ChaskisEventSender = eventSender;
                    initor.ChaskisConfigRoot = chaskisConfigRoot;
                    initor.ChaskisEventCreator = this.eventFactory.EventCreators[plugin.Key];
                    initor.Log = plugin.Value.Log;

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
                    if( e.InnerException != null )
                    {
                        errorString.AppendLine( "\tInner Exception:" );
                        errorString.AppendLine( "\t\t" + e.InnerException.Message );
                        errorString.AppendLine( "\t\t" + e.InnerException.StackTrace );
                    }
                    errorString.AppendLine( "*************" );

                    success = false;

                    StaticLogger.Log.ErrorWriteLine( errorString.ToString() );
                }
            }

            return success;
        }
    }
}