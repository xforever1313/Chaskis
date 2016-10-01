//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

namespace GenericIrcBot
{
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
        /// Sends a raw command to the server.
        /// Only use if you REALLY know what you are doing.
        /// </summary>
        /// <param name="cmd">The IRC command to send.</param>
        void SendRawCmd( string command );
    }
}