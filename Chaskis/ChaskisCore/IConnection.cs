//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace ChaskisCore
{
    /// <summary>
    /// Interface to an IRC connection.
    /// </summary>
    public interface IConnection : IIrcWriter
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
        /// Inits the class.
        /// </summary>
        void Init();

        /// <summary>
        /// Connects using the supplied settings.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects the connection.
        /// </summary>
        void Disconnect();
    }
}