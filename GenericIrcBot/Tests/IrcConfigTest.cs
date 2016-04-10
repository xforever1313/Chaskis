
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using GenericIrcBot;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Tests
{
    [TestFixture]
    public class IrcConfigTest
    {
        // -------- Fields --------

        /// <summary>
        /// Irc config under test.
        /// </summary>
        private IrcConfig ircConfig;

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures the Read-only irc config does its job.
        /// </summary>
        [Test]
        public void ReadOnlyTest()
        {
            IIrcConfig config = new ReadOnlyIrcConfig( ircConfig );

            // First, ensure properties are all equal
            Assert.AreEqual( this.ircConfig.Server, config.Server );
            Assert.AreEqual( this.ircConfig.Channel, config.Channel );
            Assert.AreEqual( this.ircConfig.Port, config.Port );
            Assert.AreEqual( this.ircConfig.UserName, config.UserName );
            Assert.AreEqual( this.ircConfig.Nick, config.Nick );
            Assert.AreEqual( this.ircConfig.RealName, config.RealName );
            Assert.AreEqual( this.ircConfig.Password, config.Password );

            // Next, ensure trying to convert to an IRCConfig results in a null (someone's going to do this).
            Assert.IsNull( config as IrcConfig );

            // Lastly, ensure setters throw exceptions.
            ReadOnlyIrcConfig roConfig = config as ReadOnlyIrcConfig;

            ReadOnlyException ex =
                Assert.Throws<ReadOnlyException>(
                    delegate ()
                    {
                        roConfig.Server = "NewServer";
                    }
                );
            Assert.IsTrue( ex.Message.Contains( nameof( roConfig.Server ) ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.Channel = "#NewChannel";
                }
            );
            Assert.IsTrue( ex.Message.Contains( nameof( roConfig.Channel ) ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.Port = 4;
                }
            );
            Assert.IsTrue( ex.Message.Contains( nameof( roConfig.Port ) ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.UserName = "NewName";
                }
            );
            Assert.IsTrue( ex.Message.Contains( nameof( roConfig.UserName ) ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.Nick = "NewNick";
                }
            );
            Assert.IsTrue( ex.Message.Contains( nameof( roConfig.Nick ) ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.RealName = "NewRName";
                }
            );
            Assert.IsTrue( ex.Message.Contains( nameof( roConfig.RealName ) ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.Password = "NewPass";
                }
            );
            Assert.IsTrue( ex.Message.Contains( nameof( roConfig.Password ) ) );
        }
    }
}
