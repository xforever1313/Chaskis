//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using ChaskisCore;
using SethCS.Exceptions;

namespace Chaskis
{
    /// <summary>
    /// This class represents information about a plugin.
    /// </summary>
    public class PluginConfig : IHandlerConfig
    {
        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Absolute Path to the assembly</param>
        public PluginConfig(
            string path,
            string name,
            IList<string> blacklistedChannels,
            IPlugin plugin
        )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( path, nameof( path ) );

            this.AssemblyPath = path;
            this.DirectoryPath = Path.GetDirectoryName( this.AssemblyPath );
            this.Name = name;
            this.BlackListedChannels = new List<string>( blacklistedChannels ).AsReadOnly();
            this.Plugin = plugin;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Absolute Path to the assembly.
        /// </summary>
        public string AssemblyPath { get; private set; }

        /// <summary>
        /// Path to the plugin.
        /// </summary>
        public string DirectoryPath { get; private set; }

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Read-only list of channels this plugin is ignored in. 
        /// </summary>
        public IList<string> BlackListedChannels { get; private set; }

        /// <summary>
        /// Associated Plugin
        /// </summary>
        public IPlugin Plugin { get; private set; }

        /// <summary>
        /// Gets the plugin's handlers.
        /// </summary>
        public IList<IIrcHandler> Handlers { get { return this.Plugin.GetHandlers(); } }
    }
}
