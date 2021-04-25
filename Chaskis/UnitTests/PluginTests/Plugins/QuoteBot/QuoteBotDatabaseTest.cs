//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Chaskis.Plugins.QuoteBot;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.QuoteBot
{
    [TestFixture]
    public class QuoteBotDatabaseTest
    {
        // ---------------- Fields ----------------

        private const string dbName = "quotebottest.ldb";

        private static readonly string dbPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            dbName
        );

        private QuoteBotDatabase uut;

        private Quote quote1;
        private Quote quote2;
        private Quote quote3;

        /// <summary>
        /// So one test runs at a time.
        /// </summary>
        private static Mutex mutex = new Mutex();

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            Assert.IsTrue( mutex.WaitOne( 60 * 1000 ) );

            this.DeleteDb();
            this.uut = new QuoteBotDatabase( dbPath );

            this.quote1 = new Quote();
            this.quote1.Author = "xforever1313";
            this.quote1.QuoteText = "Here is my quote!";

            this.quote2 = new Quote();
            this.quote2.Author = "thenaterhood";
            this.quote2.QuoteText = "Here is another quote!";

            this.quote3 = new Quote();
            this.quote3.Author = "jgallstar1";
            this.quote3.QuoteText = "This is yet another quote!";

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

        /// <summary>
        /// Deletes the database if it exists.
        /// </summary>
        private void DeleteDb()
        {
            if( File.Exists( dbPath ) )
            {
                File.Delete( dbPath );
            }
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures adding and deleteing works.
        /// </summary>
        [Test]
        public void AddGetDeleteTest()
        {
            int quote1Id = this.uut.AddQuote( quote1 );
            int quote2Id = this.uut.AddQuote( quote2 );

            // Try async
            Task<int> addTask = this.uut.AddQuoteAsync( quote3 );
            Assert.DoesNotThrow( () => addTask.Wait() );
            int quote3Id = addTask.Result;

            // Make sure our IDs are unique.
            Assert.AreNotEqual( quote1Id, quote2Id );
            Assert.AreNotEqual( quote2Id, quote3Id );
            Assert.AreNotEqual( quote3Id, quote1Id );

            // Get the quotes.
            {
                Task<Quote> task = this.uut.GetQuoteAsync( quote1Id );
                Assert.DoesNotThrow( () => task.Wait() );
                Quote gotQuote1 = task.Result;

                Assert.AreEqual( quote1Id, gotQuote1.Id );
                Assert.AreEqual( quote1.Author, gotQuote1.Author );
                Assert.AreEqual( quote1.QuoteText, gotQuote1.QuoteText );
            }

            {
                Task<Quote> task = this.uut.GetQuoteAsync( quote2Id );
                Assert.DoesNotThrow( () => task.Wait() );
                Quote gotQuote2 = task.Result;

                Assert.AreEqual( quote2Id, gotQuote2.Id );
                Assert.AreEqual( quote2.Author, gotQuote2.Author );
                Assert.AreEqual( quote2.QuoteText, gotQuote2.QuoteText );
            }

            // Try non-async
            {
                Quote gotQuote3 = this.uut.GetQuote( quote3Id );
                Assert.AreEqual( quote3Id, gotQuote3.Id );
                Assert.AreEqual( quote3.Author, gotQuote3.Author );
                Assert.AreEqual( quote3.QuoteText, gotQuote3.QuoteText );
            }

            // Try deleting async
            {
                Task<bool> task = this.uut.DeleteQuoteAsync( quote3Id );
                task.Wait();
                Assert.IsTrue( task.Result );

                Task<Quote> getQuoteTask = this.uut.GetQuoteAsync( quote3Id );
                getQuoteTask.Wait();
                Assert.IsNull( getQuoteTask.Result );

                // Try deleting again.
                task = this.uut.DeleteQuoteAsync( quote3Id );
                task.Wait();
                Assert.IsFalse( task.Result );
            }

            // Try deleting non-async
            {
                Assert.IsTrue( this.uut.DeleteQuote( quote1Id ) );
                Assert.IsNull( this.uut.GetQuote( quote1Id ) );

                // Try deleting again.
                Assert.IsFalse( this.uut.DeleteQuote( quote1Id ) );
            }

            // That just leaves #2.  Close and reconnect.  Make sure #2 is still there.
            {
                this.uut.Dispose();
                this.uut = new QuoteBotDatabase( dbPath );

                Quote gotQuote2 = this.uut.GetQuote( quote2Id );
                Assert.AreEqual( quote2Id, gotQuote2.Id );
                Assert.AreEqual( quote2.Author, gotQuote2.Author );
                Assert.AreEqual( quote2.QuoteText, gotQuote2.QuoteText );
            }
        }

        [Test]
        public void RandomTest()
        {
            // Do empty.  Returns null.
            {
                Assert.IsNull( this.uut.GetRandomQuote() );
            }

            int quote1Id = this.uut.AddQuote( this.quote1 );
            int quote2Id = this.uut.AddQuote( this.quote2 );
            int quote3Id = this.uut.AddQuote( this.quote3 );

            // Sync
            {
                Quote randomQuote = this.uut.GetRandomQuote();
                Assert.IsTrue(
                    ( randomQuote.Id == quote1Id ) ||
                    ( randomQuote.Id == quote2Id ) ||
                    ( randomQuote.Id == quote3Id )
                );
            }

            // Do Async
            {
                Task<Quote> task = this.uut.GetRandomQuoteAsync();
                task.Wait();

                Quote randomQuote = task.Result;
                Assert.IsTrue(
                    ( randomQuote.Id == quote1Id ) ||
                    ( randomQuote.Id == quote2Id ) ||
                    ( randomQuote.Id == quote3Id )
                );
            }
        }
    }
}
