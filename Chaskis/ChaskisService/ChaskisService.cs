//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using SethCS.Basic;

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

            if( Environment.OSVersion.Platform == PlatformID.Unix )
            {
                StaticLogger.OnWriteLine -= this.LinuxLogInfo;
                StaticLogger.OnWriteLine += this.LinuxLogInfo;

                StaticLogger.OnErrorWriteLine -= this.LinuxLogError;
                StaticLogger.OnErrorWriteLine += this.LinuxLogError;
            }
            else
            {
                StaticLogger.OnWriteLine -= this.WindowsLogInfo;
                StaticLogger.OnWriteLine += this.WindowsLogInfo;

                StaticLogger.OnErrorWriteLine -= this.WindowsLogError;
                StaticLogger.OnErrorWriteLine += this.WindowsLogError;
            }

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
                if( Environment.OSVersion.Platform == PlatformID.Unix )
                {
                    string filePath = Path.Combine(
                        this.rootDir,
                        "Chaskis." + DateTime.Now.ToString( "yyyy-MM-dd_HH-mm-ss-ffff" ) + ".Log"
                    );
                    this.logFile = new FileStream( filePath, FileMode.Create, FileAccess.Write );
                    this.logWriter = new StreamWriter( this.logFile );
                }

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
                StaticLogger.ErrorWriteLine(
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
            StaticLogger.WriteLine( "Stopping." );
            Teardown();
        }

        /// <summary>
        /// Called when the system is shutting down.
        /// </summary>
        protected override void OnShutdown()
        {
            StaticLogger.WriteLine( "Shutting down." );
            Teardown();
        }

        /// <summary>
        /// Tears down this service.
        /// </summary>
        private void Teardown()
        {
            this.chaskis?.Dispose();
            this.logWriter?.Dispose(); // Also disposes the file stream.

            if( Environment.OSVersion.Platform == PlatformID.Unix )
            {
                StaticLogger.OnWriteLine -= this.LinuxLogInfo;
                StaticLogger.OnErrorWriteLine -= this.LinuxLogError;
            }
            else
            {
                StaticLogger.OnWriteLine -= this.WindowsLogInfo;
                StaticLogger.OnErrorWriteLine -= this.WindowsLogError;
            }
        }

        /// <summary>
        /// How to log info on windows.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        private void WindowsLogInfo( string msg )
        {
            this.ChaskisEventLog.WriteEntry(
                msg + Environment.NewLine,
                EventLogEntryType.Information
            );
        }

        /// <summary>
        /// How to log info on Linux.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        private void LinuxLogInfo( string msg )
        {
            DateTime timeStamp = DateTime.Now;
            this.logWriter.WriteLine( timeStamp.ToString( "o" ) + "  MSG>    " + msg );
            this.logWriter.Flush();
        }

        /// <summary>
        /// How to log error on windows.
        /// </summary>
        /// <param name="msg">The error message to log.</param>
        private void WindowsLogError( string msg )
        {
            this.ChaskisEventLog.WriteEntry(
                msg + Environment.NewLine,
                EventLogEntryType.Error
            );
        }

        /// <summary>
        /// How to log error on linux.
        /// </summary>
        /// <param name="msg">The error message to log.</param>
        private void LinuxLogError( string msg )
        {
            DateTime timeStamp = DateTime.Now;
            this.logWriter.WriteLine( timeStamp.ToString( "o" ) + "  ERROR>    " + msg );
            this.logWriter.Flush();
        }
    }
}