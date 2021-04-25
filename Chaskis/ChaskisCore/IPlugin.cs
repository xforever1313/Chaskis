//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;

namespace Chaskis.Core
{
    /// <summary>
    /// Implement this interface to create plugins for the Generic IRC bot.
    /// </summary>
    public interface IPlugin : IDisposable
    {
        // -------- Properties --------

        /// <summary>
        /// The location of the source code.
        /// 
        /// Gets sent to the IRC channel when a user asks the plugin for its about information
        /// 
        /// e.g. !mybot pluginName source
        /// </summary>
        string SourceCodeLocation { get; }

        /// <summary>
        /// A version string of the plugin.
        /// 
        /// Usually something like x.y.z.
        /// 
        /// Gets sent to the IRC channel when a user asks the plugin for its version.
        /// 
        /// e.g. !mybot pluginName version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// A brief description of the plugin.
        /// 
        /// Gets sent to the IRC channel when a user asks the plugin for its about information
        /// 
        /// e.g. !mybot pluginName about
        /// </summary>
        string About { get; }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.  Allowed to throw Exceptions.
        ///
        /// This function should be used to validates that the environment is good for the plugin.
        /// For example, it has all dependencies installed, config files are in the correct spot, etc.
        /// It should also load GetHandlers() with the handlers.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        void Init( PluginInitor pluginInit );

        /// <summary>
        /// When a user queries the bot for help information about this plugin,
        /// this function is called.
        /// 
        /// The value passed into args, in the event of "!mybot pluginName help arg1 arg2" getting sent from a user,
        /// everything after "help" is passed in.  Note that an empty string can be passed in if a user only types
        /// "!mybot pluginName help", in which case they want a generic usage message.
        /// </summary>
        /// <param name="msgArgs">Message from the server.</param>
        /// <param name="helpArgs">Any arguments the user passed in.</param>
        void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs );

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The list of handlers to awtch.</returns>
        IList<IIrcHandler> GetHandlers();
    }
}