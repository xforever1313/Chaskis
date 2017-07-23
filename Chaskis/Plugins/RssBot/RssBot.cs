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

        public const string VersionStr = "0.2.0";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

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
                return "I post updates from RSS and ATOM feeds when they get updated.";
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
                "RssBot",
                "RssBotConfig.xml"
            );

            if( File.Exists( configPath ) == false )
            {
                throw new FileNotFoundException(
                    "Can not open " + configPath
                );
            }

            this.scheduler = initor.EventScheduler;

            this.rssConfig = XmlLoader.ParseConfig( configPath );
            foreach( Feed feed in this.rssConfig.Feeds )
            {
                FeedReader reader = new FeedReader( feed );

                reader.Init();

                int eventId = this.scheduler.ScheduleRecurringEvent(
                    feed.RefreshInterval,
                    delegate ( IIrcWriter writer )
                    {
                        try
                        {
                            IList<SyndicationItem> newItems = reader.Update();
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
                                            item.Title.Text,
                                            item.Links[0].Uri.ToString()
                                        );
                                    }
                                    else
                                    {
                                        msg = string.Format(
                                            "{0}: '{1}'",
                                            reader.FeedTitle,
                                            item.Title.Text
                                        );
                                    }

                                    writer.SendBroadcastMessage(
                                        msg
                                    );
                                }
                            }
                        }
                        catch( Exception err )
                        {
                            initor.Log.ErrorWriteLine(
                                "An Exception was caught while updating feed {0}:{1}{2}",
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
            writer.SendMessage(
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
