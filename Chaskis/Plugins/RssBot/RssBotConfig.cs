//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Text;
using SethCS.Exceptions;

namespace Chaskis.Plugins.RssBot
{
    public class RssBotConfig
    {
        // ---------------- Fields ----------------

        private readonly List<Feed> feeds;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public RssBotConfig()
        {
            this.feeds = new List<Feed>();
            this.Feeds = feeds.AsReadOnly();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Read-only list of feeds.
        /// </summary>
        public IReadOnlyList<Feed> Feeds { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Adds a cloned version of the passed-in feed
        /// to the feed list.
        /// </summary>
        /// <param name="feed">The feed to add.</param>
        public void AddFeed( Feed feed )
        {
            this.feeds.Add( feed.Clone() );
        }

        /// <summary>
        /// Ensures all feeds are valid.
        /// </summary>
        /// <exception cref="ValidationException">If a feed is invalid.</exception>
        public void Validate()
        {
            bool success = true;
            StringBuilder errorBuilder = new StringBuilder();

            if( this.Feeds.Count == 0 )
            {
                success = false;
                errorBuilder.AppendLine( "No feeds have been added!" );
            }

            foreach( Feed feed in this.Feeds )
            {
                string errorString;
                if( feed.TryValidate( out errorString ) == false )
                {
                    success = false;
                    errorBuilder.AppendLine( errorString );
                }
            }

            if( success == false )
            {
                throw new ValidationException( errorBuilder.ToString() );
            }
        }
    }
}
