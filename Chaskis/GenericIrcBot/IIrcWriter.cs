//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace ChaskisCore
{
    /// <summary>
    /// Interface on how to write to an IRC channel.
    /// </summary>
    public interface IIrcWriter
    {
        /// <summary>
        /// Sends the given command to channel the bot is listening on.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        void SendCommandToChannel( string msg );

        /// <summary>
        /// Sends the given command to the user.  Also works for sending messages
        /// to other channels.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="userNick">The user (or #channel) to send the message to.</param>
        void SendMessageToUser( string msg, string userNick );

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
        /// Sends a raw command to the server.
        /// Only use if you REALLY know what you are doing.
        /// </summary>
        /// <param name="cmd">The IRC command to send.</param>
        void SendRawCmd( string command );

        /// <summary>
        /// Sends a part to the current channel we are on.
        /// Note, this will make the bot LEAVE the channel.  Only use
        /// if you know what you are doing.
        /// </summary>
        /// <param name="reason">The reason for parting.</param>
        void SendPart( string reason );
    }
}