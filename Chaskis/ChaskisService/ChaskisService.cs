
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
        Chaskis.Chaskis chaskis;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public ChaskisService()
        {
            InitializeComponent();
            this.chaskis = new Chaskis.Chaskis(
                delegate( string msg )
                {
                    this.ChaskisEventLog.WriteEntry(
                        msg + Environment.NewLine,
                        EventLogEntryType.Information
                    );
                }
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
                this.ChaskisEventLog.WriteEntry(
                    "FATAL ERROR:" + Environment.NewLine + err.Message + Environment.NewLine + err.StackTrace,
                    EventLogEntryType.Error
                );
                this.Stop();
            }
        }

        /// <summary>
        /// Called when the service is stopped.
        /// </summary>
        protected override void OnStop()
        {
            Teardown();
        }

        /// <summary>
        /// Called when the system is shutting down.
        /// </summary>
        protected override void OnShutdown()
        {
            Teardown();
        }

        /// <summary>
        /// Tears down this service.
        /// </summary>
        private void Teardown()
        {
            this.chaskis?.Dispose();
        }
    }
}
