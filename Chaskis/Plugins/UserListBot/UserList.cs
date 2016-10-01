//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Chaskis.Plugins.UserListBot
{
    /// <summary>
    /// This class keeps track of the users.
    /// </summary>
    public class UserList
    {
        // -------- Fields --------

        /// <summary>
        /// Regex that contains the message from the server
        /// that contains the user names in the channel.
        ///
        /// Message 353 from here: https://www.alien.net.au/irc/irc2numerics.html
        /// ( '=' / '*' / '@' ) <channel> ' ' : [ '@' / '+' ] <nick> *( ' ' [ '@' / '+' ] <nick> )
        /// </summary>
        public const string UserNameResponseRegex =
            @"353.+[@=*]\s+(?<channel>[#\w]+)\s+:(?<names>.+)$";

        /// <summary>
        /// Regex that contains the message from the server
        /// when it is done sending the user names.
        ///
        /// Message 366 from here: https://www.alien.net.au/irc/irc2numerics.html
        /// <channel> :<info>
        /// </summary>
        public const string UserNameEndRegex =
            @"366.+\s+(?<channel>[#\w]+)\s+:(?<info>.+)$";

        /// <summary>
        /// Dictionary that stores the users in a channel.
        /// Key is the channel.
        /// Value is a string of the users in the channel.
        /// </summary>
        private Dictionary<string, string> usersPerChannel;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public UserList()
        {
            this.usersPerChannel = new Dictionary<string, string>();
            this.UsersPerChannel = new ReadOnlyDictionary<string, string>( this.usersPerChannel );
            this.ResetStates();
        }

        // -------- Properties --------

        /// <summary>
        /// Read-only dictionary of users per channel.
        /// </summary>
        public IReadOnlyDictionary<string, string> UsersPerChannel;

        // -------- Functions --------

        /// <summary>
        /// Clears the user list.
        /// </summary>
        public void ResetStates()
        {
            this.usersPerChannel.Clear();
        }

        /// <summary>
        /// Adds the list of names to the response string.
        /// </summary>
        /// <param name="list">The response to parse from the server.</param>
        public void ParseNameResponse( string nameResponse )
        {
            Match match = Regex.Match( nameResponse, UserNameResponseRegex );
            if( match.Success )
            {
                string channel = match.Groups["channel"].Value;
                string names = match.Groups["names"].Value;
                if( this.usersPerChannel.ContainsKey( channel ) )
                {
                    this.usersPerChannel[channel] += " " + names;
                }
                else
                {
                    this.usersPerChannel[channel] = names;
                }
            }
        }

        /// <summary>
        /// Checks for the end of names message from the server.
        /// If its found, this returns a tuple whose first value is the channel
        /// and whose second value is the string of user names.  When something is returned,
        /// the internal user list is cleared.
        ///
        /// If not, this returns null.
        /// </summary>
        /// <param name="serverResponse">A response from the server.</param>
        /// <returns>Null if end-of-names not received.  Otherwise, tuple of a channel and the name string.</returns>
        public Tuple<string, string> CheckAndHandleEndMessage( string serverResponse )
        {
            Tuple<string, string> userList = null;

            Match match = Regex.Match( serverResponse, UserNameEndRegex );
            if( match.Success )
            {
                string channel = match.Groups["channel"].Value;
                if( this.usersPerChannel.ContainsKey( channel ) )
                {
                    userList = new Tuple<string, string>(
                        channel,
                        this.usersPerChannel[channel]
                    );

                    this.usersPerChannel.Remove( channel );
                }
            }

            return userList;
        }
    }
}