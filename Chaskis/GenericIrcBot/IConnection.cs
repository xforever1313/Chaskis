
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

namespace GenericIrcBot
{
    /// <summary>
    /// Interface to an IRC connection.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Returns the IRCConfig.
        /// </summary>
        IIrcConfig Config { get; }

        /// <summary>
        /// Whether or not we are connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connects using the supplied settings.
        /// </summary>
        void Connect();

        /// <summary>
        /// Sends the given command to the channel.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        void SendCommand( string msg );

        /// <summary>
        /// Sends the given command to the user.  Also works for sending messages
        /// to other channels.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="userNick">The message to send.</param>
        void SendMessageToUser( string msg, string userNick );

        /// <summary>
        /// Sends a pong to the given url.
        /// </summary>
        /// <param name="response">The response to send.</param>
        void SendPong( string response );

        /// <summary>
        /// Disconnects the connection.
        /// </summary>
        void Disconnect();
    }
}
