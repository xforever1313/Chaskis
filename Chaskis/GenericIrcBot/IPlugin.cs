
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;

namespace GenericIrcBot
{
    /// <summary>
    /// Implement this interface to create plugins for the Generic IRC bot.
    /// </summary>
    public interface IPlugin
    {
        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.  Allowed to throw Exceptions.
        /// 
        /// This function should be used to validates that the environment is good for the plugin.
        /// For example, it has all dependencies installed, config files are in the correct spot, etc.
        /// It should also load GetHandlers() with the handlers.
        /// </summary>
        /// <param name="pluginPath">
        /// The absolute path to the plugin, including the file name.  To just get
        /// the path to the plugin, call Path.GetFullPath on this argument.
        /// </param>
        void Init( string pluginPath );

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The list of handlers to awtch.</returns>
        IList<IIrcHandler> GetHandlers();
    }
}

