//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SethCS.Basic;
using SethCS.Extensions;

namespace Chaskis.Service
{
    public class Worker : BackgroundService
    {
        // ---------------- Fields ----------------

        private readonly ILogger<Worker> logger;

        private readonly string rootDir;

        // ---------------- Constructor ----------------

        public Worker( ILogger<Worker> logger )
        {
            this.logger = logger;
            this.rootDir = Cli.Chaskis.DefaultRootDirectory;
        }

        // ---------------- Functions ----------------

        protected override async Task ExecuteAsync( CancellationToken stoppingToken )
        {
            try
            {
                StaticLogger.Log.OnWriteLine += this.LogInfo;
                StaticLogger.Log.OnErrorWriteLine += this.LogError;

                using( Cli.Chaskis chaskis = new Cli.Chaskis( this.rootDir ) )
                {
                    chaskis.InitState1_LoadIrcConfig();

                    bool loaded = chaskis.InitStage2_LoadPlugins();
                    if( ( loaded == false ) )
                    {
                        throw new ApplicationException( "Could not load plugins" );
                    }

                    chaskis.InitStage3_DefaultHandlers();
                    chaskis.InitStage4_OpenConnection();

                    // Wait until the stopping token comes in.
                    await Task.Delay( -1, stoppingToken );
                }
            }
            catch( TaskCanceledException )
            {
                StaticLogger.Log.WriteLine( "Starting Shutdown Sequence" );
            }
            catch( Exception e )
            {
                StaticLogger.Log.ErrorWriteLine( "FATAL ERROR:" + Environment.NewLine + e.ToString() );
                throw new Exception( "Rethrowing", e );
            }
            finally
            {
                StaticLogger.Log.OnWriteLine -= this.LogInfo;
                StaticLogger.Log.OnErrorWriteLine -= this.LogError;
            }
        }

        /// <summary>
        /// How to log info on Linux.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        private void LogInfo( string msg )
        {
            DateTime timeStamp = DateTime.UtcNow;
            string message = timeStamp.ToTimeStampString() + "  MSG> " + msg.TrimEnd();
            
            this.logger.LogInformation( message );
        }

        /// <summary>
        /// How to log error on linux.
        /// </summary>
        /// <param name="msg">The error message to log.</param>
        private void LogError( string msg )
        {
            DateTime timeStamp = DateTime.UtcNow;
            string message = timeStamp.ToTimeStampString() + "  ERROR>    " + msg.TrimEnd();

            this.logger.LogError( message );
        }
    }
}
