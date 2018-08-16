//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.ServiceProcess;
using SethCS.Basic;
using SethCS.Extensions;

namespace ChaskisService
{
    public partial class ChaskisService : ServiceBase
    {
        // -------- Fields --------

        /// <summary>
        /// The instance of chaskis.
        /// </summary>
        private Chaskis.Chaskis chaskis;

        private FileStream logFile;
        private StreamWriter logWriter;

        private string rootDir;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public ChaskisService()
        {
            InitializeComponent();

            StaticLogger.Log.OnWriteLine -= this.LogInfo;
            StaticLogger.Log.OnWriteLine += this.LogInfo;

            StaticLogger.Log.OnErrorWriteLine -= this.LogError;
            StaticLogger.Log.OnErrorWriteLine += this.LogError;

            this.rootDir = Chaskis.Chaskis.DefaultRootDirectory;

            this.chaskis = new Chaskis.Chaskis( rootDir );
        }

        /// <summary>
        /// Called when the service is started.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart( string[] args )
        {
            try
            {
               string filePath = Path.Combine(
                    this.rootDir,
                    "Chaskis." + DateTime.Now.ToFileNameString() + ".Log"
                );
                this.logFile = new FileStream( filePath, FileMode.Create, FileAccess.Write );
                this.logWriter = new StreamWriter( this.logFile );

                StaticLogger.Log.WriteLine( "Starting." );

                chaskis.InitState1_LoadIrcConfig();

                // Load Plugins.
                bool loaded = chaskis.InitStage2_LoadPlugins();
                if( ( loaded == false ) )
                {
                    this.ExitCode = 1;
                    Stop();
                }

                chaskis.InitStage3_DefaultHandlers();
                chaskis.InitStage4_OpenConnection();
            }
            catch( Exception err )
            {
                StaticLogger.Log.ErrorWriteLine(
                    "FATAL ERROR:" + Environment.NewLine + err.Message + Environment.NewLine + err.StackTrace
                );
                this.Stop();
            }
        }

        /// <summary>
        /// Called when the service is stopped.
        /// </summary>
        protected override void OnStop()
        {
            StaticLogger.Log.WriteLine( "Stopping." );
            Teardown();
        }

        /// <summary>
        /// Called when the system is shutting down.
        /// </summary>
        protected override void OnShutdown()
        {
            StaticLogger.Log.WriteLine( "Shutting down." );
            Teardown();
        }

        /// <summary>
        /// Tears down this service.
        /// </summary>
        private void Teardown()
        {
            this.chaskis?.Dispose();
            this.logWriter?.Dispose(); // Also disposes the file stream.

            StaticLogger.Log.OnWriteLine -= this.LogInfo;
            StaticLogger.Log.OnErrorWriteLine -= this.LogError;
        }

        /// <summary>
        /// How to log info on Linux.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        private void LogInfo( string msg )
        {
            DateTime timeStamp = DateTime.UtcNow;
            this.logWriter.WriteLine( timeStamp.ToTimeStampString() + "  MSG>    " + msg.TrimEnd() );
            this.logWriter.Flush();
        }

        /// <summary>
        /// How to log error on linux.
        /// </summary>
        /// <param name="msg">The error message to log.</param>
        private void LogError( string msg )
        {
            DateTime timeStamp = DateTime.UtcNow;
            this.logWriter.WriteLine( timeStamp.ToTimeStampString() + "  ERROR>    " + msg.TrimEnd() );
            this.logWriter.Flush();
        }
    }
}