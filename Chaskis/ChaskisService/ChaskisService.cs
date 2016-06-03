
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

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

        private List<IPlugin> plugins;

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        public ChaskisService()
        {
            InitializeComponent();
            this.plugins = new List<IPlugin>();
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
                    PluginManager manager = new PluginManager();

                    using ( StringWriter strWriter = new StringWriter() )
                    {
                        bool loaded = manager.LoadPlugins( pluginList, ircConfig, strWriter );

                        if ( ( loaded == false ) )
                        {
                            this.ExitCode = 1;
                            this.ChaskisEventLog.WriteEntry(
                                "Error loading plugins: " + Environment.NewLine + strWriter.ToString(),
                                EventLogEntryType.Error
                            );
                            Stop();
                        }
                    }
                    this.plugins.AddRange( manager.Plugins );
                }

                foreach ( IPlugin plugin in this.plugins )
                {
                    configs.AddRange( plugin.GetHandlers() );
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
            if ( bot != null )
            {
                foreach ( IPlugin plugin in plugins )
                {
                    try
                    {
                        plugin.Teardown();
                    }
                    catch ( Exception err )
                    {
                        this.ChaskisEventLog.WriteEntry(
                            "Error when tearing down plugin:" + Environment.NewLine + err.ToString(),
                            EventLogEntryType.Error
                        );
                    }
                }

                bot.Dispose();
            }
        }
    }
}
