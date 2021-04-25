//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chaskis.Service
{
    public class Worker : BackgroundService
    {
        // ---------------- Fields ----------------

        private readonly ILogger<Worker> logger;
        private readonly ServiceMain main;

        // ---------------- Constructor ----------------

        public Worker( ILogger<Worker> logger )
        {
            this.logger = logger;
            this.main = new ServiceMain( this.logger );
        }

        // ---------------- Functions ----------------

        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            try
            {
                await this.main.RunChaskis( Program.Args, stoppingToken );
            }
            catch( Exception e )
            {
                logger.LogError( "FATAL ERROR:" + Environment.NewLine + e );
                throw new Exception( "Rethrowing", e );
            }
        }
    }
}
