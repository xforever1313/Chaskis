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
using SethCS.Exceptions;

namespace Chaskis.Plugins.XmlBot
{
    [ChaskisPlugin( "xmlbot" )]
    public class XmlBot : IPlugin
    {
        // ---------------- Fields ----------------

        internal const string VersionStr = "0.4.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private IReadOnlyIrcConfig ircConfig;

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
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            string configPath = Path.Combine(
                initor.ChaskisConfigPluginRoot,
                "XmlBot",
                "XmlBotConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.ircConfig = initor.IrcConfig;

            this.handlers.AddRange( XmlLoader.LoadXmlBotConfig( configPath, this.ircConfig ) );
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            msgArgs.Writer.SendMessage(
                "I just respond to whatever my admin told me to respond to.  They configured me with an XML file.  Blame my admin for anything I say!",
                msgArgs.Channel
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
            // Nothing to Dispose.
        }

        /// <summary>
        /// Generates the handler for the MessageHandler based on the user's command
        /// and our expected response.
        /// </summary>
        /// <param name="command">The command our bot is listening for.</param>
        /// <param name="response">The response our bot will generate.</param>
        public static MessageHandlerAction GetMessageHandler( string response, IReadOnlyIrcConfig ircConfig )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( response, nameof( response ) );

            string innerResponse = response;

            return delegate ( MessageHandlerArgs args )
            {
                StringBuilder responseToSend = new StringBuilder(
                    Parsing.LiquefyStringWithIrcConfig( innerResponse, args.User, ircConfig.Nick, args.Channel )
                );

                foreach( string group in args.Regex.GetGroupNames() )
                {
                    responseToSend.Replace(
                        "{%" + group + "%}",
                        args.Match.Groups[group].Value
                    );
                }

                args.Writer.SendMessage(
                    responseToSend.ToString(),
                    args.Channel
                );
            };
        }
    }
}
