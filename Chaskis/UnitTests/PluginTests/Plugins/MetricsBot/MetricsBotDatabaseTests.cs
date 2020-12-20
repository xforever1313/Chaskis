//
//          Copyright Seth Hendrick 2016-2020.
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
    class MetricsBotDatabaseTests
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

        private readonly MessageInfoKey defaultKey = new MessageInfoKey
        {
            Channel = "#somechannel",
            IrcUser = "someuser",
            MessageType = MessageType.PrivMsg,
            Protocol = Protocol.IRC,
            Server = "irc.somewhere.net"
        };

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            Assert.IsTrue( mutex.WaitOne( 60 * 1000 ) );
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
            Assert.IsNull( this.uut.GetInfo( this.defaultKey ) );

            // Add one message, it should be inserted.
            {
                this.uut.AddNewMessage( this.defaultKey );
                this.uut.WriteCacheToDatabase();
                this.uut.UpdateCacheFromDatabase();

                MessageInfo foundInfo = this.uut.GetInfo( this.defaultKey );
                Assert.IsNotNull( foundInfo );
                Assert.AreEqual( this.defaultKey, foundInfo.Id );
                Assert.AreEqual( 1, foundInfo.Count );
            }

            // Add another message message, it should be updated instead of inserted.
            {
                this.uut.AddNewMessage( this.defaultKey );
                this.uut.WriteCacheToDatabase();
                this.uut.UpdateCacheFromDatabase();

                MessageInfo foundInfo = this.uut.GetInfo( this.defaultKey );
                Assert.IsNotNull( foundInfo );
                Assert.AreEqual( this.defaultKey, foundInfo.Id );
                Assert.AreEqual( 2, foundInfo.Count );
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
            Assert.IsNull( this.uut.GetInfo( this.defaultKey ) );

            // Add one message, it should be inserted.
            {
                this.uut.AddNewMessage( this.defaultKey );

                MessageInfo foundInfo = this.uut.GetInfo( this.defaultKey );
                Assert.IsNotNull( foundInfo );
                Assert.AreEqual( this.defaultKey, foundInfo.Id );
                Assert.AreEqual( 1, foundInfo.Count );
            }

            // Now, clear the cache with information from the database.
            // Everything should be lost.
            this.uut.UpdateCacheFromDatabase();
            Assert.IsNull( this.uut.GetInfo( this.defaultKey ) );
        }

        // ---------------- Test Helpers ----------------
    }
}
