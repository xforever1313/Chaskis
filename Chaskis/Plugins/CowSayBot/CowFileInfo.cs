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

namespace Chaskis.Plugins.CowSayBot
{
    public class CowFileInfo
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the cowfile to open</param>
        /// <param name="command">String to look for in {%saycmd%}</param>
        public CowFileInfo()
        {
            this.CommandList = new Dictionary<string, string>();
        }

        // -------- Properties --------

        /// <summary>
        /// List of commands and which cowfile to use.  The key is the command (e.g. cowsay or tuxsay),
        /// and the value is the cowfile to use.  DEFAULT cowfile is running cowsay with no cowfile specified.
        /// </summary>
        public IDictionary<string, string> CommandList { get; private set; }

        // -------- Functions ---------

        /// <summary>
        /// Ensures this object is valid.
        /// </summary>
        /// <exception cref="ValidationException">Thrown when this does not validate.</exception>
        public void Validate()
        {
            StringBuilder errorMessage = new StringBuilder();
            errorMessage.AppendLine( "Can not validate this CowFileInfo object:" );
            bool success = true;

            if( CommandList.Count == 0 )
            {
                errorMessage.AppendLine( nameof( this.CommandList ) + " can not be empty." );
                success = false;
            }
            else
            {
                foreach( KeyValuePair<string, string> command in this.CommandList )
                {
                    if( string.IsNullOrEmpty( command.Key ) || string.IsNullOrWhiteSpace( command.Key ) )
                    {
                        errorMessage.AppendLine( "Can not have empty or null command in " + nameof( this.CommandList ) );
                        success = false;
                    }
                    if( string.IsNullOrEmpty( command.Value ) || string.IsNullOrWhiteSpace( command.Value ) )
                    {
                        errorMessage.AppendLine( "Can not have empty or null cowfile in " + nameof( this.CommandList ) );
                        success = false;
                    }
                }
            }

            if( success == false )
            {
                throw new ValidationException( errorMessage.ToString() );
            }
        }

        /// <summary>
        /// Makes a copy of this object.
        /// </summary>
        /// <returns>A copy of this object.</returns>
        public CowFileInfo Clone()
        {
            CowFileInfo clone = (CowFileInfo)this.MemberwiseClone();
            clone.CommandList = new Dictionary<string, string>( clone.CommandList );
            return clone;
        }
    }
}