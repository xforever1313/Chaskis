//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Plugins.MetricsBot;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.MetricsBot
{
    [TestFixture]
    public class HourOfDayInfoKeyTests
    {
        [Test]
        public void EqualsTest()
        {
            HourOfDayInfoKey uut1 = new HourOfDayInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                1
            );

            HourOfDayInfoKey uut2 = new HourOfDayInfoKey(
                uut1.Protocol,
                uut1.Server,
                uut1.Channel,
                uut1.HourOfDay
            );

            Assert.AreEqual( uut1, uut2 );
            Assert.AreEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        [Test]
        public void EqualsCapsTest()
        {
            HourOfDayInfoKey uut1 = new HourOfDayInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                0
            );

            HourOfDayInfoKey uut2 = new HourOfDayInfoKey(
                uut1.Protocol,
                uut1.Server.ToUpper(),
                uut1.Channel.ToUpper(),
                uut1.HourOfDay
            );

            Assert.AreEqual( uut1, uut2 );
            Assert.AreEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        // Only 1 protocol right now, so can't test not equals if there's only one choice.
#if false
        [Test]
        public void EqualsProtocolNotEqualTest()
        {
            HourOfDayInfoKey uut1 = new HourOfDayInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                1
            );

            HourOfDayInfoKey uut2 = new HourOfDayInfoKey(
                uut1.Protocol,
                uut1.Server,
                uut1.Channel,
                uut1.HourOfDay
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }
#endif

        [Test]
        public void EqualsServerNotEqualTest()
        {
            HourOfDayInfoKey uut1 = new HourOfDayInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                2
            );

            HourOfDayInfoKey uut2 = new HourOfDayInfoKey(
                uut1.Protocol,
                "irc.somewhereelse.net",
                uut1.Channel,
                uut1.HourOfDay
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        [Test]
        public void EqualsChannelNotEqualTest()
        {
            HourOfDayInfoKey uut1 = new HourOfDayInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                3
            );

            HourOfDayInfoKey uut2 = new HourOfDayInfoKey(
                uut1.Protocol,
                uut1.Server,
                "#channel2",
                uut1.HourOfDay
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        [Test]
        public void EqualsDayOfWeekNotEqualTest()
        {
            HourOfDayInfoKey uut1 = new HourOfDayInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                0
            );

            HourOfDayInfoKey uut2 = new HourOfDayInfoKey(
                uut1.Protocol,
                uut1.Server,
                uut1.Channel,
                uut1.HourOfDay + 1
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }
    }
}
