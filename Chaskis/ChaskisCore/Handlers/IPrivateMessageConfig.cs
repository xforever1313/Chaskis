//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    /// <summary>
    /// Configuration that is common for all flavors of PRIVMSG.
    /// </summary>
    public interface IPrivateMessageConfig
    {
        /// <summary>
        /// The regex to search for in order to fire the action.
        /// For example, if you want !bot help to trigger the action, pass in "!bot\s+help"
        /// 
        /// This DOES get Liquified via <see cref="Parsing.LiquefyStringWithIrcConfig(string, string, string, string)'"/>
        /// </summary>
        string LineRegex { get; }

        /// <summary>
        /// What regex options to use with <see cref="LineRegex"/>.
        /// </summary>
        RegexOptions RegexOptions { get; }

        /// <summary>
        /// How long to wait in seconds between firing events. 0 for no cool down.
        /// This cool down is on a per-channel basis if the bot is in multiple channels.
        /// </summary>
        int CoolDown { get; }

        /// <summary>
        /// Whether or not this bot will respond to private messages or not.
        /// </summary>
        ResponseOptions ResponseOption { get; }

        /// <summary>
        /// Whether or not the action will be triggered if the person
        /// who sent the message was this bot.
        /// </summary>
        bool RespondToSelf { get; }
    }
}
