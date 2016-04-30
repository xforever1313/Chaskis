
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Reflection;
using System.IO;
using GenericIrcBot;
using System.Collections.Generic;

namespace TestBot
{
    /// <summary>
    /// This class loads plugins.
    /// </summary>
    public class PluginManager
    {
        // -------- Fields --------

        /// <summary>
        /// Where to log errors to.
        /// </summary>
        private TextWriter logger;

        /// <summary>
        /// The handlers from the assembly.
        /// </summary>
        private List<IIrcHandler> handlers;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Where to log to.  Null for Console.out</param>
        public PluginManager( TextWriter logger = null )
        {
            if( logger == null )
            {
                this.logger = Console.Out;
            }
            else
            {
                this.logger = logger;
            }

            this.handlers = new List<IIrcHandler>();
            this.Handlers = this.handlers.AsReadOnly();
        }

        // -------- Properties --------

        /// <summary>
        /// Any handlers found when calling LoadAssembly are appended to this list.
        /// Its recommended that this is not called unless you are done loading plugins.
        /// </summary>
        public IList<IIrcHandler> Handlers{ get; private set; }

        // -------- Functions --------

        /// <summary>
        /// Loads te given assembly.
        /// Any handlers are appended to this.Handlers.
        /// Any errors are logged to the passed in logger.
        /// </summary>
        /// <param name="absPath">Absolute Path to the assembly to load.</param>
        /// <param name="className">Class name to load (including namespaces)</param>
        public bool LoadAssembly( string absPath, string className )
        {
            try
            {
                Assembly dll = Assembly.LoadFile( absPath );
                Type type = dll.GetType( className );

                MethodInfo validateFunction = type.GetMethod( "Validate" );
                MethodInfo initFunction = type.GetMethod( "Init" );
                MethodInfo getHandlerFunction = type.GetMethod( "GetHandlers" );

                // Make instance
                Object instance = Activator.CreateInstance( type );

                string msg = string.Empty;
                if( ( bool )validateFunction.Invoke( instance, new object[] { msg } ) == false )
                {
                    throw new Exception( "Error validating " + className + Environment.NewLine + msg );
                }

                initFunction.Invoke( instance, new Object[]{ } );
 
                IList<IIrcHandler> handlersToAdd = ( IList<IIrcHandler> )getHandlerFunction.Invoke( instance, new Object[]{ } );
                this.handlers.AddRange( handlersToAdd );
            }
            catch( Exception e )
            {
                this.logger.WriteLine( "*************" );
                this.logger.WriteLine( "Warning! Error when loading assembly " + className + ":" );
                this.logger.WriteLine( e.Message );
                this.logger.WriteLine( e.StackTrace );
                this.logger.WriteLine( "*************" );

                return false;
            }

            return true;
        }
    }
}
