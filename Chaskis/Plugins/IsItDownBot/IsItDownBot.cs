//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using Chaskis.Core;
using SethCS.Basic;

namespace Chaskis.Plugins.IsItDownBot
{
    [ChaskisPlugin( PluginName )]
    public class IsItDownBot : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string VersionStr = "0.1.2";

        internal const string PluginName = "isitdownbot";

        private IsItDownBotConfig config;

        private GenericLogger logger;

        private readonly List<IIrcHandler> ircHandlers;

        // ---------------- Constructor ----------------

        public IsItDownBot()
        {
            this.ircHandlers = new List<IIrcHandler>();
        }

        // ---------------- Properties ----------------

        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/IsItDownBot";
            }
        }

        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        public string About
        {
            get
            {
                return "I check to see if a website is down for you.";
            }
        }

        // ---------------- Functions ----------------

        public void Init( PluginInitor initor )
        {
            this.logger = initor.Log;

            string pluginDir = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "IsItDownBot"
            );

            string configPath = Path.Combine(
                pluginDir,
                "IsItDownBotConfig.xml"
            );

            this.config = XmlLoader.LoadConfig( configPath );
        }

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            msgArgs.Writer.SendMessage(
                "Usage: " + this.config.CommandPrefix + " https://url",
                msgArgs.Channel
            );
        }

        public IList<IIrcHandler> GetHandlers()
        {
            return this.ircHandlers.AsReadOnly();
        }

        public void Dispose()
        {
            // Nothing to Dispose.
        }
    }
}
