//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SethCS.Exceptions;

namespace Chaskis.Plugins.IsItDownBot
{
    public class Website
    {
        // ---------------- Constructor ----------------

        public Website()
        {
            this.Channels = new List<string>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The URL to query to see if it is down.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// How often to check the website to see if
        /// it is up or not.
        /// </summary>
        public TimeSpan CheckInterval { get; set; }

        /// <summary>
        /// The channels to send the "is it down" message to.
        /// If empty, ALL channels will get the message.
        /// </summary>
        public IList<string> Channels { get; private set; }

        // ---------------- Functions ----------------

        public bool TryValidate( out string errorString )
        {
            bool success = true;
            StringBuilder errorMsg = new StringBuilder();
            errorMsg.AppendLine( "The following is wrong with the " + nameof( Website ) + " config:" );

            if( string.IsNullOrWhiteSpace( Url ) )
            {
                success = false;
                errorMsg.AppendLine( "\t-" + nameof( Url ) + " can not be null, empty, or whitespace" );
            }
            if( CheckInterval <= TimeSpan.Zero )
            {
                success = false;
                errorMsg.AppendLine( "\t-" + nameof( CheckInterval ) + " can not be zero or less" );
            }
            if( this.Channels.Any( s => string.IsNullOrWhiteSpace( s ) ) )
            {
                success = false;
                errorMsg.AppendLine( "\t-" + nameof( Channels ) + " can not have any elements that are null, empty, or whitespace" );
            }

            if( success )
            {
                errorString = string.Empty;
            }
            else
            {
                errorString = errorMsg.ToString();
            }

            return success;
        }

        /// <summary>
        /// Checks to see if this configuration object is valid.
        /// </summary>
        /// <exception cref="ValidationException">If this object is not valid.</exception>
        public void Validate()
        {
            string errorString;
            if( this.TryValidate( out errorString ) == false )
            {
                throw new ValidationException( errorString );
            }
        }
    }

    public class IsItDownBotConfig
    {
        // ---------------- Fields ----------------

        // ---------------- Constructor ----------------

        public IsItDownBotConfig()
        {
            this.CommandPrefix = "!isitdown";
            this.Websites = new List<Website>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The start of the command to see if a URL is down or not.
        /// </summary>
        public string CommandPrefix { get; set; }

        /// <summary>
        /// List of websites to automatically check to see if they are up.
        /// </summary>
        public IList<Website> Websites { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Checks to see if this configuration object is valid.
        /// </summary>
        /// <exception cref="ValidationException">If this object is not valid.</exception>
        public void Validate()
        {
            bool success = true;
            StringBuilder errorMsg = new StringBuilder();
            errorMsg.AppendLine( "The following is wrong with the " + nameof( IsItDownBotConfig ) );

            if( string.IsNullOrWhiteSpace( this.CommandPrefix ) )
            {
                success = false;
                errorMsg.AppendLine( "\t-" + nameof( CommandPrefix ) + " can not be null, empty, whitespace" );
            }

            foreach( Website site in this.Websites )
            {
                if( site == null )
                {
                    success = false;
                    errorMsg.AppendLine( "\t-" + nameof( Websites ) + " can not contain a null" );
                    continue;
                }

                string websiteError;
                if( site.TryValidate( out websiteError ) == false )
                {
                    success = false;
                    errorMsg.AppendLine( websiteError );
                }
            }

            if( success == false )
            {
                throw new ValidationException( errorMsg.ToString() );
            }
        }
    }
}
