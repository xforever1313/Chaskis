//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChaskisCore;
using SethCS.Exceptions;

namespace Chaskis.Plugins.XmlBot
{
    [ChaskisPlugin( "xmlbot" )]
    public class XmlBot : IPlugin
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "0.1.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private IIrcConfig ircConfig;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public XmlBot()
        {
            this.handlers = new List<IIrcHandler>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The location of the source code.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/XmlBot";
            }
        }

        /// <summary>
        /// This plugin's version.
        /// </summary>
        public string Version
        {
            get
            {
                return VersionStr;
            }
        }

        /// <summary>
        /// About this plugin.
        /// </summary>
        public string About
        {
            get
            {
                return "I am an XML engine that allows people to program my responses based on what is said in the channel.";
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginPath">The absolute path to the plugin dll.</param>
        /// <param name="ircConfig">The IRC config we are using.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig )
        {
            string configPath = Path.Combine(
                Path.GetDirectoryName( pluginPath ),
                "XmlBotConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.ircConfig = ircConfig;

            this.handlers.AddRange( XmlLoader.LoadXmlBotConfig( configPath ) );
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            writer.SendMessageToUser(
                "I just respond to whatever my admin told me to respond to.  They configured me with an XML file.  Blame my admin for anything I say!",
                response.Channel
            );
        }

        /// <summary>
        /// Gets the handlers that should be added to the main bot.
        /// </summary>
        /// <returns>The list of handlers to awtch.</returns>
        public IList<IIrcHandler> GetHandlers()
        {
            return this.handlers.AsReadOnly();
        }

        /// <summary>
        /// Tears down the plugin.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Generates the handler for the MessageHandler based on the user's command
        /// and our expected response.
        /// </summary>
        /// <param name="command">The command our bot is listening for.</param>
        /// <param name="response">The response our bot will generate.</param>
        internal static Action<IIrcWriter, IrcResponse> GetMessageHandler( string response )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( response, nameof( response ) );

            return delegate ( IIrcWriter writer, IrcResponse ircResponse )
            {
                StringBuilder responseToSend = new StringBuilder( response );

                foreach( string group in ircResponse.Regex.GetGroupNames() )
                {
                    responseToSend.Replace(
                        "{%" + group + "%}",
                        ircResponse.Match.Groups[group].Value
                    );
                }

                writer.SendMessageToUser(
                    responseToSend.ToString(),
                    ircResponse.Channel
                );
            };
        }
    }
}
