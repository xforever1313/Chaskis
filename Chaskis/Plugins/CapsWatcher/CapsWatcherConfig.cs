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

namespace Chaskis.Plugins.CapsWatcher
{
    public class CapsWatcherConfig
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// Sets the message list to empty.
        /// </summary>
        public CapsWatcherConfig()
        {
            this.Messages = new List<string>();
            this.Ignores = new List<string>();
        }

        // -------- Properties --------

        /// <summary>
        /// List of messages to send to an offending user.
        /// </summary>
        public IList<string> Messages { get; private set; }

        /// <summary>
        /// Before messages are checked for caps, these get filted out.
        /// 
        /// These can include acronym, or other things that need to be ignored.
        /// 
        /// White Space is allowed!
        /// </summary>
        public IList<string> Ignores { get; private set; }

        /// <summary>
        /// Validates this class.
        /// Must have at least one message loaded,
        /// and no message can be empty or null.
        /// </summary>
        /// <exception cref="ValidationException">If the configuration isnot valid.</exception>
        public void Validate()
        {
            bool success = true;
            StringBuilder errorMessage = new StringBuilder();
            errorMessage.AppendLine( "Error validating Caps Watcher Config:" );

            if( this.Messages.Count == 0 )
            {
                success = false;
                errorMessage.AppendLine( "\tMessage list can not be empty." );
            }

            foreach( string msg in this.Messages )
            {
                if( string.IsNullOrEmpty( msg ) )
                {
                    success = false;
                    errorMessage.AppendLine( "\tFound a null or empty message in the message list." );
                }
            }

            foreach( string ignore in this.Ignores )
            {
                if( string.IsNullOrEmpty( ignore ) )
                {
                    success = false;
                    errorMessage.AppendLine( "\tFound a null or empty ignore in the ignore list." );
                }
            }

            if( success == false )
            {
                throw new ValidationException( errorMessage.ToString() );
            }
        }
    }
}