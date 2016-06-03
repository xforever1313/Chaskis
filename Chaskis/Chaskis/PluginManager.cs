
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Reflection;
using System.IO;
using GenericIrcBot;
using System.Collections.Generic;

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
        private List<IPlugin> plugins;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Where to log to.  Null for Console.out</param>
        public PluginManager()
        {
            this.plugins = new List<IPlugin>();
            this.Plugins = this.plugins.AsReadOnly();
        }

        // -------- Properties --------

        /// <summary>
        /// List of plugins loaded.
        /// Its recommended that this is not called unless you are done loading plugins.
        /// </summary>
        public IList<IPlugin> Plugins { get; private set; }

        // -------- Functions --------

        /// <summary>
        /// Loads the given list of plugins.
        /// Any errors are logged to the passed in logger.
        /// </summary>
        /// <param name="ircConfig">The irc config we are using.</param>
        /// <param name="errorLog">Where to log the errors.  Default to Console.Out.</param>
        public bool LoadPlugins( IList<AssemblyConfig> pluginList, IIrcConfig ircConfig, TextWriter errorLog = null )
        {
            if ( errorLog == null )
            {
                errorLog = Console.Out;
            }

            bool success = true;
            foreach ( AssemblyConfig pluginConfig in pluginList )
            {
                try
                {
                    Assembly dll = Assembly.LoadFile( pluginConfig.AssemblyPath );
                    Type type = dll.GetType( pluginConfig.ClassName );

                    MethodInfo initFunction = type.GetMethod( "Init" );

                    // Make instance
                    object instance = Activator.CreateInstance( type );
                    initFunction.Invoke( instance, new object[] { pluginConfig.AssemblyPath, ircConfig } );

                    IPlugin plugin = ( IPlugin ) instance;
                    this.plugins.Add( plugin );

                    errorLog.WriteLine( "Successfully loaded plugin: " + pluginConfig.ClassName );
                }
                catch ( Exception e )
                {
                    errorLog.WriteLine( "*************" );
                    errorLog.WriteLine( "Warning! Error when loading assembly " + pluginConfig.ClassName + ":" );
                    errorLog.WriteLine( e.Message );
                    errorLog.WriteLine();
                    errorLog.WriteLine( e.StackTrace );
                    errorLog.WriteLine();
                    if ( e.InnerException != null )
                    {
                        errorLog.WriteLine( "Inner Exception:" );
                        errorLog.WriteLine( e.InnerException.Message );
                        errorLog.WriteLine( e.InnerException.StackTrace );
                    }
                    errorLog.WriteLine( "*************" );

                    success = false;
                }
            }

            return success;
        }
    }
}
