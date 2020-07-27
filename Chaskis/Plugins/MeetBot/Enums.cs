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
}
