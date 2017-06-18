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
        /// <param name="ircConfig">The irc config we are using.</param>
        public bool LoadPlugins(
            IList<AssemblyConfig> assemblyList,
            IIrcConfig ircConfig,
            IChaskisEventScheduler scheduler
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
                        MethodInfo initFunction = type.GetMethod( "Init" );

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

                        PluginInitor initor = new PluginInitor();
                        initor.PluginPath = assemblyConfig.AssemblyPath;
                        initor.IrcConfig = ircConfig;
                        initor.EventScheduler = scheduler;

                        initFunction.Invoke( instance, new object[] { initor } );
                        ChaskisPlugin chaskisPlugin = type.GetCustomAttribute<ChaskisPlugin>();

                        this.plugins.Add(
                            chaskisPlugin.PluginName,
                            new PluginConfig(
                                assemblyConfig.AssemblyPath,
                                chaskisPlugin.PluginName,
                                new List<string>(), // <- TODO.
                                plugin
                            )
                        );

                        StaticLogger.WriteLine( "Successfully loaded plugin: " + chaskisPlugin.PluginName );
                    }
                }
                catch( Exception e )
                {
                    StaticLogger.ErrorWriteLine( "*************" );
                    StaticLogger.ErrorWriteLine( "Warning! Error when loading assembly " + assemblyConfig.AssemblyPath + ":" );
                    StaticLogger.ErrorWriteLine( e.Message );
                    StaticLogger.ErrorWriteLine();
                    StaticLogger.ErrorWriteLine( e.StackTrace );
                    StaticLogger.ErrorWriteLine();
                    if( e.InnerException != null )
                    {
                        StaticLogger.ErrorWriteLine( "Inner Exception:" );
                        StaticLogger.ErrorWriteLine( e.InnerException.Message );
                        StaticLogger.ErrorWriteLine( e.InnerException.StackTrace );
                    }
                    StaticLogger.ErrorWriteLine( "*************" );

                    success = false;
                }
            }

            return success;
        }
    }
}