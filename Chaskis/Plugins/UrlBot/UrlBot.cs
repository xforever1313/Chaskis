//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using Chaskis.Core;

namespace Chaskis.Plugins.UrlBot
{
    /// <summary>
    /// This class takes a URL from the channel, reads the HTML From it, finds
    /// the description meta tag, and then prints the description to the channel.
    /// </summary>
    [ChaskisPlugin( "urlbot" )]
    public class UrlBot : IPlugin
    {
        // -------- Fields --------

        internal const string VersionStr = "0.4.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private UrlReader urlReader;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public UrlBot()
        {
            this.handlers = new List<IIrcHandler>();
        }

        // -------- Properties --------

        /// <summary>
        /// The location of the source code.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/UrlBot";
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
                return "When someone posts a URL, I will go to the URL and print its title information. I will ignore sites that do not have a title tag or are over 1Mb large.";
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Initializes the plugin.  This includes loading any configuration files,
        /// starting services, etc.  Allowed to throw Exceptions.
        ///
        /// This function should be used to validates that the environment is good for the plugin.
        /// For example, it has all dependencies installed, config files are in the correct spot, etc.
        /// It should also load GetHandlers() with the handlers.
        /// </summary>
        /// <param name="pluginInit">The class that has information required for initing the plugin.</param>
        public void Init( PluginInitor initor )
        {
            this.urlReader = new UrlReader( initor.Log, initor.HttpClient );

            MessageHandlerConfig msgConfig = new MessageHandlerConfig
            {
                LineRegex = ".+",
                LineAction = this.HandleMessage
            };

            MessageHandler handler = new MessageHandler(
                msgConfig
            );

            this.handlers.Add( handler );
        }

        /// <summary>
        /// Handles the help command.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, MessageHandlerArgs response, string[] args )
        {
            writer.SendMessage(
                this.About,
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
            // Nothing to Dispose.
        }

        /// <summary>
        /// Handles a message from the channel.
        /// </summary>
        /// <param name="writer">The IRC Writer to write to.</param>
        /// <param name="response">The response from the channel.</param>
        private async void HandleMessage( IIrcWriter writer, MessageHandlerArgs response )
        {
            string url;
            if( UrlReader.TryParseUrl( response.Message, out url ) )
            {
                UrlResponse urlResponse = await this.urlReader.AsyncGetDescription( url );

                if( urlResponse.IsValid )
                {
                    writer.SendMessage(
                        string.Format( "Title: {0}", urlResponse.TitleShortened ),
                        response.Channel
                    );
                }
            }
        }
    }
}