//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;

namespace ChaskisCore
{
    /// <summary>
    /// Class that is used to initialize the plugin.
    /// This is passed into <see cref="IPlugin"/>'s Init function.
    /// This way, if we need to add or remove something, we don't
    /// need to change ALL of the plugin's Init function signature.
    /// </summary>
    public class PluginInitor
    {
        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public PluginInitor()
        {
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// The absolute path to the plugin, including the file name. 
        /// </summary>
        public string PluginPath { get; set; }

        /// <summary>
        /// The directory to the plugin DLL.
        /// </summary>
        public string PluginDirectory => Path.GetDirectoryName( this.PluginPath );

        /// <summary>
        /// The IRC config we are using.
        /// </summary>
        public IIrcConfig IrcConfig { get; set; }

        /// <summary>
        /// The event scheduler that can be used to schedule events to run on the event queue.
        /// </summary>
        public IChaskisEventScheduler EventScheduler { get; set; }
    }
}
