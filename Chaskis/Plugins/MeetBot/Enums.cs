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
}
