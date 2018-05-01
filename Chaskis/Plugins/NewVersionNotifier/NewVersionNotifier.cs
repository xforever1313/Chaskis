//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ChaskisCore;
using SethCS.Basic;

namespace Chaskis.Plugins.NewVersionNotifier
{
    [ChaskisPlugin( PluginName )]
    public class NewVersionNotifier : IPlugin
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "0.1.0";

        private const string cacheFileName = "lastversion.txt";

        public const string PluginName = "new_version_notifier";

        private string pluginDir;

        private IChaskisEventScheduler eventScheduler;
        private IChaskisEventCreator chaskisEventCreator;
        private IChaskisEventSender eventSender;
        private GenericLogger logger;

        private string cachedFilePath;
        private string cachedVersion;

        private NewVersionNotifierConfig config;

        private List<IIrcHandler> ircHandlers;

        private int eventId;

        // ---------------- Constructor ----------------

        public NewVersionNotifier()
        {
            this.ircHandlers = new List<IIrcHandler>();
        }

        // ---------------- Properties ----------------

        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/NewVersionNotifier";
            }
        }

        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        public string About
        {
            get
            {
                return "I send a message when I first connect if my version has changed.";
            }
        }

        // ---------------- Functions ----------------

        public void Init( PluginInitor initor )
        {
            this.eventScheduler = initor.EventScheduler;
            this.chaskisEventCreator = initor.ChaskisEventCreator;
            this.eventSender = initor.ChaskisEventSender;
            this.logger = initor.Log;

            this.pluginDir = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "NewVersionNotifier"
            );

            string configPath = Path.Combine(
                this.pluginDir,
                "NewVersionNotifierConfig.xml"
            );

            this.config = XmlLoader.LoadConfig( configPath );

            this.cachedFilePath = Path.Combine(
                this.pluginDir,
                cacheFileName
            );

            if( File.Exists( this.cachedFilePath ) == false )
            {
                this.cachedVersion = string.Empty;
            }
            else
            {
                string[] lines = File.ReadAllLines( this.cachedFilePath );
                if( lines.Length == 0 )
                {
                    this.cachedVersion = string.Empty;
                }
                else
                {
                    this.cachedVersion = lines[0].Trim();
                }
            }

            ChaskisEventHandler eventHandler = this.chaskisEventCreator.CreatePluginEventHandler(
                @"QUERY=VERSION\s+PLUGIN=chaskis\s+VERSION=(?<version>.+)",
                "chaskis",
                this.HandleChaskisEvent
            );

            this.ircHandlers.Add( eventHandler );

            this.eventId = this.eventScheduler.ScheduleEvent(
                this.config.Delay,
                this.OnTimeExpired
            );
        }

        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            writer.SendMessage(
                this.About,
                response.Channel
            );
        }

        public IList<IIrcHandler> GetHandlers()
        {
            return this.ircHandlers.AsReadOnly();
        }

        public void Dispose()
        {
            this.eventScheduler.StopEvent( this.eventId );
        }

        private void OnTimeExpired( IIrcWriter writer )
        {
            ChaskisEvent e = this.chaskisEventCreator.CreateTargetedEvent(
                "chaskis",
                new Dictionary<string, string>()
                {
                    ["QUERY"] = "VERSION",
                    ["PLUGIN"] = "chaskis"
                },
                new Dictionary<string, string>()
                {
                    ["CHANNEL"] = string.Empty // TODO.
                }
            );

            this.eventSender.SendChaskisEvent( e );
        }

        private async void HandleChaskisEvent( ChaskisEventHandlerLineActionArgs args )
        {
            if( args.EventArgs.ContainsKey( "version" ) )
            {
                string versString = args.EventArgs["VERSION"];
                if( versString.Equals( this.cachedVersion ) == false )
                {
                    string msg = this.config.Message.Replace( "{%version%}", versString );
                    args.IrcWriter.SendBroadcastMessage( msg );

                    await Task.Run(
                        () =>
                        {
                            File.WriteAllText( this.cachedFilePath, versString );
                            this.logger.WriteLine( "{0}'s {1} file has been updated", PluginName, cacheFileName );
                        }
                    );
                }
                else
                {
                    this.logger.WriteLine( "Bot not updated, skipping message" );
                }
            }
        }
    }
}
