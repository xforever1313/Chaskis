//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
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
        }

        // -------- Properties --------

        /// <summary>
        /// List of messages to send to an offending user.
        /// </summary>
        public IList<string> Messages;

        /// <summary>
        /// Validates this class.
        /// Must have at least one message loaded,
        /// and no message can be empty or null.
        /// </summary>
        /// <exception cref="ValidationException">If the configuration isnot valid.</exception>
        public void Validate()
        {
            bool success = true;
            string errorMessage = "Error validating Caps Watcher Config:" + Environment.NewLine;

            if( this.Messages.Count == 0 )
            {
                success = false;
                errorMessage += "\tMessage list can not be empty." + Environment.NewLine;
            }

            foreach( string msg in this.Messages )
            {
                if( string.IsNullOrEmpty( msg ) )
                {
                    success = false;
                    errorMessage += "\tFound a null or empty message in the message list." + Environment.NewLine;
                }
            }

            if( success == false )
            {
                throw new ValidationException( errorMessage );
            }
        }
    }
}