//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using System.Text;
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

        private CommandDefinitionCollection cmdDefs;
        private MeetBotConfig meetbotConfig;

        private string rootHelpMessage;

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

            ReadConfigFiles();
            BuildRootHelpMsg();
        }

        private void ReadConfigFiles()
        {
            XmlLoader loader = new XmlLoader( this.logger );

            this.meetbotConfig = loader.ParseDefaultConfigFile( this.pluginDir );
            this.meetbotConfig.Validate();

            IList<CommandDefinition> defs = loader.ParseDefaultFile();

            if( string.IsNullOrEmpty( this.meetbotConfig.CommandConfigPath ) == false )
            {
                using( FileStream reader = new FileStream( this.meetbotConfig.CommandConfigPath, FileMode.Open, FileAccess.Read ) )
                {
                    IList<CommandDefinition> userDefs = loader.ParseCommandFile( reader, false );
                    foreach( CommandDefinition userDef in userDefs )
                    {
                        defs.Add( userDef );
                    }
                }
            }

            this.cmdDefs = new CommandDefinitionCollection( defs );

            this.cmdDefs.InitStage1_ValidateDefinitions();
            this.cmdDefs.InitStage2_FilterOutOverrides();
            this.cmdDefs.InitStage3_BuildDictionaries();
        }

        private void BuildRootHelpMsg()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( "Meeting Commands: " );

            foreach( CommandDefinition def in this.cmdDefs.CommandDefinitions )
            {
                foreach( string prefix in def.Prefixes )
                {
                    builder.Append( prefix + " " );
                }
            }

            this.rootHelpMessage = builder.ToString().TrimEnd();
        }

        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            if( helpArgs.Length == 0 )
            {
                msgArgs.Writer.SendMessage(
                    this.rootHelpMessage,
                    msgArgs.Channel
                );
            }
            else if( helpArgs.Length == 1 )
            {
                CommandDefinitionFindResult result = this.cmdDefs.Find( helpArgs[0] );
                if( result == null )
                {
                    msgArgs.Writer.SendMessage(
                        $"'{helpArgs[0]}' is not a command I know.  Try the help command with no arguments to see what commands I know.",
                        msgArgs.Channel
                    );
                }
                else
                {
                    msgArgs.Writer.SendMessage(
                        result.FoundDefinition.GetFullHelpText( result.CommandPrefix ),
                        msgArgs.Channel
                    );
                }
            }
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
