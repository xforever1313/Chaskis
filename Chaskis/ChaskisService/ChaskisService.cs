using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using Chaskis;
using GenericIrcBot;

namespace ChaskisService
{
    public partial class ChaskisService : ServiceBase
    {
        // -------- Fields --------

        private IrcBot bot = null;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public ChaskisService()
        {
            InitializeComponent();
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

                IIrcConfig ircConfig = XmlLoader.ParseIrcConfig( Path.Combine( rootDir, "IrcConfig.xml" ) );

                List<IIrcHandler> configs = new List<IIrcHandler>();

                // Load Plugins.
                {
                    IList<AssemblyConfig> pluginList = XmlLoader.ParsePluginConfig( Path.Combine( rootDir, "PluginConfig.xml" ) );
                    using ( StringWriter strWriter = new StringWriter() )
                    {
                        PluginManager manager = new PluginManager( strWriter );

                        foreach ( AssemblyConfig pluginInfo in pluginList )
                        {
                            bool success = manager.LoadAssembly( Path.GetFullPath( pluginInfo.AssemblyPath ), pluginInfo.ClassName );
                            if ( ( success == false ) )
                            {
                                this.ExitCode = 1;
                                this.ChaskisEventLog.WriteEntry(
                                    "Error loading assembly " + pluginInfo.AssemblyPath + Environment.NewLine + strWriter.ToString(),
                                    EventLogEntryType.Error
                                );
                                Stop();
                            }
                        }
                        configs.AddRange( manager.Handlers );
                    }
                }

                // Must always check for pings.
                configs.Add( new PingHandler() );

                bot = new IrcBot( ircConfig, configs );
                bot.Start();
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
            this.bot?.Dispose();
        }

        /// <summary>
        /// Called when the system is shutting down.
        /// </summary>
        protected override void OnShutdown()
        {
            this.bot?.Dispose();
        }
    }
}
