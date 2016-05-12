
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;

namespace Chaskis.Plugins.CowSayBot
{
    public struct CowFileInfo
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the cowfile to open</param>
        /// <param name="command">String to look for in {%saycmd%}</param>
        public CowFileInfo( string name, string command )
        {
            this.Name = name;
            this.Command = command;
        }

        // -------- Properties --------

        /// <summary>
        /// The cowfile's name is what gets passed to the -f on the cowsay command line.  The exception is
        /// "DEFAULT", which just runs cowsay with no -f specified.T
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The command is the string the bot is looking for in {%saycmd%}.
        /// </summary>
        public string Command { get; set; }

        // -------- Functions ---------

        /// <summary>
        /// Ensures this object is valid.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this does not validate.</exception>
        public void Validate()
        {
            string errors = "Can not validate this CowFileInfo object:" + Environment.NewLine;
            bool success = true;

            if ( string.IsNullOrEmpty( this.Name ) || string.IsNullOrWhiteSpace( this.Name ) )
            {
                errors += nameof( this.Name ) + " can not be empty or null" + Environment.NewLine;
                success = false;
            }
            if ( string.IsNullOrEmpty( this.Command ) || string.IsNullOrWhiteSpace( this.Command ) )
            {
                errors += nameof( this.Command ) + " can not be empty or null" + Environment.NewLine;
                success = false;
            }

            if ( success == false )
            {
                throw new InvalidOperationException( errors );
            }
        }

        /// <summary>
        /// Makes a copy of this object.
        /// </summary>
        /// <returns>A copy of this object.</returns>
        public CowFileInfo Clone()
        {
            return ( CowFileInfo ) this.MemberwiseClone();
        }
    }
}
