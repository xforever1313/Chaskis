//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using SethCS.Exceptions;

namespace Chaskis.Plugins.RssBot
{
    public class FeedReader
    {
        // ---------------- Fields ----------------

        private readonly Feed feedConfig;

        private SyndicationFeed feed;
        private readonly object feedLock;

        private readonly HttpClient httpClient;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="feedConfig">The feed to read.</param>
        public FeedReader( Feed feedConfig, HttpClient httpClient )
        {
            ArgumentChecker.IsNotNull( feedConfig, nameof( feedConfig ) );
            ArgumentChecker.IsNotNull( httpClient, nameof( httpClient ) );

            this.feedConfig = feedConfig.Clone();
            this.feedLock = new object();

            this.httpClient = httpClient;
            this.Url = feedConfig.Url;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The title of the feed.  <see cref="string.Empty"/> until Init() is called.
        /// </summary>
        public string FeedTitle { get; private set; }

        public string Url { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Inits the feed by pulling down the latest RSS info.
        /// </summary>
        public void Init()
        {
            Task<SyndicationFeed> task = this.FetchFeed();
            task.Wait();

            SortFeeds( task.Result );
        }

        /// <summary>
        /// Updates the RSS feed and fires the OnUpdate event if there's an update.
        /// </summary>
        /// <returns>
        /// A list of items that have changed since the last update.
        /// Will NOT be null.  Will return empty list if there are none.
        /// 
        /// Items are sorted by last updated time, with the oldest being in
        /// index 0.
        /// </returns>
        public async Task<IList<SyndicationItem>> UpdateAsync()
        {
            SyndicationFeed updatedFeed = await this.FetchFeedAsync();
            return SortFeeds( updatedFeed );
        }

        private List<SyndicationItem> SortFeeds( SyndicationFeed latestFeed )
        {
            List<SyndicationItem> newItems = new List<SyndicationItem>();

            // this.feed can be modified by multiple threads if UpdateAsync() is called multiple times...
            // lock it up.
            lock( this.feedLock )
            {
                if( this.feed == null )
                {
                    // If we are null, we have not gotten any feed data yet.
                    // Therefore, do not add any new items as we do not want to spam
                    // channels.
                    //
                    // This can happen if when the IRC bot starts up, the URL to get the
                    // feed is down during initialization, so we get nothing.

                    this.feed = latestFeed;
                    this.FeedTitle = this.feed.Title.Text;
                }
                else
                {
                    // If our feed is not null, then we have at least 1 update.
                    // we can post to channels any new updates.
                    foreach( SyndicationItem item in latestFeed.Items )
                    {
                        // If our item does not exist, call OnNewItem.
                        if( this.feed.Items.FirstOrDefault( i => i.Id == item.Id ) == null )
                        {
                            newItems.Add( item );
                        }
                    }
                    this.feed = latestFeed;
                }
            }

            newItems.Sort( this.SortByDate );
            return newItems;
        }

        private async Task<SyndicationFeed> FetchFeed()
        {
            HttpResponseMessage response = await this.httpClient.GetAsync( this.feedConfig.Url );
            if( response.IsSuccessStatusCode )
            {
                using( Stream content = await response.Content.ReadAsStreamAsync() )
                {
                    using( XmlReader xmlReader = XmlReader.Create( content ) )
                    {
                        return SyndicationFeed.Load( xmlReader );
                    }
                }
            }
            else
            {
                throw new HttpRequestException( "Error when getting HTTP: " + Environment.NewLine + response.StatusCode );
            }
        }

        private Task<SyndicationFeed> FetchFeedAsync()
        {
            return Task.Run( () => FetchFeed() );
        }

        private int SortByDate( SyndicationItem item1, SyndicationItem item2 )
        {
            return item1.LastUpdatedTime.CompareTo( item2.LastUpdatedTime );
        }
    }
}
