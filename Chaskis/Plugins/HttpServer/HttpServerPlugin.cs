//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using Chaskis.Core;

namespace Chaskis.Plugins.HttpServer
{
    [ChaskisPlugin( PluginName )]
    public class HttpServerPlugin : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string VersionStr = "0.1.0";

        internal const string PluginName = "httpserver";

        private HttpServer server;

        // ---------------- Constructor ----------------

        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/HttpServer";
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
                return "I allow admins to control me remotely via HTTP.";
            }
        }

        // ---------------- Functions ----------------

        public void Init( PluginInitor pluginInit )
        {
            HttpServerConfig config = new HttpServerConfig
            {
                Port = 10013
            };

            this.server = new HttpServer( config );
            this.server.Start();
        }

        public void Dispose()
        {
            this.server?.Dispose();
        }

        public IList<IIrcHandler> GetHandlers()
        {
            // No handlers that need to happen, everything is driven
            // through the HTTP interface.
            return new List<IIrcHandler>();
        }

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            msgArgs.Writer.SendMessage(
                "I have no commands.",
                msgArgs.Channel
            );
        }
    }
}
