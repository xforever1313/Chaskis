//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Chaskis.Core;
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

        internal const string VersionStr = "0.5.2";

        /// <summary>
        /// The handlers for this plugin.
        /// </summary>
        private readonly List<IIrcHandler> handlers;

        private RssBotConfig rssConfig;

        private readonly Dictionary<int, FeedReader> feedReaders;

        private IChaskisEventScheduler scheduler;

        private GenericLogger pluginLogger;

        IList<string> admins;

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

            this.admins = initor.IrcConfig.Admins;

            this.pluginLogger = initor.Log;

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
                FeedReader reader = new FeedReader( feed, initor.HttpClient );

                try
                {
                    reader.Init();
                }
                catch( Exception e )
                {
                    this.pluginLogger.WriteLine(
                        "Error when initing feed.  Will try again during the next update." + Environment.NewLine + e
                    );
                }

                int eventId = this.scheduler.ScheduleRecurringEvent(
                    feed.RefreshInterval,
                    delegate ( IIrcWriter writer )
                    {
                        CheckForUpdates( reader, writer, feed.Channels );
                    }
                );

                this.feedReaders.Add( eventId, reader );
            }

            MessageHandlerConfig msgConfig = new MessageHandlerConfig
            {
                LineRegex = @"!debug\s+rssbot\s+updatefeed\s+(?<url>\S+)",
                LineAction = this.HandleDebug
            };

            MessageHandler debugHandler = new MessageHandler(
                msgConfig
            );

            this.handlers.Add( debugHandler );
        }

        private async void CheckForUpdates( FeedReader reader, IIrcWriter writer, IList<string> channels )
        {
            try
            {
                this.pluginLogger.WriteLine(
                    Convert.ToInt32( LogVerbosityLevel.LowVerbosity ),
                    "Fetching RSS feed for '" + reader.Url + "'"
                );

                IList<SyndicationItem> newItems = await reader.UpdateAsync();
                if( newItems.Count > 0 )
                {
                    this.pluginLogger.WriteLine(
                        Convert.ToInt32( LogVerbosityLevel.LowVerbosity ),
                        "Found updates on RSS feed '" + reader.Url + "', sending to channels..."
                    );

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

                        foreach( string channel in channels )
                        {
                            writer.SendMessage(
                                msg,
                                channel
                            );
                        }
                    }
                }
                else
                {
                    this.pluginLogger.WriteLine(
                        Convert.ToInt32( LogVerbosityLevel.HighVerbosity ),
                        "No updates for RSS feed '" + reader.Url + "'"
                    );
                }
            }
            catch( Exception err )
            {
                this.pluginLogger.ErrorWriteLine(
                    "An Exception was caught while updating feed {0}:{1}{2}",
                    reader.FeedTitle ?? "<Unknown Title>",
                    Environment.NewLine,
                    err.ToString()
                );
            }
        }

        /// <summary>
        /// Handles the help message.
        /// </summary>
        public void HandleHelp( MessageHandlerArgs msgArgs, string[] helpArgs )
        {
            msgArgs.Writer.SendMessage(
                this.About + "  Don't like one of the feeds I post?  Yell at my admin!",
                msgArgs.Channel
            );
        }

        public void HandleDebug( MessageHandlerArgs args )
        {
            if( this.admins.Contains( args.User ) == false )
            {
                // Do Nothing
                return;
            }

            Match match = args.Match;
            string url = match.Groups["url"].Value;
            FeedReader reader = this.feedReaders.Values.FirstOrDefault( f => f.Url == url );
            Feed feed = this.rssConfig.Feeds.FirstOrDefault( f => f.Url == url );
            if( reader != null )
            {
                this.CheckForUpdates( reader, args.Writer, feed.Channels );
                args.Writer.SendMessage(
                    "Updating feed at URL " + url,
                    args.User
                );
            }
            else
            {
                args.Writer.SendMessage(
                    "Could not find feed that matches URL " + url,
                    args.User
                );
            }
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
