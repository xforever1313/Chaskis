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
    public sealed class MessageInfoKeyTests
    {
        [Test]
        public void EqualsTest()
        {
            MessageInfoKey uut1 = new MessageInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#somechannel",
                "someuser",
                MessageType.PrivMsg
            );

            MessageInfoKey uut2 = new MessageInfoKey(
                uut1.Protocol,
                uut1.Server,
                uut1.Channel,
                uut1.IrcUser,
                uut1.MessageType
            );

            Assert.AreEqual( uut1, uut2 );
            Assert.AreEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        /// <summary>
        /// Caps should not matter for a key.
        /// </summary>
        [Test]
        public void EqualsCapsTest()
        {
            MessageInfoKey uut1 = new MessageInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#somechannel",
                "someuser",
                MessageType.PrivMsg
            );

            MessageInfoKey uut2 = new MessageInfoKey(
                uut1.Protocol,
                uut1.Server.ToUpper(),
                uut1.Channel.ToUpper(),
                uut1.IrcUser.ToUpper(),
                uut1.MessageType
            );

            Assert.AreEqual( uut1, uut2 );
            Assert.AreEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        // Only 1 protocol right now, so can't test not equals if there's only one choice.
#if false
        [Test]
        public void EqualsProtocolNotEqualTest()
        {
            MessageInfoKey uut1 = new MessageInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#somechannel",
                "someuser",
                MessageType.PrivMsg
            );

            MessageInfoKey uut2 = new MessageInfoKey(
                uut1.Protocol,
                uut1.Server.ToUpper(),
                uut1.Channel.ToUpper(),
                uut1.IrcUser.ToUpper(),
                uut1.MessageType
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }
#endif

        [Test]
        public void EqualsServerNotEqualTest()
        {
            MessageInfoKey uut1 = new MessageInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#somechannel",
                "someuser",
                MessageType.PrivMsg
            );

            MessageInfoKey uut2 = new MessageInfoKey(
                uut1.Protocol,
                "irc.somewhereelse.net",
                uut1.Channel,
                uut1.IrcUser,
                uut1.MessageType
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        [Test]
        public void EqualsChannelNotEqualTest()
        {
            MessageInfoKey uut1 = new MessageInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#somechannel",
                "someuser",
                MessageType.PrivMsg
            );

            MessageInfoKey uut2 = new MessageInfoKey(
                uut1.Protocol,
                uut1.Server,
                "#someotherchannel",
                uut1.IrcUser,
                uut1.MessageType
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        [Test]
        public void EqualsUserNotEqualTest()
        {
            MessageInfoKey uut1 = new MessageInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#somechannel",
                "someuser",
                MessageType.PrivMsg
            );

            MessageInfoKey uut2 = new MessageInfoKey(
                uut1.Protocol,
                uut1.Server,
                uut1.Channel,
                "someonelese",
                uut1.MessageType
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }

        [Test]
        public void MessageTypeNotEqualTest()
        {
            MessageInfoKey uut1 = new MessageInfoKey(
                Protocol.IRC,
                "irc.somewhere.net",
                "#somechannel",
                "someuser",
                MessageType.PrivMsg
            );

            MessageInfoKey uut2 = new MessageInfoKey(
                uut1.Protocol,
                uut1.Server,
                uut1.Channel,
                uut1.IrcUser,
                MessageType.Kick
            );

            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
        }
    }
}
