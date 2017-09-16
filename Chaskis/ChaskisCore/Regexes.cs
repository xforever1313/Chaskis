﻿//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace ChaskisCore
{
    /// <summary>
    /// Useful Regexes that can be used.
    /// </summary>
    public static class Regexes
    {
        // Per section 2.3 of RFC2812, an IRC message is the following:
        // message    =  [ ":" prefix SPACE ] command [ params ] crlf
        // and a prefix is this:
        // prefix     =  servername / ( nickname [ [ "!" user ] "@" host ] )
        // A prefix will have either the servername OR a nickname
        // A nickname can be just the nickname, or optionally contain
        // a host.  The Host can optionally have a user name.
        // The user name will be separated by the nick name via an '!'.
        //
        // The following strings are valid IRC prefixes:
        // - :someone[m]!someone@blah.org
        //   - This one can appear in a PRIVMSG from a user. The user has both a nickname and a user name.
        //     The host is also specified.
        // - :aserver.somewhere.net 
        //   - This can appear from a PONG or a PING.  Just the servername appears.
        // - :anickname!~ausername@192.168.2.1 
        //   - Notice the ~ in front of the user name.  This tilda is PART of the user name.
        //     According to this site: https://www.mirc.com/ircintro.html, a '~' in front of the user
        //     name means that the user is not identified with an Ident Server.
        // - :anickname
        //   - Personally, I've never seen just a nickname be used... but according to the BNF, it's valid.
        // - :anickanme@somewhere.net
        //   - I've never seen just a nickname and a host be used... but according to the BNF, it's valid.

        /// <summary>
        /// This is the prefix to an IRC message.
        /// Groups:
        ///     nickOrServer - The Nickname or server name... depending on the command.  Required.
        ///     username - The username of the user.  Optional.
        ///     host - The IP or address of the user.  Optional.
        /// </summary>
        public const string IrcMessagePrefix =
            @"^:(((?<nickOrServer>\S+)!(?<username>\S+)@(?<host>\S+))|" + // All three nick, user, and host.
            @"((?<nickOrServer>\S+)@(?<host>\S+))|"+ // Just nick and host.
            @"((?<nickOrServer>\S+)))"; // Just nick or server.

        /// <summary>
        /// Pattern that can be used to capture Chaskis
        /// IRC connect events.
        /// Groups:
        ///     server - the Server we connected to
        ///     nick - The user name we connected as.
        /// </summary>
        public const string ChaskisIrcConnectEvent =
            @"CONNECT\s+TO\s+(?<server>\S+)\s+AS\s+(?<nick>\S+)";

        /// <summary>
        /// Pattern that can be used to capture Chaskis
        /// IRC disconnect events.
        /// Groups:
        ///     server - the Server we disconnected from
        ///     nick - The user name we disconnected as.
        /// </summary>
        public const string ChaskisIrcDisconnectEvent =
            @"DISCONNECT\s+FROM\s+(?<server>\S+)\s+AS\s+(?<nick>\S+)";
    }
}
