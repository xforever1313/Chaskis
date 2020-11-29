//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.RegressionTests.TestCore
{
    /// <summary>
    /// In the fixture setup, how long should we wait for?
    /// </summary>
    public enum ConnectionWaitMode
    {
        /// <summary>
        /// Do not wait for a connection status.
        /// </summary>
        DoNotWait,

        /// <summary>
        /// Wait until we get a connection status, do not wait to join channels.
        /// </summary>
        WaitForConnected,

        /// <summary>
        /// Wait until we finish joining all channels.
        /// </summary>
        WaitForFinishJoiningChannels,

        /// <summary>
        /// We should expect the server to NOT connect to anything,
        /// as the process may purposefully not connect.
        /// </summary>
        ExpectNoConnection
    }

    public class ChaskisFixtureConfig
    {
        // ---------------- Constructor ----------------

        public ChaskisFixtureConfig()
        {
            this.Environment = null;
            this.Port = 10123;
            this.ConnectionWaitMode = ConnectionWaitMode.WaitForFinishJoiningChannels;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The environment to use during testing.
        /// Set to null to use the default environment.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Port used to talk between the server and the client.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// How to know when we are done connecting.  Defaulted to <seealso cref="ConnectionWaitMode.WaitForFinishJoiningChannels"/>
        /// </summary>
        public ConnectionWaitMode ConnectionWaitMode { get; set; }
    }
}
