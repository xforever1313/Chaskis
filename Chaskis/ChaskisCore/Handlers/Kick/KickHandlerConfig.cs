//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public class KickHandlerConfig
    {
        // ---------------- Constructor ----------------

        public KickHandlerConfig()
        {
            this.RespondToSelfPerformingKick = false;
            this.RespondToSelfBeingKicked = false;
        }

        // ---------------- Properties ----------------

        public KickHandlerAction KickAction { get; set; }

        /// <summary>
        /// Does the bot respond if a message appears where it did the kicking?
        /// </summary>
        public bool RespondToSelfPerformingKick { get; set; }

        /// <summary>
        /// Does the bot respond to itself being kicked from a channel?
        /// Defaulted to false.
        /// </summary>
        public bool RespondToSelfBeingKicked { get; set; }

        // ---------------- Functions ----------------

        public void Validate()
        {
            bool success = true;
            StringBuilder errorString = new StringBuilder();
            errorString.AppendLine( "Errors when validating " + nameof( JoinHandlerConfig ) );

            if( this.KickAction == null )
            {
                success = false;
                errorString.AppendLine( "\t- " + nameof( this.KickAction ) + " can not be null" );
            }

            if( success == false )
            {
                throw new ValidationException( errorString.ToString() );
            }
        }

        public KickHandlerConfig Clone()
        {
            return (KickHandlerConfig)this.MemberwiseClone();
        }
    }
}
