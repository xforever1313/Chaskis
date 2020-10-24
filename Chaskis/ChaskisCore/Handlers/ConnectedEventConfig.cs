//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    /// <summary>
    /// Event to configure <see cref="ConnectedEventHandler"/>
    /// </summary>
    public class ConnectedEventConfig
    {
        // ---------------- Constructor ----------------

        public ConnectedEventConfig()
        {
        }

        // ---------------- Properties ----------------

        public ConnectedHandlerAction ConnectedAction { get; set; }

        // ---------------- Functions ----------------

        public void Validate()
        {
            List<string> errors = new List<string>();

            if( ConnectedAction == null )
            {
                errors.Add( $"{nameof( this.ConnectedAction )} can not be null" );
            }

            if( errors.Count != 0 )
            {
                throw new ListedValidationException(
                    $"Errors when validating {nameof( ConnectedEventConfig )}",
                    errors
                 );
            }
        }

        public ConnectedEventConfig Clone()
        {
            return (ConnectedEventConfig)this.MemberwiseClone();
        }
    }
}
