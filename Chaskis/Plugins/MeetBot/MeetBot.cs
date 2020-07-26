//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using Chaskis.Core;
using SethCS.Basic;

namespace Chaskis.Plugins.MeetBot
{
    [ChaskisPlugin( PluginName )]
    public class MeetBot : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string PluginName = "MeetBot";

        private static readonly string versionStr;

        private GenericLogger logger;

        private string pluginDir;

        private string defaultNotesDirectory;

        private readonly List<IIrcHandler> ircHandlers;

        // ---------------- Constructor ----------------

        public MeetBot()
        {
            this.ircHandlers = new List<IIrcHandler>();
        }

        static MeetBot()
        {
            versionStr = typeof( MeetBot ).Assembly.GetName().Version.ToString( 3 );
        }

        // ---------------- Properties ----------------

        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/MeetBot";
            }
        }

        public string Version
        {
            get
            {
                return versionStr;
            }
        }

        public string About
        {
            get
            {
                return "I help take meeting over IRC";
            }
        }

        // ---------------- Functions ----------------

        public void Init( PluginInitor initor )
        {
            this.logger = initor.Log;

            this.pluginDir = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                PluginName
            );

            this.defaultNotesDirectory = Path.Combine(
                this.pluginDir,
                "meeting_notes"
             );
        }

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            msgArgs.Writer.SendMessage(
                this.About,
                msgArgs.Channel
            );
        }

        public IList<IIrcHandler> GetHandlers()
        {
            return this.ircHandlers.AsReadOnly();
        }

        public void Dispose()
        {
            // Nothing to Dispose.
        }
    }
}
