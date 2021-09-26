//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Plugins.MetricsBot;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.MetricsBot
{
    [TestFixture]
    public sealed class DayOfWeekInfoKeyTests
    {
        [Test]
        public void EqualsTest()
        {
            DayOfWeekInfoKey uut1 = new DayOfWeekInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                DayOfWeek.Monday
            );

            DayOfWeekInfoKey uut2 = new DayOfWeekInfoKey(
                uut1.Protocol,
                uut1.Server,
                uut1.Channel,
                uut1.DayOfWeek
            );

            Assert.AreEqual( uut1, uut2 );
            Assert.AreEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        [Test]
        public void EqualsCapsTest()
        {
            DayOfWeekInfoKey uut1 = new DayOfWeekInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                DayOfWeek.Monday
            );

            DayOfWeekInfoKey uut2 = new DayOfWeekInfoKey(
                uut1.Protocol,
                uut1.Server.ToUpper(),
                uut1.Channel.ToUpper(),
                uut1.DayOfWeek
            );

            Assert.AreEqual( uut1, uut2 );
            Assert.AreEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        // Only 1 protocol right now, so can't test not equals if there's only one choice.
#if false
        [Test]
        public void EqualsProtocolNotEqualTest()
        {
            DayOfWeekInfoKey uut1 = new DayOfWeekInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                DayOfWeek.Monday
            );

            DayOfWeekInfoKey uut2 = new DayOfWeekInfoKey(
                uut1.Protocol,
                uut1.Server,
                uut1.Channel,
                uut1.DayOfWeek
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }
#endif

        [Test]
        public void EqualsServerNotEqualTest()
        {
            DayOfWeekInfoKey uut1 = new DayOfWeekInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                DayOfWeek.Monday
            );

            DayOfWeekInfoKey uut2 = new DayOfWeekInfoKey(
                uut1.Protocol,
                "irc.somewhereelse.net",
                uut1.Channel,
                uut1.DayOfWeek
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        [Test]
        public void EqualsChannelNotEqualTest()
        {
            DayOfWeekInfoKey uut1 = new DayOfWeekInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                DayOfWeek.Monday
            );

            DayOfWeekInfoKey uut2 = new DayOfWeekInfoKey(
                uut1.Protocol,
                uut1.Server,
                "#channel2",
                uut1.DayOfWeek
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        [Test]
        public void EqualsDayOfWeekNotEqualTest()
        {
            DayOfWeekInfoKey uut1 = new DayOfWeekInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#achannel",
                DayOfWeek.Monday
            );

            DayOfWeekInfoKey uut2 = new DayOfWeekInfoKey(
                uut1.Protocol,
                uut1.Server,
                uut1.Channel,
                DayOfWeek.Sunday
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }
    }
}
