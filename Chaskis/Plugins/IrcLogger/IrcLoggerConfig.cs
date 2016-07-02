
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

namespace Chaskis.Plugins.IrcLogger
{
    /// <summary>
    /// Contains the configuration for the Irc Logger Plugin.
    /// </summary>
    public class IrcLoggerConfig
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// Sets all values to the defaults.
        /// </summary>
        public IrcLoggerConfig()
        {
            this.LogFileLocation = null;
            this.MaxNumberMessagesPerLog = 1000;
            this.LogName = null;
        }

        // -------- Properties --------

        /// <summary>
        /// Where to put the log files.  Defaulted to
        /// null, which tells the plugin to use the default file location.
        /// string.Empty also results in a default value.
        /// This must be a directory. If it does not exist, the plugin will
        /// try to create it.
        /// </summary>
        public string LogFileLocation { get; set; }

        /// <summary>
        /// The number of messages received from the channel that
        /// go into a log before creating a new log.  Set to 0 for 
        /// no limit.  Defaulted to 1000
        /// </summary>
        public uint MaxNumberMessagesPerLog { get; set; }

        /// <summary>
        /// What to name the logs.  Defaulted to null,
        /// which tells the plugin to use "irclog".
        /// string.empty also results in the default value.
        /// </summary>
        public string LogName { get; set; }
    }
}
