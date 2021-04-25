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
    /// <summary>
    /// Configuration used for <see cref="AnyInterPluginEventHandler"/>
    /// </summary>
    public class AnyInterPluginEventHandlerConfig
    {
        // ---------------- Constructor ----------------

        public AnyInterPluginEventHandlerConfig()
        {
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The action to take for ANY inter-plugin event.  It is up to this action to
        /// actually parse the string.
        /// </summary>
        public AnyInterPluginEventHandlerAction LineAction { get; set; }

        // ---------------- Functions ----------------

        public void Validate()
        {
            bool success = true;
            StringBuilder errorString = new StringBuilder();
            errorString.AppendLine( "Errors when validating " + nameof( AnyInterPluginEventHandlerConfig ) );

            if( this.LineAction == null )
            {
                success = false;
                errorString.AppendLine( "\t- " + nameof( this.LineAction ) + " can not be null" );
            }

            if( success == false )
            {
                throw new ValidationException( errorString.ToString() );
            }
        }

        public AnyInterPluginEventHandlerConfig Clone()
        {
            return (AnyInterPluginEventHandlerConfig)this.MemberwiseClone();
        }
    }
}
