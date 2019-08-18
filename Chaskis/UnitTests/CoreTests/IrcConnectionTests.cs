//
//          Copyright Seth Hendrick 2016-2019.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Chaskis.Core;
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

        private IrcConfig defaultConfig;

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

            this.defaultConfig = new IrcConfig
            {
                Nick = nick,
                QuitMessage = quitMessage,
                RealName = realName,
                UserName = userName,
                RateLimit = 0
            };

            defaultConfig.Channels.Add( channel1 );
            defaultConfig.Channels.Add( channel2 );
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
                this.DoConnect( connection );

                this.DoDisconnect( connection );
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

                this.DoConnect( connection );

                this.DoDisconnect( connection );
            }

            this.mac.VerifyAll();
        }

        /// <summary>
        /// Ensures that we try to connect without initing the class first,
        /// we get an Exception.
        /// </summary>
        [Test]
        public void ConnectWithoutInitTest()
        {
            using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                // Do not call connection.Init().
                Assert.Throws<InvalidOperationException>( () => connection.Connect() );
                Assert.DoesNotThrow( () => connection.Disconnect() ); // <- Should be a no-op since we are not connected.
            }
        }

        /// <summary>
        /// Ensures that if we try to connect twice, we get an exception.
        /// </summary>
        [Test]
        public void DoubleConnectTest()
        {
            using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                this.DoConnect( connection );

                Assert.Throws<InvalidOperationException>( () => connection.Connect() );

                this.DoDisconnect( connection );
            }
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

                this.DoConnect( connection );

                this.DoDisconnect( connection );
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

                this.DoConnect( connection );

                this.DoDisconnect( connection );
            }

            this.mac.VerifyAll();
        }

        /// <summary>
        /// Ensures our kick command string is formatted correctly if no
        /// reason is specified.
        /// </summary>
        [Test]
        public void SendKickCommandWithNoReasonTest()
        {
            // According to RFC2812, the kick command is:
            // TheChannel TheUser :reason

            using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                this.DoConnect( connection );

                this.mac.Setup(
                    m => m.WriteLine( "KICK " + channel1 + " " + userName )
                );

                connection.SendKick( userName, channel1 );

                this.DoDisconnect( connection );
            }

            this.mac.VerifyAll();
        }

        /// <summary>
        /// Ensures our kick command string is formatted correctly if we specify
        /// a reason.
        /// </summary>
        [Test]
        public void SendKickCommandWithReasonTest()
        {
            // According to RFC2812, the kick command is:
            // TheChannel TheUser :reason

            using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                this.DoConnect( connection );
                this.mac.Setup(
                    m => m.WriteLine( "KICK " + channel1 + " " + userName + " :Some Reason" )
                );

                connection.SendKick( userName, channel1, "Some Reason" );

                this.DoDisconnect( connection );
            }

            this.mac.VerifyAll();
        }

        /// <summary>
        /// Ensures our broadcast message helper works as expected.
        /// It should send the same message to all channels the bot is in.
        /// </summary>
        [Test]
        public void SendBroadcastMessageTest()
        {
            // According to RFC2812, sending a message's syntax is:
            // PRIVMSG target :message

            using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                const string message = "My Message";

                foreach( string channel in this.defaultConfig.Channels )
                {
                    this.mac.Setup(
                        m => m.WriteLine( $"PRIVMSG {channel} :{message}" )
                    );
                }

                this.DoConnect( connection );
                connection.SendBroadcastMessage( message );
                this.DoDisconnect( connection );
            }

            this.mac.VerifyAll();
        }

        /// <summary>
        /// Ensures that if we are sending a message, the format
        /// is correct.
        /// </summary>
        [Test]
        public void SendMessageTest()
        {
            // According to RFC2812, sending a message's syntax is:
            // PRIVMSG target :message

            using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                const string channel = "seth"; // "Channel" can be a username.
                const string message = "My Message";

                this.mac.Setup(
                    m => m.WriteLine( $"PRIVMSG {channel} :{message}" )
                );

                this.DoConnect( connection );
                connection.SendMessage( message, channel );
                this.DoDisconnect( connection );
            }

            this.mac.VerifyAll();
        }

        [Test]
        public void SendLongMessageTest()
        {
            StringBuilder msg1Builder = new StringBuilder();
            for( int i = 0; i < IrcConnection.MaximumLength; ++i )
            {
                msg1Builder.Append( "m" );
            }

            StringBuilder msg2Builder = new StringBuilder();
            for( int i = 0; i < IrcConnection.MaximumLength - 1; ++i )
            {
                msg2Builder.Append( "s" );
            }

            string message1 = msg1Builder.ToString();
            string message2 = msg2Builder.ToString();

            // Sanity check.
            Assert.AreEqual( IrcConnection.MaximumLength, msg1Builder.Length );
            Assert.AreEqual( IrcConnection.MaximumLength - 1, msg2Builder.Length );

            string fullMessage = message1 + message2;

            // For this test, we will send the full message; we expect it to be split twice.
            // The first message should end in a <more> to specifiy that the message is not completed yet.
            using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                const string channel = channel1;

                this.mac.Setup(
                    m => m.WriteLine( $"PRIVMSG {channel} :{message1} <more>" )
                );

                this.mac.Setup(
                    m => m.WriteLine( $"PRIVMSG {channel} :{message2}" )
                );

                this.DoConnect( connection );
                connection.SendMessage( fullMessage, channel );
                this.DoDisconnect( connection );
            }

            this.mac.VerifyAll();
        }

        [Test]
        public void SendPingTest()
        {
            // According to RFC2812, sending a PING message
            // looks like:
            // PING TheMessage
            // Note that there is no ':' before the message when sending a ping,
            // that's only when receiving a ping.

            using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                const string message = "SomePing";

                this.mac.Setup(
                    m => m.WriteLine( "PING " + message )
                );

                this.DoConnect( connection );
                connection.SendPing( message );
                this.DoDisconnect( connection );
            }

            this.mac.VerifyAll();
        }

        [Test]
        public void SendPongTest()
        {
            using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
            {
                const string message = "SomePong";

                this.mac.Setup(
                    m => m.WriteLine( "PONG :" + message )
                );

                this.DoConnect( connection );
                connection.SendPong( message );
                this.DoDisconnect( connection );
            }

            this.mac.VerifyAll();
        }

        /// <summary>
        /// Ensures the reader thread will exit correctly when a <see cref="SocketException"/> happens.
        /// </summary>
        [Test]
        public void ReaderThreadSocketException()
        {
            this.DoReaderThreadFailureTest( new SocketException() );
        }

        /// <summary>
        /// Ensures the reader thread will exit correctly when a <see cref="IOException"/> happens.
        /// </summary>
        [Test]
        public void ReaderThreadIoException()
        {
            this.DoReaderThreadFailureTest( new IOException() );
        }

        /// <summary>
        /// Ensures the reader thread will exit correctly when an <see cref="ObjectDisposedException"/> happens.
        /// </summary>
        [Test]
        public void ReaderThreadObjectDisposedException()
        {
            this.DoReaderThreadFailureTest( new ObjectDisposedException( "someobject" ) );
        }

        /// <summary>
        /// Ensures the reader thread will exit correctly when an <see cref="AggregateException"/> happens.
        /// </summary>
        [Test]
        public void ReaderThreadAggregateException()
        {
            this.DoReaderThreadFailureTest( new AggregateException() );
        }

        /// <summary>
        /// Ensures the reader thread will exit correctly when any <see cref="Exception"/> happens.
        /// </summary>
        [Test]
        public void ReaderThreadException()
        {
            this.DoReaderThreadFailureTest( new Exception( "lol" ) );
        }

        // ---------------- Test Helpers ----------------

        /// <summary>
        /// This test tests issue that caused issue #29.
        /// If we catch an Exception we weren't expecting, the reader thread would
        /// blow up and consistently throw exceptions.
        /// 
        /// Now if we get any Exception, we gracefully exit the reader thread and wait
        /// for the watchdog to reconnect us.  This test case simply tests that the
        /// reader thread terminates.
        /// </summary>
        private void DoReaderThreadFailureTest<T>( T e ) where T : Exception
        {
            const string line1 = "Line 1";

            this.mac.SetupSequence(
                m => m.ReadLine()
            )
            .Returns( line1 )
            .Throws( e );

            StringBuilder messages = new StringBuilder();
            using( ManualResetEvent readerThreadExitedEvent = new ManualResetEvent( false ) )
            {
                using( IrcConnection connection = new IrcConnection( this.defaultConfig, this.parsingQueue.Object, this.mac.Object ) )
                {
                    void threadExitedAction() { readerThreadExitedEvent.Set(); }
                    void onReadLineAction( string s ) { messages.AppendLine( s ); }

                    try
                    {
                        connection.OnReaderThreadExit += threadExitedAction;
                        connection.ReadEvent += onReadLineAction;

                        this.DoConnect( connection );

                        Assert.IsTrue( readerThreadExitedEvent.WaitOne( 5000 ) );

                        this.DoDisconnect( connection );
                    }
                    finally
                    {
                        connection.OnReaderThreadExit -= threadExitedAction;
                        connection.ReadEvent -= onReadLineAction;
                    }
                }
            }

            // Ensure our line gets called *somewhere*.
            Assert.IsTrue( Regex.IsMatch( messages.ToString(), @"\s*" + line1 + @"\s*" ) );
            this.mac.VerifyAll();
        }

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

        private void DoConnect( IrcConnection connection )
        {
            this.SetupConnection();
            connection.Init();
            connection.Connect();

            Assert.IsTrue( connection.IsConnected );
            Assert.IsTrue( this.mac.Object.IsConnected );
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

        private void DoDisconnect( IrcConnection connection )
        {
            this.SetupDisconnection();
            connection.Disconnect();
            Assert.IsFalse( connection.IsConnected );
            Assert.IsFalse( this.mac.Object.IsConnected );
        }
    }
}
