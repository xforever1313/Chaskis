//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Threading;
using Chaskis.Plugins.KarmaBot;
using NUnit.Framework;
using SethCS.Extensions;

namespace Chaskis.UnitTests.PluginTests.Plugins.KarmaBot
{
    [TestFixture]
    public class KaramaBotDatabaseTests
    {
        // -------- Fields --------

        /// <summary>
        /// The name of the database to use.
        /// </summary>
        private const string databaseName = "test.ldb";

        /// <summary>
        /// User 1's name
        /// </summary>
        private const string user1Name = "user1";

        /// <summary>
        /// User 2's name
        /// </summary>
        private const string user2Name = "user2";

        /// <summary>
        /// Unit under test.
        /// </summary>
        private KarmaBotDatabase uut;

        /// <summary>
        /// So one test runs at a time.
        /// </summary>
        private static Mutex mutex = new Mutex();

        private static readonly string databasePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            databaseName
        );

        [SetUp]
        public void TestSetUp()
        {
            Assert.IsTrue( mutex.WaitOne( 60 * 1000 ) );

            this.DeleteDb();

            this.uut = new KarmaBotDatabase( databasePath );
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
            if( File.Exists( databasePath ) )
            {
                File.Delete( databasePath );

                if( Environment.UserName.EqualsIgnoreCase( "ContainerUser" ) )
                {
                    // No idea why we need to do this in a docker container, but we do >_>.
                    Console.WriteLine( "Sleeping after deleting in container" );
                    Thread.Sleep( 5000 );
                }
            }
        }

        /// <summary>
        /// Ensures we can increase the karama of a user.
        /// </summary>
        [Test]
        public void IncreaseKarmaTest()
        {
            // New user, karma should be 1.
            int karama = uut.IncreaseKarma( user1Name ).Result;
            Assert.AreEqual( 1, karama );
            Assert.AreEqual( 1, uut.QueryKarma( user1Name ).Result );

            karama = uut.IncreaseKarma( user1Name ).Result;
            Assert.AreEqual( 2, karama );
            Assert.AreEqual( 2, uut.QueryKarma( user1Name ).Result );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databasePath );

            karama = uut.IncreaseKarma( user1Name ).Result;
            Assert.AreEqual( 3, karama );
            Assert.AreEqual( 3, uut.QueryKarma( user1Name ).Result );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databasePath );

            // Ensure querying is okay.
            Assert.AreEqual( 3, uut.QueryKarma( user1Name ).Result );

            // One more increase, since why not?
            karama = uut.IncreaseKarma( user1Name ).Result;
            Assert.AreEqual( 4, karama );
            Assert.AreEqual( 4, uut.QueryKarma( user1Name ).Result );
        }

        /// <summary>
        /// Ensures we can increase several user's karma
        /// </summary>
        [Test]
        public void IncreaseKarmaMultiUserTest()
        {
            // New user, karma should be 1.
            int karama = uut.IncreaseKarma( user1Name ).Result;
            Assert.AreEqual( 1, karama );
            Assert.AreEqual( 1, uut.QueryKarma( user1Name ).Result );

            // New User, karma should be 1.
            Assert.AreEqual( 0, uut.QueryKarma( user2Name ).Result );
            karama = uut.IncreaseKarma( user2Name ).Result;
            Assert.AreEqual( 1, karama );
            Assert.AreEqual( 1, uut.QueryKarma( user2Name ).Result );

            // user 1 should be increaesd, but no user 2.
            karama = uut.IncreaseKarma( user1Name ).Result;
            Assert.AreEqual( 2, karama );
            Assert.AreEqual( 2, uut.QueryKarma( user1Name ).Result );
            Assert.AreEqual( 1, uut.QueryKarma( user2Name ).Result );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databasePath );

            karama = uut.IncreaseKarma( user1Name ).Result;
            Assert.AreEqual( 3, karama );
            Assert.AreEqual( 3, uut.QueryKarma( user1Name ).Result );

            // User 2 should be the same
            Assert.AreEqual( 1, uut.QueryKarma( user2Name ).Result );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databasePath );

            // Ensure querying is okay.
            Assert.AreEqual( 3, uut.QueryKarma( user1Name ).Result );
            Assert.AreEqual( 1, uut.QueryKarma( user2Name ).Result );

            // One more increase, since why not?
            karama = uut.IncreaseKarma( user1Name ).Result;
            Assert.AreEqual( 4, karama );
            Assert.AreEqual( 4, uut.QueryKarma( user1Name ).Result );
            Assert.AreEqual( 1, uut.QueryKarma( user2Name ).Result );
        }

        /// <summary>
        /// Ensures we can decrease the karama of a user.
        /// </summary>
        [Test]
        public void DecreaseKarmaTest()
        {
            // New user, karma should be 1.
            int karama = uut.DecreaseKarma( user1Name ).Result;
            Assert.AreEqual( -1, karama );
            Assert.AreEqual( -1, uut.QueryKarma( user1Name ).Result );

            karama = uut.DecreaseKarma( user1Name ).Result;
            Assert.AreEqual( -2, karama );
            Assert.AreEqual( -2, uut.QueryKarma( user1Name ).Result );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databasePath );

            karama = uut.DecreaseKarma( user1Name ).Result;
            Assert.AreEqual( -3, karama );
            Assert.AreEqual( -3, uut.QueryKarma( user1Name ).Result );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databasePath );

            // Ensure querying is okay.
            Assert.AreEqual( -3, uut.QueryKarma( user1Name ).Result );

            // One more increase, since why not?
            karama = uut.DecreaseKarma( user1Name ).Result;
            Assert.AreEqual( -4, karama );
            Assert.AreEqual( -4, uut.QueryKarma( user1Name ).Result );
        }

        /// <summary>
        /// Ensures we can decrease several user's karma
        /// </summary>
        [Test]
        public void DecreaseKarmaMultiUserTest()
        {
            // New user, karma should be 1.
            int karama = uut.DecreaseKarma( user1Name ).Result;
            Assert.AreEqual( -1, karama );
            Assert.AreEqual( -1, uut.QueryKarma( user1Name ).Result );

            // New User, karma should be 1.
            Assert.AreEqual( 0, uut.QueryKarma( user2Name ).Result );
            karama = uut.DecreaseKarma( user2Name ).Result;
            Assert.AreEqual( -1, karama );
            Assert.AreEqual( -1, uut.QueryKarma( user2Name ).Result );

            // user 1 should be increaesd, but no user 2.
            karama = uut.DecreaseKarma( user1Name ).Result;
            Assert.AreEqual( -2, karama );
            Assert.AreEqual( -2, uut.QueryKarma( user1Name ).Result );
            Assert.AreEqual( -1, uut.QueryKarma( user2Name ).Result );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databasePath );

            karama = uut.DecreaseKarma( user1Name ).Result;
            Assert.AreEqual( -3, karama );
            Assert.AreEqual( -3, uut.QueryKarma( user1Name ).Result );

            // User 2 should be the same
            Assert.AreEqual( -1, uut.QueryKarma( user2Name ).Result );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databasePath );

            // Ensure querying is okay.
            Assert.AreEqual( -3, uut.QueryKarma( user1Name ).Result );
            Assert.AreEqual( -1, uut.QueryKarma( user2Name ).Result );

            // One more increase, since why not?
            karama = uut.DecreaseKarma( user1Name ).Result;
            Assert.AreEqual( -4, karama );
            Assert.AreEqual( -4, uut.QueryKarma( user1Name ).Result );
            Assert.AreEqual( -1, uut.QueryKarma( user2Name ).Result );
        }
    }
}