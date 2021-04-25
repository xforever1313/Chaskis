//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Chaskis.Plugins.MeetBot
{
    public class Meeting
    {
        // ---------------- Fields ----------------

        private readonly List<MeetingNote> meetingNotes;

        private readonly HashSet<string> chairs;
        private readonly HashSet<string> silencedUsers;
        private readonly HashSet<string> bannedUsers;

        private readonly IEnumerable<string> botAdmins;

        // ---------------- Constructor ----------------

        public Meeting( IMeetingInfo meetingInfo, IEnumerable<string> botAdmins )
        {
            this.MeetingInfo = meetingInfo;

            this.meetingNotes = new List<MeetingNote>();
            this.MeetingNotes = this.meetingNotes.AsReadOnly();

            this.chairs = new HashSet<string>();
            this.Chairs = this.chairs;

            this.silencedUsers = new HashSet<string>();
            this.SilencedUsers = silencedUsers;

            this.bannedUsers = new HashSet<string>();
            this.BannedUsers = bannedUsers;

            this.botAdmins = botAdmins;

            this.chairs.Add( meetingInfo.Owner );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Information about the meeting.
        /// </summary>
        public IMeetingInfo MeetingInfo { get; private set; }

        /// <summary>
        /// List of meeting notes in the order they occurred.
        /// Index 0 being the first meeting note.
        /// </summary>
        public IReadOnlyList<IReadOnlyMeetingNote> MeetingNotes { get; private set; }

        /// <summary>
        /// Collection of all chairs.
        /// All users are lower-cased before being put into the list.
        /// </summary>
        public IReadOnlyCollection<string> Chairs { get; private set; }

        /// <summary>
        /// Collection of all silenced users.  Silenced users will not be added
        /// to the meeting notes while they are silenced.
        /// All users are lower-cased before being put into the list.
        /// </summary>
        public IReadOnlyCollection<string> SilencedUsers { get; private set; }

        /// <summary>
        /// Collection of all banned users.  Banned users will not be added
        /// to the meeting notes, and are purged from the meeting notes as well.
        /// All users are lower-cased before being put into the list.
        /// </summary>
        public IReadOnlyCollection<string> BannedUsers { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Parses the message and updates the meeting notes as needed.
        /// </summary>
        /// <param name="msg">The message received.</param>
        /// <param name="user">Who send the message?</param>
        /// <param name="timestamp">
        /// The timestamp the message was sent.
        /// Set to null for <see cref="DateTime.UtcNow"/>
        /// </param>
        public ParseMessageResult ParseMessage( IMeetingMessage msg, string user, DateTime? timestamp = null )
        {
            user = user.ToLower();

            // Do not add to the meeting notes if the user who sent the message
            // is banned or silenced.
            if( this.bannedUsers.Contains( user ) || this.silencedUsers.Contains( user ) )
            {
                return ParseMessageResult.UserIsSilenced;
            }
            else if( msg.MeetingAction == MeetingAction.Help )
            {
                // No need to add help messages to the meeting notes.
                return ParseMessageResult.Success;
            }

            MeetingNote note = new MeetingNote
            {
                MeetingAction = msg.MeetingAction,
                TimeStamp = timestamp ?? DateTime.UtcNow,
                UserName = user
            };

            // If our action is unknown, so just a standard message,
            // there is no command prefix, the our message becomes the command args.
            if( msg.MeetingAction == MeetingAction.Unknown )
            {
                note.Message = msg.CommandArgs;
            }
            // Otherwise, our full message is both the command prefix and the full arguments.
            else
            {
                note.Message = $"{msg.CommandPrefix} {msg.CommandArgs}";
            }

            this.meetingNotes.Add( note );

            ParseMessageResult? restrictionResult = this.TryCheckRestriction( msg, user );
            if( restrictionResult != null )
            {
                // If a user does not have permission,
                // the message they sent simply becomes a standard message,
                // and will not be highlighted in the meeting notes.
                //
                // ... My opinion is that it should show up in the meeting notes
                // because I think it is important to show intent of all meeting goers.  A chair can purge
                // a user if they feel like they are being annoying.
                note.MeetingAction = MeetingAction.Unknown;

                // Stop here and return if there is a restriction.
                return restrictionResult.Value;
            }

            // If there is no restriction, proceed with the parsing.
            ParseMessageResult result;
            switch( msg.MeetingAction )
            {
                case MeetingAction.Chair:
                    result = HandleChair( msg );
                    break;

                case MeetingAction.Unchair:
                    result = HandleUnChair( msg );
                    break;

                case MeetingAction.Purge:
                    result = HandlePurge( msg );
                    break;

                case MeetingAction.Voice:
                    result = HandleVoice( msg );
                    break;

                case MeetingAction.Silence:
                    result = HandleSilence( msg );
                    break;

                case MeetingAction.Banish:
                    result = HandleBan( msg );
                    break;

                default:
                    // All other comamnds to not need special handling,
                    // return success.
                    result = ParseMessageResult.Success;
                    break;
            }

            // If we made it this far, success!
            return result;
        }

        private ParseMessageResult HandleChair( IMeetingMessage msg )
        {
            Action<string> action = delegate ( string user )
            {
                if( this.chairs.Contains( user ) == false )
                {
                    this.chairs.Add( user );
                }
            };

            DoUserSplitAction( msg.CommandArgs, action );

            // Not really much that can go wrong here...
            return ParseMessageResult.Success;
        }

        private ParseMessageResult HandleUnChair( IMeetingMessage msg )
        {
            ParseMessageResult result = ParseMessageResult.Success;

            Action<string> action = delegate ( string user )
            {
                if( this.chairs.Contains( user ) )
                {
                    if( user != this.MeetingInfo.Owner )
                    {
                        this.chairs.Remove( user );
                    }
                    else
                    {
                        // Override our result if we attempt to do this to an owner.
                        result = ParseMessageResult.CanNotDoThisToOwner;
                    }
                }
            };

            DoUserSplitAction( msg.CommandArgs, action );

            return result;
        }

        private ParseMessageResult HandlePurge( IMeetingMessage msg )
        {
            Action<string> action = delegate ( string user )
            {
                PurgeUser( user );
            };

            DoUserSplitAction( msg.CommandArgs, action );

            // Not really much that can go wrong here...
            return ParseMessageResult.Success;
        }

        private ParseMessageResult HandleVoice( IMeetingMessage msg )
        {
            Action<string> action = delegate ( string user )
            {
                if( this.silencedUsers.Contains( user ) )
                {
                    this.silencedUsers.Remove( user );
                }
            };

            DoUserSplitAction( msg.CommandArgs, action );

            // Not really much that can go wrong here...
            return ParseMessageResult.Success;
        }

        private ParseMessageResult HandleSilence( IMeetingMessage msg )
        {
            ParseMessageResult result = ParseMessageResult.Success;

            Action<string> action = delegate ( string user )
            {
                if( this.chairs.Contains( user ) )
                {
                    result = ParseMessageResult.CanNotDoThisToChair;
                }
                else
                {
                    this.silencedUsers.Add( user );
                }
            };

            DoUserSplitAction( msg.CommandArgs, action );

            return result;
        }

        private ParseMessageResult HandleBan( IMeetingMessage msg )
        {
            ParseMessageResult result = ParseMessageResult.Success;

            Action<string> action = delegate ( string user )
            {
                if( this.chairs.Contains( user ) )
                {
                    result = ParseMessageResult.CanNotDoThisToChair;
                }
                else
                {
                    if( this.bannedUsers.Contains( user ) == false )
                    {
                        this.bannedUsers.Add( user );
                    }

                    PurgeUser( user );
                }
            };

            DoUserSplitAction( msg.CommandArgs, action );

            return result;
        }

        private ParseMessageResult? TryCheckRestriction( IMeetingMessage msg, string user )
        {
            if( msg.Restriction == CommandRestriction.ChairsOnly )
            {
                if( this.chairs.Contains( user ) == false )
                {
                    return ParseMessageResult.ChairOnlyCommand;
                }
            }
            else if( msg.Restriction == CommandRestriction.ChairsAndBotAdmins )
            {
                if( ( this.chairs.Contains( user ) == false ) && ( this.botAdmins.Contains( user ) == false ) )
                {
                    return ParseMessageResult.ChairBotAdminOnlyMessage;
                }
            }

            return null;
        }

        private void DoUserSplitAction( string stringToSplit, Action<string> action )
        {
            string[] users = stringToSplit.ToLower().Split( ' ' );
            foreach( string user in users )
            {
                if( string.IsNullOrWhiteSpace( user ) == false )
                {
                    action( user );
                }
            }
        }

        private void PurgeUser( string user )
        {
            this.meetingNotes.RemoveAll(
                r => r.UserName.Equals( user )
            );
        }
    }
}
