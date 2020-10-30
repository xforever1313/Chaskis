//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.RegressionTests.TestCore
{
    public static class CommonTests
    {
        public static void WaitForClientToConnect( this ChaskisProcess processRunner )
        {
            processRunner.WaitForStringFromChaskis(
                @"<chaskis_connect_event><server>(?<server>\S+)</server><protocol>IRC</protocol></chaskis_connect_event>"
            ).FailIfFalse( "Did not connected event" );
        }

        public static void WaitToFinishJoiningChannels( this ChaskisProcess processRunner )
        {
            processRunner.WaitForStringFromChaskis(
                @"<chaskis_finishedjoiningchannels_event><server>(?<server>\S+)</server><protocol>IRC</protocol></chaskis_finishedjoiningchannels_event>"
            ).FailIfFalse( "Did not get joined channel event" );
        }

        /// <summary>
        /// Ensures the connection is still alive.
        /// </summary>
        public static void CanaryTest( this ChaskisTestFramework testFrame )
        {
            testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                "!chaskistest canary",
                TestConstants.NormalUser,
                TestConstants.Channel1,
                @"Canary\s+Alive!"
            ).FailIfFalse( "Did not get canary" );
        }
    }
}
