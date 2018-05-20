//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using ChaskisCore;

namespace IsItDownBot
{
    [ChaskisPlugin( PluginName )]
    public class IsItDownBot : IPlugin
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "0.1.0";

        public const string PluginName = "isitdownbot";

        // ---------------- Constructor ----------------

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

        public void Init( PluginInitor pluginInit )
        {
            throw new NotImplementedException();
        }

        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            throw new NotImplementedException();
        }

        public IList<IIrcHandler> GetHandlers()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
