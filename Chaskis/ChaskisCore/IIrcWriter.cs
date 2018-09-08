//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Interface on how to write to an IRC channel.
    /// </summary>
    public interface IIrcWriter
    {
        /// <summary>
        /// Sends the given message to ALL channels this bot is listening on.
        /// </summary>
        void SendBroadcastMessage( string msg );

        /// <summary>
        /// Sends the given command to the user.  Also works for sending messages
        /// to other channels.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="userNick">The user (or #channel) to send the message to.</param>
        void SendMessage( string msg, string userNick );

        /// <summary>
        /// Sends a pong using the given response.
        /// </summary>
        /// <param name="response">The response we need to send.</param>
        void SendPong( string response );

        /// <summary>
        /// Sends a ping to the server so if we are still connected.
        /// </summary>
        /// <param name="msg">The message to ping the server with.</param>
        void SendPing( string msg );

        /// <summary>
        /// Call when we receive a pong from the server.
        /// </summary>
        /// <param name="response">The response from the server.</param>
        void ReceivedPong( string response );

        /// <summary>
        /// Sends a part to the given channel.
        /// Note, this will make the bot LEAVE the channel.  Only use
        /// if you know what you are doing.
        /// </summary>
        /// <param name="reason">The reason for parting.</param>
        /// <param name="channel">The channel to leave.</param>
        void SendPart( string reason, string channel );

        /// <summary>
        /// Sends a kick command that kicks the specified user from the given channel.
        /// Note, your bot must actually have power in the channel for this to actually do anything.
        /// </summary>
        /// <param name="userToKick">The user to kick in the channel.</param>
        /// <param name="channel">The channel to kick the user from.</param>
        /// <param name="reason">The reason for kicking.  Leave null for the default message.</param>
        void SendKick( string userToKick, string channel, string reason = null );

        /// <summary>
        /// Sends a raw command to the server.
        /// Only use if you REALLY know what you are doing.
        /// </summary>
        /// <param name="cmd">The IRC command to send.</param>
        void SendRawCmd( string command );
    }
}