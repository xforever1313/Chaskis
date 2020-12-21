//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using Chaskis.Core;

namespace Metrics
{
    [ChaskisPlugin( PluginName )]
    public class MetricsBot : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string PluginName = "metrics_bot";

        // ---------------- Constructor ----------------

        // ---------------- Properties ----------------

        public string SourceCodeLocation
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Version
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string About
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        // ---------------- Functions ----------------

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            throw new NotImplementedException();
        }

        public void Init( PluginInitor pluginInit )
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IList<IIrcHandler> GetHandlers()
        {
            throw new NotImplementedException();
        }
    }
}
