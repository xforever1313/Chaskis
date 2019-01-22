//
//          Copyright Seth Hendrick 2016-2019.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using Chaskis.UnitTests.Common;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.CoreTests
{
    [TestFixture]
    public class IrcConnectionTests
    {
        // ---------------- Fields ----------------

        private const string channel1 = "#channel1";
        private const string channel2 = "#channel2";
        private const string nick = "bot";
        private const string nickServPass = "nickservpass";
        private const string realName = "realbot";
        private const string serverPassword = "serverpass";
        private const string quitMessage = "I quit!";
        private const string userName = "botuser";

        private Mock<IIrcMac> mac;
        private Mock<INonDisposableStringParsingQueue> parsingQueue;

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
        }

        [OneTimeTearDown]
        public void FixtureTeardown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
            this.mac = new Mock<IIrcMac>( MockBehavior.Strict );
            this.parsingQueue = new Mock<INonDisposableStringParsingQueue>( MockBehavior.Strict );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures if there is no password specfied, we don't send the server
        /// password
        /// </summary>
        [Test]
        public void ConnectWithNoPasswordTest()
        {
            IrcConfig ircConfig = new IrcConfig
            {
                Nick = nick,
                QuitMessage = quitMessage,
                RealName = realName,
                UserName = userName,
                RateLimit = 0
            };

            ircConfig.Channels.Add( channel1 );
            ircConfig.Channels.Add( channel2 );

            using( IrcConnection connection = new IrcConnection( ircConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                this.SetupConnection();
                connection.Init();
                connection.Connect();
                Assert.IsTrue( connection.IsConnected );
                Assert.IsTrue( this.mac.Object.IsConnected );

                this.SetupDisconnection();
                connection.Disconnect();
                Assert.IsFalse( connection.IsConnected );
                Assert.IsFalse( this.mac.Object.IsConnected );
            }

            this.mac.VerifyAll();
        }

        /// <summary>
        /// Ensures if there is a server but no NickServ password specified,
        /// we send the password.
        /// password
        /// </summary>
        [Test]
        public void ConnectWithServerPasswordTest()
        {
            IrcConfig ircConfig = new IrcConfig
            {
                Nick = nick,
                QuitMessage = quitMessage,
                RealName = realName,
                ServerPassword = serverPassword,
                UserName = userName,
                RateLimit = 0
            };

            ircConfig.Channels.Add( channel1 );
            ircConfig.Channels.Add( channel2 );

            using( IrcConnection connection = new IrcConnection( ircConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                // We need to send the server password too!
                this.mac.Setup(
                    m => m.WriteLine( "PASS {0}", serverPassword )
                );

                this.SetupConnection();

                connection.Init();
                connection.Connect();
                Assert.IsTrue( connection.IsConnected );
                Assert.IsTrue( this.mac.Object.IsConnected );

                this.SetupDisconnection();
                connection.Disconnect();
                Assert.IsFalse( connection.IsConnected );
                Assert.IsFalse( this.mac.Object.IsConnected );
            }

            this.mac.VerifyAll();
        }

        /// <summary>
        /// Ensures if there is a NickServ but no Server password specified,
        /// we send the correct passwords.
        /// </summary>
        [Test]
        public void ConnectWithNickServPasswordTest()
        {
            IrcConfig ircConfig = new IrcConfig
            {
                Nick = nick,
                NickServPassword = nickServPass,
                QuitMessage = quitMessage,
                RealName = realName,
                UserName = userName,
                RateLimit = 0
            };

            ircConfig.Channels.Add( channel1 );
            ircConfig.Channels.Add( channel2 );

            using( IrcConnection connection = new IrcConnection( ircConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                // We need to send the nickserv password too!
                this.mac.Setup(
                    m => m.WriteLine( "PRIVMSG NickServ :IDENTIFY {0}", nickServPass )
                );

                this.SetupConnection();

                connection.Init();
                connection.Connect();
                Assert.IsTrue( connection.IsConnected );
                Assert.IsTrue( this.mac.Object.IsConnected );

                this.SetupDisconnection();
                connection.Disconnect();
                Assert.IsFalse( connection.IsConnected );
                Assert.IsFalse( this.mac.Object.IsConnected );
            }

            this.mac.VerifyAll();
        }

        /// <summary>
        /// Ensures if there is a server AND a NickServ password specified,
        /// we send both.
        /// </summary>
        [Test]
        public void ConnectWithNickServAndServerPasswordTest()
        {
            IrcConfig ircConfig = new IrcConfig
            {
                Nick = nick,
                NickServPassword = nickServPass,
                QuitMessage = quitMessage,
                RealName = realName,
                ServerPassword = serverPassword,
                UserName = userName,
                RateLimit = 0
            };

            ircConfig.Channels.Add( channel1 );
            ircConfig.Channels.Add( channel2 );

            using( IrcConnection connection = new IrcConnection( ircConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                // We need to send the passwords too!
                this.mac.Setup(
                    m => m.WriteLine( "PRIVMSG NickServ :IDENTIFY {0}", nickServPass )
                );

                this.mac.Setup(
                    m => m.WriteLine( "PASS {0}", serverPassword )
                );

                this.SetupConnection();

                connection.Init();
                connection.Connect();
                Assert.IsTrue( connection.IsConnected );
                Assert.IsTrue( this.mac.Object.IsConnected );

                this.SetupDisconnection();
                connection.Disconnect();
                Assert.IsFalse( connection.IsConnected );
                Assert.IsFalse( this.mac.Object.IsConnected );
            }

            this.mac.VerifyAll();
        }

        // ---------------- Tests ----------------

        private void SetupConnection()
        {
            this.mac.Setup(
                m => m.Connect()
            ).Callback(
                () =>
                {
                    mac.Setup(
                        m => m.IsConnected
                    ).Returns( true );
                }
            );

            // We should send the user name...
            this.mac.Setup(
                m => m.WriteLine( "USER {0} 0 * :{1}", userName, realName )
            );

            // Then the nick name...
            this.mac.Setup(
                m => m.WriteLine( "NICK {0}", nick )
            );

            // Finally, join channels.
            this.mac.Setup(
                m => m.WriteLine( "JOIN {0}", channel1 )
            );

            this.mac.Setup(
                m => m.WriteLine( "JOIN {0}", channel2 )
            );
        }

        private void SetupDisconnection()
        {
            this.mac.Setup(
                m => m.Disconnect()
            ).Callback(
                () =>
                {
                    mac.Setup(
                        m => m.IsConnected
                    ).Returns( false );
                }
            );

            this.mac.Setup(
                m => m.Dispose()
            );
        }
    }
}
