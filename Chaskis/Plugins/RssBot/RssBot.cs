//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using ChaskisCore;
using SethCS.Basic;

namespace Chaskis.Plugins.RssBot
{
    /// <summary>
    /// RSS Bot pulls from RSS feeds and posts any updates
    /// to the IRC channel.
    /// </summary>
    [ChaskisPlugin( "rssbot" )]
    public class RssBot : IPlugin
    {
        // ---------------- Fields ----------------

        public const string VersionStr = "0.1.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private IIrcConfig ircConfig;

        private RssBotConfig rssConfig;

        private Dictionary<int, FeedReader> feedReaders;

        private IChaskisEventScheduler scheduler;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor
        /// </summary>
        public RssBot()
        {
            this.handlers = new List<IIrcHandler>();
            this.feedReaders = new Dictionary<int, FeedReader>();
            this.scheduler = null;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The location of the source code.
        /// </summary>
        public string SourceCodeLocation
        {
            get
            {
                return "https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/RssBot";
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
                return "I post updates to RSS and ATOM feeds when they get updated.";
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        /// <param name="pluginPath">The absolute path to the plugin dll.</param>
        /// <param name="ircConfig">The IRC config we are using.</param>
        /// <param name="eventScheduler">The event scheduler.</param>
        public void Init( string pluginPath, IIrcConfig ircConfig, IChaskisEventScheduler eventScheduler )
        {
            string configPath = Path.Combine(
                Path.GetDirectoryName( pluginPath ),
                "RssBotConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.scheduler = eventScheduler;
            this.ircConfig = ircConfig;

            this.rssConfig = XmlLoader.ParseConfig( configPath );
            foreach( Feed feed in this.rssConfig.Feeds )
            {
                FeedReader reader = new FeedReader( feed );

                reader.Init();

                int eventId = this.scheduler.ScheduleRecurringEvent(
                    feed.RefreshInterval,
                    async delegate ( IIrcWriter writer )
                    {
                        try
                        {
                            IList<SyndicationItem> newItems = await reader.Update();
                            if( newItems.Count > 0 )
                            {
                                foreach( SyndicationItem item in newItems )
                                {
                                    string msg = string.Empty;

                                    if( item.Links.Count > 0 )
                                    {
                                        msg = string.Format(
                                            "{0}: '{1}' {2}",
                                            reader.FeedTitle,
                                            item.Title,
                                            item.Links[0].Uri.ToString()
                                        );
                                    }
                                    else
                                    {
                                        msg = string.Format(
                                            "{0}: '{1}'",
                                            reader.FeedTitle,
                                            item.Title
                                        );
                                    }

                                    writer.SendMessageToUser(
                                        msg,
                                        this.ircConfig.Channel
                                    );
                                }
                            }
                        }
                        catch( Exception err )
                        {
                            StaticLogger.ErrorWriteLine(
                                "RssBot> An Exception was caught while updating feed {0}:{1}{2}",
                                reader.FeedTitle,
                                Environment.NewLine,
                                err.ToString()
                            );
                        }
                    }
                );

                this.feedReaders.Add( eventId, reader );
            }
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( IIrcWriter writer, IrcResponse response, string[] args )
        {
            writer.SendMessageToUser(
                this.About + "  Don't like one of the feeds I post?  Yell at my admin!",
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
            if( this.scheduler != null )
            {
                foreach( int eventId in this.feedReaders.Keys )
                {
                    this.scheduler.StopEvent( eventId );
                }
            }
        }
    }
}
