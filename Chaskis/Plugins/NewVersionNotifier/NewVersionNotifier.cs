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

namespace Chaskis.Plugins.NewVersionNotifier
{
    [ChaskisPlugin( "new_version_notifier" )]
    public class NewVersionNotifier : IPlugin
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "0.1.0";

        private string pluginDir;

        private IChaskisEventScheduler eventScheduler;
        private IChaskisEventCreator chaskisEventCreator;
        private IChaskisEventSender eventSender;

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
                "lastversion.txt"
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
                new TimeSpan( 0, 0, 30 ),
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
                new List<string>() {
                    "QUERY=VERSION",
                    "PLUGIN=chaskis"
                }
            );

            this.eventSender.SendChaskisEvent( e );
        }

        private async void HandleChaskisEvent( ChaskisEventHandlerLineActionArgs args )
        {
            try
            {
                string versString = args.Match.Groups["version"].Value;
                if( versString.Equals( this.cachedVersion ) == false )
                {
                    string msg = this.config.Message.Replace( "{%version%}", versString );
                    args.IrcWriter.SendBroadcastMessage( msg );
                }
            }
            finally
            {
                await Task.Run( () => File.WriteAllText( this.cachedFilePath, this.cachedVersion ) );
            }
        }
    }
}
