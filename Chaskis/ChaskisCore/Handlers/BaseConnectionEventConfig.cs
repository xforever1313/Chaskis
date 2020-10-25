//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public abstract class BaseConnectionEventConfig<TChildType, TLineAction, TLineActionArgs> : IConnectionEvent
        where TChildType : IConnectionEvent
        where TLineAction : Delegate
        where TLineActionArgs : IConnectionEventArgs
    {
        // ---------------- Constructor ----------------

        protected BaseConnectionEventConfig()
        {
        }

        // ---------------- Properties ----------------

        public TLineAction LineAction { get; set; }

        // ---------------- Functions ----------------

        public void Validate()
        {
            List<string> errors = new List<string>();

            if( this.LineAction== null )
            {
                errors.Add( $"{nameof( this.LineAction )} can not be null" );
            }

            IEnumerable<string> childErrors = ValidateChild();
            if( childErrors != null )
            {
                errors.AddRange( childErrors );
            }

            if( errors.Count != 0 )
            {
                throw new ListedValidationException(
                    $"Errors when validating {typeof( TChildType ).Name}",
                    errors
                 );
            }
        }

        public abstract TChildType Clone();

        /// <summary>
        /// Validate the child's properties, if any.
        /// </summary>
        /// <returns>
        /// A list of strings that are wrong with the child node.
        /// Return null or an empty list if nothing is wrong.
        /// </returns>
        protected abstract IEnumerable<string> ValidateChild();
    }
}
