//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using SethCS.Basic;

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
            this.Log = new GenericLogger();
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

        /// <summary>
        /// Allows a plugin to send Chaskis Events.
        /// </summary>
        public IChaskisEventSender ChaskisEventSender { get; set; }

        /// <summary>
        /// Allows the plugin to create Chaskis Events.
        /// </summary>
        public IChaskisEventCreator ChaskisEventCreator { get; set; }

        /// <summary>
        /// The root of the directory that contains the configuration.
        /// </summary>
        public string ChaskisConfigRoot { get; set; }

        /// <summary>
        /// Directory that contains the plugin config folder.
        /// Your plugin config will live in <see cref="ChaskisConfigPluginRoot"/>/PluginName/config.xml
        /// </summary>
        public string ChaskisConfigPluginRoot => Path.Combine( this.ChaskisConfigRoot, "Plugins" );

        /// <summary>
        /// Reference to a Log instance for the specific plugin.
        /// When your plugin needs to log something, it should call this log instance.
        /// </summary>
        public GenericLogger Log { get; private set; } 
    }
}
