//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;
using SethCS.Exceptions;

namespace Chaskis.Plugins.RssBot
{
    public class Feed
    {
        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public Feed()
        {
            this.Url = string.Empty;
            this.RefreshInterval = TimeSpan.Zero;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The URL to pull.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// How often to pull the feed to check for updates.
        /// </summary>
        public TimeSpan RefreshInterval { get; set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Ensures this feed object was created correctly.
        /// </summary>
        /// <param name="errorString">What is wrong with this object in string form.  Empty if there is nothing.</param>
        /// <returns>True if this feed is a valid object, else false.</returns>
        public bool TryValidate( out string errorString )
        {
            bool success = true;
            StringBuilder errorBuilder = new StringBuilder();
            errorBuilder.AppendLine( "The following is wrong with a Feed:" );

            if( string.IsNullOrEmpty( this.Url ) || string.IsNullOrWhiteSpace( this.Url ) )
            {
                success = false;
                errorBuilder.AppendLine( "\t-URL can not be empty, null, or whitespace." );
            }
            if( this.RefreshInterval <= TimeSpan.Zero )
            {
                success = false;
                errorBuilder.AppendLine( "\t-Refresh interval can not be negative or zero." );
            }

            if( success )
            {
                errorString = string.Empty;
            }
            else
            {
                errorString = errorBuilder.ToString();
            }

            return success;
        }

        /// <summary>
        /// Creates a deep-copy of this object.
        /// </summary>
        public Feed Clone()
        {
            return (Feed)this.MemberwiseClone();
        }
    }

    public class RssBotConfig
    {
        // ---------------- Fields ----------------

        private List<Feed> feeds;

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
