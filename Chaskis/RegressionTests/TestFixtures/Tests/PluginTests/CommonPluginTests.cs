//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.RegressionTests.TestCore;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    public static class CommonPluginTests
    {
        public static void DoPluginLoadTest( ChaskisTestFramework testFrame, string expectedPluginName )
        {
            testFrame.IrcServer.SendMessageToChannelAs(
                $"!{TestConstants.BotName} plugins",
                TestConstants.Channel1,
                TestConstants.NormalUser
            );


            testFrame.IrcServer.WaitForMessageOnChannel(
                @"List\s+of\s+plugins\s+I\s+am\s+running:\s+.*" + expectedPluginName + ".*",
                TestConstants.Channel1
            ).FailIfFalse( $"Did not find '{expectedPluginName}' in list of plugins" );
        }
    }
}
