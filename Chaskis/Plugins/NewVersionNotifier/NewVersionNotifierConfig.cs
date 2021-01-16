//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;
using SethCS.Exceptions;
using SethCS.Extensions;

namespace Chaskis.Plugins.NewVersionNotifier
{
    public class NewVersionNotifierConfig : IEquatable<NewVersionNotifierConfig>
    {
        // ---------------- Fields ----------------

        // ---------------- Constructor ----------------

        public NewVersionNotifierConfig()
        {
            this.Message =
                "I have been updated to version {%version%}.  Release Notes: https://github.com/xforever1313/Chaskis/releases/tag/{%version%}";

            this.ChannelsToSendTo = new List<string>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The message to send out if we have a new Chaskis version.
        /// {%version%} is replaced with the version string.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Channels to send the new version notification to.
        /// If this is empty, then the notification is sent
        /// to ALL channels the bot joins.
        /// </summary>
        public IList<string> ChannelsToSendTo { get; private set; }

        // ---------------- Functions ----------------

        public override bool Equals( object obj )
        {
            NewVersionNotifierConfig other = obj as NewVersionNotifierConfig;
            return this.Equals( other );
        }

        public bool Equals( NewVersionNotifierConfig other )
        {
            if( other == null )
            {
                return false;
            }
            return
                ( this.Message == other.Message ) &&
                // Order doesn't matter here.
                this.ChannelsToSendTo.EqualsIgnoreOrder( other.ChannelsToSendTo );
        }

        public override int GetHashCode()
        {
            // This object is mutable, so use the base implementation.
            return base.GetHashCode();
        }

        /// <summary>
        /// Ensures this config is valid.
        /// </summary>
        public void Validate()
        {
            StringBuilder errorString = new StringBuilder();
            bool success = true;

            errorString.AppendLine( "The following is wrong with " + nameof( NewVersionNotifierConfig ) );

            if( string.IsNullOrWhiteSpace( this.Message ) )
            {
                success = false;
                errorString.AppendLine( "\t-" + nameof( Message ) + " can not be null, empty, or whitespace." );
            }

            foreach( string channel in this.ChannelsToSendTo )
            {
                if( string.IsNullOrWhiteSpace( channel ) )
                {
                    success = false;
                    errorString.AppendLine( "\t-" + nameof( ChannelsToSendTo ) + " can not contain a null, empty, or whitespace channel." );
                }
            }

            if( success == false )
            {
                throw new ValidationException( errorString.ToString() );
            }
        }
    }
}
