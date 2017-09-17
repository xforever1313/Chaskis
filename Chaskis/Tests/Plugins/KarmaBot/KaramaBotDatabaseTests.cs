//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using Chaskis.Plugins.KarmaBot;
using NUnit.Framework;

namespace Tests.Plugins.KarmaBot
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

        [SetUp]
        public void TestSetUp()
        {
            if( File.Exists( databaseName ) )
            {
                File.Delete( databaseName );
            }

            this.uut = new KarmaBotDatabase( databaseName );
        }

        [TearDown]
        public void TestTeardown()
        {
            this.uut.Dispose();

            if( File.Exists( databaseName ) )
            {
                File.Delete( databaseName );
            }
        }

        /// <summary>
        /// Ensures we can increase the karama of a user.
        /// </summary>
        [Test]
        public async void IncreaseKarmaTest()
        {
            // New user, karma should be 1.
            int karama = await uut.IncreaseKarma( user1Name );
            Assert.AreEqual( 1, karama );
            Assert.AreEqual( 1, await uut.QueryKarma( user1Name ) );

            karama = await uut.IncreaseKarma( user1Name );
            Assert.AreEqual( 2, karama );
            Assert.AreEqual( 2, await uut.QueryKarma( user1Name ) );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databaseName );

            karama = await uut.IncreaseKarma( user1Name );
            Assert.AreEqual( 3, karama );
            Assert.AreEqual( 3, await uut.QueryKarma( user1Name ) );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databaseName );

            // Ensure querying is okay.
            Assert.AreEqual( 3, await uut.QueryKarma( user1Name ) );

            // One more increase, since why not?
            karama = await uut.IncreaseKarma( user1Name );
            Assert.AreEqual( 4, karama );
            Assert.AreEqual( 4, await uut.QueryKarma( user1Name ) );
        }

        /// <summary>
        /// Ensures we can increase several user's karma
        /// </summary>
        [Test]
        public async void IncreaseKarmaMultiUserTest()
        {
            // New user, karma should be 1.
            int karama = await uut.IncreaseKarma( user1Name );
            Assert.AreEqual( 1, karama );
            Assert.AreEqual( 1, await uut.QueryKarma( user1Name ) );

            // New User, karma should be 1.
            Assert.AreEqual( 0, await uut.QueryKarma( user2Name ) );
            karama = await uut.IncreaseKarma( user2Name );
            Assert.AreEqual( 1, karama );
            Assert.AreEqual( 1, await uut.QueryKarma( user2Name ) );

            // user 1 should be increaesd, but no user 2.
            karama = await uut.IncreaseKarma( user1Name );
            Assert.AreEqual( 2, karama );
            Assert.AreEqual( 2, await uut.QueryKarma( user1Name ) );
            Assert.AreEqual( 1, await uut.QueryKarma( user2Name ) );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databaseName );

            karama = await uut.IncreaseKarma( user1Name );
            Assert.AreEqual( 3, karama );
            Assert.AreEqual( 3, await uut.QueryKarma( user1Name ) );

            // User 2 should be the same
            Assert.AreEqual( 1, await uut.QueryKarma( user2Name ) );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databaseName );

            // Ensure querying is okay.
            Assert.AreEqual( 3, await uut.QueryKarma( user1Name ) );
            Assert.AreEqual( 1, await uut.QueryKarma( user2Name ) );

            // One more increase, since why not?
            karama = await uut.IncreaseKarma( user1Name );
            Assert.AreEqual( 4, karama );
            Assert.AreEqual( 4, await uut.QueryKarma( user1Name ) );
            Assert.AreEqual( 1, await uut.QueryKarma( user2Name ) );
        }

        /// <summary>
        /// Ensures we can decrease the karama of a user.
        /// </summary>
        [Test]
        public async void DecreaseKarmaTest()
        {
            // New user, karma should be 1.
            int karama = await uut.DecreaseKarma( user1Name );
            Assert.AreEqual( -1, karama );
            Assert.AreEqual( -1, await uut.QueryKarma( user1Name ) );

            karama = await uut.DecreaseKarma( user1Name );
            Assert.AreEqual( -2, karama );
            Assert.AreEqual( -2, await uut.QueryKarma( user1Name ) );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databaseName );

            karama = await uut.DecreaseKarma( user1Name );
            Assert.AreEqual( -3, karama );
            Assert.AreEqual( -3, await uut.QueryKarma( user1Name ) );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databaseName );

            // Ensure querying is okay.
            Assert.AreEqual( -3, await uut.QueryKarma( user1Name ) );

            // One more increase, since why not?
            karama = await uut.DecreaseKarma( user1Name );
            Assert.AreEqual( -4, karama );
            Assert.AreEqual( -4, await uut.QueryKarma( user1Name ) );
        }

        /// <summary>
        /// Ensures we can decrease several user's karma
        /// </summary>
        [Test]
        public async void DecreaseKarmaMultiUserTest()
        {
            // New user, karma should be 1.
            int karama = await uut.DecreaseKarma( user1Name );
            Assert.AreEqual( -1, karama );
            Assert.AreEqual( -1, await uut.QueryKarma( user1Name ) );

            // New User, karma should be 1.
            Assert.AreEqual( 0, await uut.QueryKarma( user2Name ) );
            karama = await uut.DecreaseKarma( user2Name );
            Assert.AreEqual( -1, karama );
            Assert.AreEqual( -1, await uut.QueryKarma( user2Name ) );

            // user 1 should be increaesd, but no user 2.
            karama = await uut.DecreaseKarma( user1Name );
            Assert.AreEqual( -2, karama );
            Assert.AreEqual( -2, await uut.QueryKarma( user1Name ) );
            Assert.AreEqual( -1, await uut.QueryKarma( user2Name ) );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databaseName );

            karama = await uut.DecreaseKarma( user1Name );
            Assert.AreEqual( -3, karama );
            Assert.AreEqual( -3, await uut.QueryKarma( user1Name ) );

            // User 2 should be the same
            Assert.AreEqual( -1, await uut.QueryKarma( user2Name ) );

            // Dispose and reopen.
            this.uut.Dispose();
            this.uut = new KarmaBotDatabase( databaseName );

            // Ensure querying is okay.
            Assert.AreEqual( -3, await uut.QueryKarma( user1Name ) );
            Assert.AreEqual( -1, await uut.QueryKarma( user2Name ) );

            // One more increase, since why not?
            karama = await uut.DecreaseKarma( user1Name );
            Assert.AreEqual( -4, karama );
            Assert.AreEqual( -4, await uut.QueryKarma( user1Name ) );
            Assert.AreEqual( -1, await uut.QueryKarma( user2Name ) );
        }
    }
}