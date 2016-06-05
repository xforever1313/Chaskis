
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Reflection;
using System.IO;
using GenericIrcBot;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        private Dictionary<string, IPlugin> plugins;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Where to log to.  Null for Console.out</param>
        public PluginManager()
        {
            this.plugins = new Dictionary<string, IPlugin>();
            this.Plugins = new ReadOnlyDictionary<string, IPlugin>( this.plugins );
        }

        // -------- Properties --------

        /// <summary>
        /// Dictionary of all the loaded plugins.
        /// String is the plugin name in all lower case, value is the IPlugin object.
        /// Its recommended that this is not called unless you are done loading plugins.
        /// </summary>
        public IDictionary<string, IPlugin> Plugins { get; private set; }

        // -------- Functions --------

        /// <summary>
        /// Loads the given list of plugins.
        /// Any errors are logged to the passed in logger.
        /// </summary>
        /// <param name="ircConfig">The irc config we are using.</param>
        /// <param name="errorLog">Where to log the errors.  Default to Console.Out.</param>
        /// <param name="logFunction">
        /// Action to take when we want to log something.
        /// Argument to action is the string to log.
        /// </param>
        public bool LoadPlugins( IList<AssemblyConfig> pluginList, IIrcConfig ircConfig, Action<string> logFunction = null )
        {
            bool success = true;

            using ( StringWriter errorLog = new StringWriter() )
            {
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

                        // Use string.split, there's less bugs and edge-cases when trying to parse it out.
                        string[] splitString = pluginConfig.ClassName.Split( '.' );

                        string name = splitString[splitString.Length - 1].ToLower();
                        this.plugins.Add( name, plugin );

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

                if ( logFunction != null )
                {
                    logFunction( errorLog.ToString() );
                }
            }

            return success;
        }
    }
}
