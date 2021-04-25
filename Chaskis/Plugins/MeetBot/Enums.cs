//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Chaskis.Plugins.MeetBot
{
    /// <summary>
    /// See the Commands.xml file in the Documentation folder
    /// for what each of these actions represent.
    /// </summary>
    /// <remarks>
    /// Do not rename these or XML parsing will break.
    /// </remarks>
    public enum MeetingAction
    {
        Unknown,

        StartMeeting,

        EndMeeting,

        Topic,

        Agree,

        Chair,

        Unchair,

        Action,

        Info,

        Link,

        Unlink,

        MeetingTopic,

        Help,

        Accept,

        Reject,

        Save,

        CancelMeeting,

        Purge,

        Silence,

        Voice,

        Banish
    }

    /// <summary>
    /// Who is allowed to execute a command?
    /// All commands are defaulted to <see cref="CommandRestriction.Anyone"/>
    /// </summary>
    /// <remarks>
    /// Do not rename these or XML parsing will break.
    /// </remarks>
    public enum CommandRestriction
    {
        Unknown,

        /// <summary>
        /// Anyone can perform a command.
        /// </summary>
        Anyone,

        /// <summary>
        /// Only Chairs and Bot Admins can use this command.
        /// </summary>
        ChairsAndBotAdmins,

        /// <summary>
        /// Only chairs can use the command.
        /// </summary>
        ChairsOnly
    }

    /// <summary>
    /// How to generate meeting notes.
    /// </summary>
    public enum MeetingNotesGeneratorType
    {
        Unknown,

        /// <summary>
        /// Generate the meeting notes into a default XML file that
        /// can later be parsed out.
        /// </summary>
        /// <remarks>
        /// Lower-case so we can parse the enum from the config XML.
        /// </remarks>
        xml,

        /// <summary>
        /// Generate the meeting notes into an html file generated from
        /// a cshtml file.
        /// </summary>
        /// <remarks>
        /// Lower-case so we can parse the enum from the config XML.
        /// </remarks>
        html,

        /// <summary>
        /// Generate the meeting notes into an html file generated from
        /// a cstxt file.
        /// </summary>
        /// <remarks>
        /// Lower-case so we can parse the enum from the config XML.
        /// </remarks>
        txt
    }

    public enum ParseMessageResult
    {
        /// <summary>
        /// The message was parsed successfully, and added
        /// to the meeting notes.
        /// </summary>
        Success,

        /// <summary>
        /// A command is reserved for chairs only,
        /// and the user who tried it is not a chair.
        /// </summary>
        ChairOnlyCommand,

        /// <summary>
        /// A command is reserved for chairs and bot admins
        /// only, and the user who tried it is neither of those.
        /// </summary>
        ChairBotAdminOnlyMessage,

        /// <summary>
        /// The user who sent a command is silenced.
        /// </summary>
        UserIsSilenced,

        /// <summary>
        /// An action is attempted on the owner (such as dechairing or banning),
        /// that the owner is immune to.
        /// </summary>
        CanNotDoThisToOwner,

        /// <summary>
        /// An action is attempted to another chair (such as banning or silencing)
        /// that a chair is immune to.  A chair must be dechaired first.
        /// </summary>
        CanNotDoThisToChair
    }
}
