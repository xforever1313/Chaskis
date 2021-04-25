//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Threading;
using Chaskis.Plugins.MetricsBot;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.MetricsBot
{
    [TestFixture]
    public class MetricsBotDatabaseTests
    {
        // ---------------- Fields ----------------

        private const string dbName = "metricsbottest.ldb";

        private static readonly string dbPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            dbName
        );

        /// <summary>
        /// So one test runs at a time.
        /// </summary>
        private static readonly Mutex mutex = new Mutex();

        private MetricsBotDatabase uut;

        private static readonly MessageInfoKey defaultKey1 = new MessageInfoKey(
            channel: "#somechannel",
            ircUser:  "someuser",
            messageType : MessageType.PrivMsg,
            protocol: Protocol.IRC,
            server: "irc.somewhere.net"
        );

        private static readonly MessageInfoKey defaultKey2 = new MessageInfoKey(
            channel: "#somechannel2",
            ircUser: "someuser",
            messageType: MessageType.PrivMsg,
            protocol: Protocol.IRC,
            server: "irc.somewhere.net"
        );

        private static readonly MessageInfoKey defaultKey1AllCaps = new MessageInfoKey(
            channel: defaultKey1.Channel.ToUpper(),
            ircUser:  defaultKey1.IrcUser.ToUpper(),
            messageType : MessageType.PrivMsg,
            protocol: Protocol.IRC,
            server: defaultKey1.Server.ToUpper()
        );

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            Assert.IsTrue( mutex.WaitOne( 60 * 1000 ) );

            DeleteDb();
            this.uut = new MetricsBotDatabase( dbPath );
        }

        [TearDown]
        public void TestTeardown()
        {
            try
            {
                this.uut.Dispose();
                this.DeleteDb();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private void DeleteDb()
        {
            if( File.Exists( dbPath ) )
            {
                File.Delete( dbPath );
            }
        }

        // ---------------- Tests ----------------

        [Test]
        public void AddTest()
        {
            // Nothing should be there are the start.
            Assert.IsNull( this.uut.GetInfo( defaultKey1 ) );

            // Add one message, it should be inserted.
            {
                this.uut.AddNewMessage( defaultKey1 );
                this.uut.WriteCacheToDatabase();
                this.uut.UpdateCacheFromDatabase();

                MessageInfo foundInfo = this.uut.GetInfo( defaultKey1 );
                Assert.IsNotNull( foundInfo );
                Assert.AreEqual( defaultKey1, foundInfo.Id );
                Assert.AreEqual( 1, foundInfo.Count );
            }

            // Add another message, it should be updated instead of inserted.
            {
                this.uut.AddNewMessage( defaultKey1 );
                this.uut.WriteCacheToDatabase();
                this.uut.UpdateCacheFromDatabase();

                MessageInfo foundInfo = this.uut.GetInfo( defaultKey1 );
                Assert.IsNotNull( foundInfo );
                Assert.AreEqual( defaultKey1, foundInfo.Id );
                Assert.AreEqual( 2, foundInfo.Count );
            }
        }

        [Test]
        public void AddWithCapsTest()
        {
            // Nothing should be there are the start.
            Assert.IsNull( this.uut.GetInfo( defaultKey1 ) );

            // Add one message, it should be inserted.
            {
                this.uut.AddNewMessage( defaultKey1 );
                this.uut.WriteCacheToDatabase();
                this.uut.UpdateCacheFromDatabase();

                MessageInfo foundInfo = this.uut.GetInfo( defaultKey1 );
                Assert.IsNotNull( foundInfo );
                Assert.AreEqual( defaultKey1, foundInfo.Id );
                Assert.AreEqual( 1, foundInfo.Count );
            }

            // Add another message, it should be updated instead of inserted,
            // even if everything is in all caps (casing should not matter)
            {
                this.uut.AddNewMessage( defaultKey1AllCaps );
                this.uut.WriteCacheToDatabase();
                this.uut.UpdateCacheFromDatabase();

                MessageInfo foundInfo = this.uut.GetInfo( defaultKey1AllCaps );
                Assert.IsNotNull( foundInfo );
                Assert.AreEqual( defaultKey1AllCaps, foundInfo.Id );
                Assert.AreEqual( 2, foundInfo.Count );
            }
        }

        [Test]
        public void AddTwoDifferentKeysTest()
        {
            // Nothing should be there are the start.
            Assert.IsNull( this.uut.GetInfo( defaultKey1 ) );
            Assert.IsNull( this.uut.GetInfo( defaultKey2 ) );

            // Add one message, it should be inserted.
            {
                this.uut.AddNewMessage( defaultKey1 );
                this.uut.WriteCacheToDatabase();
                this.uut.UpdateCacheFromDatabase();

                MessageInfo foundInfo = this.uut.GetInfo( defaultKey1 );
                Assert.IsNotNull( foundInfo );
                Assert.AreEqual( defaultKey1, foundInfo.Id );
                Assert.AreEqual( 1, foundInfo.Count );

                Assert.IsNull( this.uut.GetInfo( defaultKey2 ) );
            }

            // Add another message that is a different key,
            // old key should not be altered.
            {
                this.uut.AddNewMessage( defaultKey2 );
                this.uut.WriteCacheToDatabase();
                this.uut.UpdateCacheFromDatabase();

                MessageInfo foundInfo1 = this.uut.GetInfo( defaultKey1 );
                Assert.IsNotNull( foundInfo1 );
                Assert.AreEqual( defaultKey1, foundInfo1.Id );
                Assert.AreEqual( 1, foundInfo1.Count );

                MessageInfo foundInfo2 = this.uut.GetInfo( defaultKey2 );
                Assert.IsNotNull( foundInfo2 );
                Assert.AreEqual( defaultKey2, foundInfo2.Id );
                Assert.AreEqual( 1, foundInfo2.Count );
            }
        }

        /// <summary>
        /// Ensures if we load from the database BEFORE writing cache out,
        /// our cache gets reset.
        /// 
        /// Tests to make sure cache is *actually* getting written to.
        /// </summary>
        [Test]
        public void CacheOverrideTest()
        {
            // Nothing should be there are the start.
            Assert.IsNull( this.uut.GetInfo( defaultKey1 ) );

            // Add one message, it should be inserted.
            {
                this.uut.AddNewMessage( defaultKey1 );

                MessageInfo foundInfo = this.uut.GetInfo( defaultKey1 );
                Assert.IsNotNull( foundInfo );
                Assert.AreEqual( defaultKey1, foundInfo.Id );
                Assert.AreEqual( 1, foundInfo.Count );
            }

            // Now, clear the cache with information from the database.
            // Everything should be lost.
            this.uut.UpdateCacheFromDatabase();
            Assert.IsNull( this.uut.GetInfo( defaultKey1 ) );
        }
    }
}
