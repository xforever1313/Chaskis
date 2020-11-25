//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Cli;
using Microsoft.Extensions.Logging;
using SethCS.Extensions;

namespace Chaskis.Service
{
    public class ServiceMain : ChaskisMain
    {
        // ---------------- Fields ----------------

        private readonly ILogger<Worker> logger;

        // ---------------- Constructor ----------------

        public ServiceMain( ILogger<Worker> logger )
        {
            this.logger = logger;
        }

        // ---------------- Functions ----------------

        public override void LogInfo( string str )
        {
            DateTime timeStamp = DateTime.UtcNow;
            string message = timeStamp.ToTimeStampString() + "  MSG> " + str.TrimEnd();

            this.logger.LogInformation( message );
        }

        public override void LogError( string str )
        {
            DateTime timeStamp = DateTime.UtcNow;
            string message = timeStamp.ToTimeStampString() + "  ERROR>    " + str.TrimEnd();

            this.logger.LogError( message );
        }
    }
}
