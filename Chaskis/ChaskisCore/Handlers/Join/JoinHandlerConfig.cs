//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public class JoinHandlerConfig
    {
        // ---------------- Constructor ----------------

        public JoinHandlerConfig()
        {
            this.RespondToSelf = false;
        }

        // ---------------- Properties ----------------

        public JoinHandlerAction JoinAction { get; set; }

        /// <summary>
        /// Does the bot respond to itself joining a channel?
        /// Defaulted to false.
        /// </summary>
        public bool RespondToSelf { get; set; }

        // ---------------- Functions ----------------

        public void Validate()
        {
            bool success = true;
            StringBuilder errorString = new StringBuilder();
            errorString.AppendLine( "Errors when validating " + nameof( JoinHandlerConfig ) );

            if( this.JoinAction == null )
            {
                success = false;
                errorString.AppendLine( "\t- " + nameof( this.JoinAction ) + " can not be null" );
            }

            if( success == false )
            {
                throw new ValidationException( errorString.ToString() );
            }
        }

        public JoinHandlerConfig Clone()
        {
            return (JoinHandlerConfig)this.MemberwiseClone();
        }
    }
}
