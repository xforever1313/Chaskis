//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using SethCS.Exceptions;

namespace Chaskis.Plugins.RssBot
{
    public class FeedReader
    {
        // ---------------- Fields ----------------

        private Feed feedConfig;

        private SyndicationFeed feed;

        private const string userAgent = "Chaskis IRC RssBot";

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="feedConfig">The feed to read.</param>
        public FeedReader( Feed feedConfig )
        {
            ArgumentChecker.IsNotNull( feedConfig, nameof( feedConfig ) );

            this.feedConfig = feedConfig.Clone();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The title of the feed.  <see cref="string.Empty"/> until Init() is called.
        /// </summary>
        public string FeedTitle { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Inits the feed by pulling down the latest RSS info.
        /// </summary>
        public void Init()
        {
            this.feed = FetchFeed();
            this.FeedTitle = this.feed.Title.Text;
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
        public async Task<IList<SyndicationItem>> Update()
        {
            List<SyndicationItem> newItems = new List<SyndicationItem>();

            SyndicationFeed updatedFeed = await this.AsyncFetchFeed();

            // If we have more items than what our old feed is, we need to update.
            if( updatedFeed.Items.Count() > this.feed.Items.Count() )
            {
                foreach( SyndicationItem item in updatedFeed.Items )
                {
                    // If our item does not exist, call OnNewItem.
                    if( this.feed.Items.FirstOrDefault( i => i.Id == item.Id ) == null )
                    {
                        newItems.Add( item );
                    }
                }

                this.feed = updatedFeed;
            }

            newItems.Sort( this.SortByDate );

            return newItems;
        }

        /// <summary>
        /// Updates the feed in a background thread.
        /// </summary>
        /// <returns></returns>
        public Task<SyndicationFeed> AsyncFetchFeed()
        {
            return Task.Run( () => { return this.FetchFeed(); } );
        }

        private SyndicationFeed FetchFeed()
        {
            string contents;
            using( WebClient client = new WebClient() )
            {
                client.Headers.Add( "user-agent", userAgent );
                contents = client.DownloadString( this.feedConfig.Url );
            }

            using( StringReader reader = new StringReader( contents ) )
            {
                using( XmlReader xmlReader = XmlReader.Create( reader ) )
                {
                    return SyndicationFeed.Load( xmlReader );
                }
            }
        }

        private int SortByDate( SyndicationItem item1, SyndicationItem item2 )
        {
            return item1.LastUpdatedTime.CompareTo( item2.LastUpdatedTime );
        }
    }
}
