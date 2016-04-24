
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
        /// <summary>
        /// Validates that the environment is good for the plugin.
        /// For example, it has all dependencies installed.
        /// </summary>
        /// <param name="error">The errors that occurred if any.  string.Empty if none.</param>
        /// <returns>True if its okay to load this plugin, else false.</returns>
        bool Validate( out string error );

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The list of handlers to awtch.</returns>
        IList<IIrcHandler> GetHandlers();
    }
}

