//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Plugins.ServerDiagnostics
{
    /// <summary>
    /// Class that configures the ServerDiagnosticsConfig.
    /// </summary>
    public class ServerDiagnosticsConfig
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.  Sets all properties to
        /// string.Empty by default.
        /// </summary>
        public ServerDiagnosticsConfig()
        {
            this.UpTimeCmd = string.Empty;
            this.OsVersionCmd = string.Empty;
            this.ProcessorCountCmd = string.Empty;
            this.TimeCmd = string.Empty;
        }

        // -------- Properties --------

        /// <summary>
        /// The command from the channel that causes the bot
        /// to respond with the time it has been running.
        /// </summary>
        public string UpTimeCmd { get; set; }

        /// <summary>
        /// The command from the channel that causes the bot
        /// to respond with the operating system the bot is running on.
        /// </summary>
        public string OsVersionCmd { get; set; }

        /// <summary>
        /// The command from the channel that causes the bot
        /// to repond with the number of processors on the system the bot is running on.
        /// </summary>
        public string ProcessorCountCmd { get; set; }

        /// <summary>
        /// The command from the channel that causes the bot
        /// to repond with the local time oon the system the bot is running on.
        /// </summary>
        public string TimeCmd { get; set; }
    }
}