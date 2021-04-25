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
    public class PartHandlerConfig
    {
        // ---------------- Constructor ----------------

        public PartHandlerConfig()
        {
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The action that gets triggered when a user Parts from the channel.
        /// </summary>
        public PartHandlerAction PartAction { get; set; }

        // ---------------- Functions ----------------

        public void Validate()
        {
            bool success = true;
            StringBuilder errorString = new StringBuilder();
            errorString.AppendLine( "Errors when validating " + nameof( PartHandlerConfig ) );

            if( this.PartAction == null )
            {
                success = false;
                errorString.AppendLine( "\t- " + nameof( this.PartAction ) + " can not be null" );
            }

            if( success == false )
            {
                throw new ValidationException( errorString.ToString() );
            }
        }

        public PartHandlerConfig Clone()
        {
            return (PartHandlerConfig)this.MemberwiseClone();
        }
    }
}
