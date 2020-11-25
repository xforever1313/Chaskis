//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Chaskis.Plugins.MessageFixerBot
{
    public class MessageFixer
    {
        // ---------------- Fields ----------------

        private const string FilterPattern = @"^s/(?<findpattern>(\\/|[^/])+)/(?<replacestr>(\\/|[^/])+)$";

        private static readonly Regex filterRegex = new Regex( FilterPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture );

        /// <summary>
        /// Dictionary of the previous message the user sent us.
        /// Key is the user, value is the message.
        /// </summary>
        private readonly Dictionary<string, string> lastMessages;

        // ---------------- Constructor ----------------

        public MessageFixer()
        {
            this.lastMessages = new Dictionary<string, string>();
            this.LastMessages = new ReadOnlyDictionary<string, string>( this.lastMessages );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The last messages that were read from each user.
        /// </summary>
        public IReadOnlyDictionary<string, string> LastMessages { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Record a new IRC message.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        public MessageFixerResult RecordNewMessage( string user, string msg )
        {
            bool success;
            string messageToReport;

            // If our regex is our filter regex, do NOT save this as the user's
            // previous message.
            Match match = filterRegex.Match( msg );
            if( match.Success )
            {
                if( this.lastMessages.ContainsKey( user ) )
                {
                    string findString = match.Groups["findpattern"].Value.Replace( @"\/", "/" );
                    string replaceString = match.Groups["replacestr"].Value.Replace( @"\/", "/" );

                    try
                    {
                        string oldMessage = this.lastMessages[user];

                        // Does our find string actually match?
                        Match oldmessageMatch = Regex.Match( oldMessage, findString, RegexOptions.IgnoreCase );
                        if( oldmessageMatch.Success )
                        {
                            string newMessage = Regex.Replace( oldMessage, findString, replaceString, RegexOptions.IgnoreCase );

                            success = true;
                            messageToReport = newMessage;
                        }
                        else
                        {
                            success = false;
                            messageToReport = "Your find regex doesn't match anything in your previous message, can not edit.";
                        }
                    }
                    catch( Exception e )
                    {
                        success = false;
                        messageToReport = e.Message;
                    }
                }
                else
                {
                    // No previous message means there is nothing to fix.
                    // No-Op.
                    success = false;
                    messageToReport = "You don't have a previous message to fix since I joined!";
                }
            }
            // Otherwise, that becomes this user's previous message.
            else
            {
                lastMessages[user] = msg;
                success = false;
                messageToReport = string.Empty;
            }

            return new MessageFixerResult( success, messageToReport );
        }
    }
}
