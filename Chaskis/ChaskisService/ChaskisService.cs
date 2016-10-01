
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

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

        private Action<string> infoLogAction;
        private Action<string> errorLogAction;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public ChaskisService()
        {
            InitializeComponent();

            if ( Environment.OSVersion.Platform == PlatformID.Unix )
            {
                infoLogAction = this.LinuxLogInfo;
                errorLogAction = this.LinuxLogError;
            }
            else
            {
                infoLogAction = this.WindowsLogInfo;
                errorLogAction = this.WindowsLogError;
            }

            this.chaskis = new Chaskis.Chaskis(
                infoLogAction,
                errorLogAction
            );
        }

        /// <summary>
        /// Called when the service is started.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart( string[] args )
        {
            try
            {
                string rootDir = Path.Combine(
                    Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
                    "Chaskis"
                );

                if ( Environment.OSVersion.Platform == PlatformID.Unix )
                {
                    string filePath = Path.Combine( rootDir, "Chaskis." + DateTime.Now.ToString( "yyyy-MM-dd_HH-mm-ss-ffff" ) + ".Log" );
                    this.logFile = new FileStream( filePath, FileMode.Create, FileAccess.Write );
                    this.logWriter = new StreamWriter( this.logFile );
                }

                chaskis.InitState1_LoadIrcConfig( Path.Combine( rootDir, "IrcConfig.xml" ) );

                // Load Plugins.
                bool loaded = chaskis.InitStage2_LoadPlugins( Path.Combine( rootDir, "PluginConfig.xml" ) );
                if ( ( loaded == false ) )
                {
                    this.ExitCode = 1;
                    Stop();
                }

                chaskis.InitStage3_DefaultHandlers();
                chaskis.InitStage4_OpenConnection();
            }
            catch ( Exception err )
            {
                this.errorLogAction(
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
            this.infoLogAction( "Stopping." );
            Teardown();
        }

        /// <summary>
        /// Called when the system is shutting down.
        /// </summary>
        protected override void OnShutdown()
        {
            this.infoLogAction( "Shutting down." );
            Teardown();
        }

        /// <summary>
        /// Tears down this service.
        /// </summary>
        private void Teardown()
        {
            this.chaskis?.Dispose();
            this.logWriter?.Dispose(); // Also disposes the file stream.
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
