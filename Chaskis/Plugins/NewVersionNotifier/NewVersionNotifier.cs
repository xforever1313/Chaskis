//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using ChaskisCore;

namespace Chaskis.Plugins.NewVersionNotifier
{
    [ChaskisPlugin( "new_version_notifier" )]
    public class NewVersionNotifier : IPlugin
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "0.1.0";

        private string pluginDir;

        // ---------------- Constructor ----------------

        public NewVersionNotifier()
        {
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
            this.pluginDir = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "NewVersionNotifier"
            );

            string configPath = Path.Combine(
                this.pluginDir,
                "NewVersionNotifierConfig.xml"
            );

            string filePath = Path.Combine(
                this.pluginDir,
                "lastversion.txt"
            );

            if( File.Exists( filePath ) == false )
            {
            }

            initor.EventScheduler.ScheduleEvent(
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
            return new List<IIrcHandler>();
        }

        public void Dispose()
        {
        }

        private void OnTimeExpired( IIrcWriter writer )
        {
        }
    }
}
