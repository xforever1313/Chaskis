//
//          Copyright Seth Hendrick 2018.
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
        // ---------------- Fields ----------------

        private readonly List<string> channels;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public Feed()
        {
            this.Url = string.Empty;
            this.RefreshInterval = TimeSpan.Zero;
            this.channels = new List<string>();
            this.Channels = this.channels.AsReadOnly();
        }

        /// <summary>
        /// Copy Constructor.
        /// </summary>
        public Feed( Feed feed )
        {
            this.Url = feed.Url;
            this.RefreshInterval = feed.RefreshInterval;
            this.channels = new List<string>( feed.channels );
            this.Channels = this.channels.AsReadOnly();
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

        /// <summary>
        /// Which channels to send feed updates to.
        /// </summary>
        public IList<string> Channels { get; set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Add channel to the <see cref="Channels"/> List.
        /// </summary>
        public void AddChannel( string channel )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( channel, nameof( channel ) );
            this.channels.Add( channel );
        }

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
            if( this.Channels.Count == 0 )
            {
                success = false;
                errorBuilder.AppendLine( "\t-There must be at least one channel specified." );
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
            return new Feed( this );
        }
    }
}
