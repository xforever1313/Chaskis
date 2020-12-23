//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using Chaskis.Core;
using SethCS.Extensions;

namespace Metrics
{
    [ChaskisPlugin( PluginName )]
    public class MetricsBot : IPlugin
    {
        // ---------------- Fields ----------------

        private static readonly string versionStr;

        private readonly List<IIrcHandler> handlers;

        internal const string PluginName = "metrics_bot";

        private const string userStatsCmd = "stats";

        private const string allUserStatsCmd = "allstats";

        private IIrcConfig ircConfig;

        // ---------------- Constructor ----------------

        public MetricsBot()
        {
            this.handlers = new List<IIrcHandler>();
        }

        static MetricsBot()
        {
            versionStr = typeof( MetricsBot ).Assembly.GetName().Version.ToString( 3 );
        }

        // ---------------- Properties ----------------

        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/MetricsBot";
            }
        }

        public string Version
        {
            get
            {
                return versionStr;
            }
        }

        public string About
        {
            get
            {
                return "I keep track of how many messages were sent in the channels I am in.";
            }
        }

        // ---------------- Functions ----------------

        public void Init( PluginInitor pluginInit )
        {
            this.ircConfig = pluginInit.IrcConfig;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            // Possible Commands:
            // !stats <user>
            // !botname stats
            // !allstats <user>
            // !botname allstats

            string response;
            if ( ( helpArgs.Length == 1 ) && helpArgs[0].EqualsIgnoreCase( userStatsCmd ) )
            {
                response = $"Usage: !{userStatsCmd} [user].  Gets status for the specified user.  If user is not specified, the stats of the user who sent the command is returned.";
            }
            else if ( ( helpArgs.Length == 1 ) && helpArgs[0].EqualsIgnoreCase( allUserStatsCmd ) )
            {
                response = $"Usage: !{allUserStatsCmd} [user].  Gets status for the specified user across all channels I am in.  If user is not specified, the stats of the user who sent the command is returned.";
            }
            else 
            {
                response = $"Possible commands:  '{userStatsCmd}', '{allUserStatsCmd}'";
            }

            msgArgs.Writer.SendMessage(
                response,
                msgArgs.Channel
            );
        }

        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }
    }
}
