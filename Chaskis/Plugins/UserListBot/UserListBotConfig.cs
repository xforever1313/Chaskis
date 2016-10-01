//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;

namespace Chaskis.Plugins.UserListBot
{
    /// <summary>
    /// Config for the User List Bot.
    /// </summary>
    public class UserListBotConfig
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// Sets everything to a default value.
        /// </summary>
        public UserListBotConfig()
        {
            this.Command = "!userlist";
            this.Cooldown = 60;
        }

        // -------- Properties --------

        /// <summary>
        /// The command that triggers the bot to print the user list.
        /// Defaulted to "!userlist"
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The cooldown time in seconds before the bot responds to the command again.
        /// Defaulted to 60 seconds.
        /// </summary>
        public int Cooldown { get; set; }

        // -------- Functions --------

        /// <summary>
        /// Ensures the config is in a valid state.
        /// Command can not be null or empty.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this does not validate.</exception>
        public void Validate()
        {
            string errorMessage = "Can not validate this " + nameof( UserListBotConfig ) + " object:" + Environment.NewLine;
            bool success = true;

            if( string.IsNullOrEmpty( this.Command ) || ( string.IsNullOrWhiteSpace( this.Command ) ) )
            {
                errorMessage += "\t- " + nameof( this.Command ) + " can not be null or empty." + Environment.NewLine;
                success = false;
            }
            if( this.Cooldown < 0 )
            {
                errorMessage += "\t- " + nameof( this.Cooldown ) + " can not be less than zero." + Environment.NewLine;
                success = false;
            }

            if( success == false )
            {
                throw new InvalidOperationException( errorMessage );
            }
        }
    }
}