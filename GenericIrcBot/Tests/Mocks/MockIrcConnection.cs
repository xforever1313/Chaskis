
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.Collections.Generic;
using GenericIrcBot;

namespace Tests.Mocks
{
    /// <summary>
    /// Class that mocks an IRC Connection.
    /// </summary>
    public class MockIrcConnection : IConnection
    {
        // -------- Fields --------

        /// <summary>
        /// Messages sent.
        /// Key is the user, list is the messages sent to the user.
        /// </summary>
        Dictionary<string, IList<string>> messagesSend;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.  Fills everything with default settings.
        /// </summary>
        public MockIrcConnection( IIrcConfig config )
        {
            Reset();
            this.Config = config;
        }

        // -------- Properties --------

        /// <summary>
        /// Whether or not we are connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Dictionary of messages sent using SendCommand
        /// </summary>
        public IReadOnlyDictionary<string, IList<string>> MessagesSent { get; set; }

        /// <summary>
        /// The IRC config to use.
        /// </summary>
        public IIrcConfig Config { get; set; }

        /// <summary>
        /// The pong response from SendPong().
        /// </summary>
        public string PongResponse { get; private set; }

        // -------- Functions --------

        /// <summary>
        /// "Connects"
        /// </summary>
        public void Connect()
        {
            this.IsConnected = true;
        }

        /// <summary>
        /// Sends the given command to the testChannel specified in the config.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        public void SendCommand( string msg )
        {
            if( messagesSend.ContainsKey( this.Config.Channel ) == false )
            {
                messagesSend[this.Config.Channel] = new List<string>();
            }

            messagesSend[this.Config.Channel].Add( msg );
        }

        /// <summary>
        /// Sends the given command to the user.  Also works for sending messages
        /// to other channels.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="userNick">The message to send.</param>
        public void SendMessageToUser( string msg, string userNick )
        {
            if( messagesSend.ContainsKey( userNick ) == false )
            {
                messagesSend[userNick] = new List<string>();
            }

            messagesSend[userNick].Add( msg );
        }

        /// <summary>
        /// Sends the pong to the give url.
        /// Actually saves it to this.PongUrl.
        /// </summary>
        /// <param name="response">The response to we send with the pong.</param>
        public void SendPong( string response )
        {
            this.PongResponse = response;
        }

        /// <summary>
        /// Disconnects the "connection".
        /// </summary>
        public void Disconnect()
        {
            this.IsConnected = false;
        }

        /// <summary>
        /// Resets the state of this class.
        /// Does not reset Config.
        /// </summary>
        public void Reset()
        {
            messagesSend = new Dictionary<string, IList<string>>();
            this.MessagesSent = messagesSend;
            this.IsConnected = false;
            this.PongResponse = string.Empty;
        }
    }
}
