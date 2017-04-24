//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Plugins.QuoteBot;
using NUnit.Framework;

namespace Tests.Plugins.QuoteBot
{
    [TestFixture]
    public class QuoteBotParserTest
    {
        // ---------------- Fields ----------------

        private QuoteBotConfig quoteConfig;

        private QuoteBotParser uut;

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.quoteConfig = new QuoteBotConfig();

            // Pretend someone is VERY generious with their regexes (.* instead of \S+ for user,
            // has no mimimum size, no number checks for IDs).
            this.quoteConfig.AddCommand = @"!quote\s+add\s+\<(?<user>.*)\>\s+(?<quote>.*)";
            this.quoteConfig.DeleteCommand = @"!quote\s+delete\s+(?<id>.*)";
            this.quoteConfig.GetCommand = @"!quote\s+(get)?\s*(?<id>.*)";

            this.uut = new QuoteBotParser( this.quoteConfig );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        [Test]
        public void ParseAddCommandTest()
        {
            string errorString;
            Quote quote;

            // Good test.
            {
                Assert.IsTrue( this.uut.TryParseAddCommand( "!quote add <me> This is a quote!", out quote, out errorString ) );
                Assert.IsEmpty( errorString );
                Assert.IsNull( quote.Id );
                Assert.AreEqual( "me", quote.Author );
                Assert.AreEqual( "This is a quote!", quote.QuoteText );
            }

            // Does Not Match
            {
                Assert.IsFalse( this.uut.TryParseAddCommand( "!Derp", out quote, out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.IsNull( quote );
                Assert.IsTrue( errorString.Contains( "match" ) ); // Error due to matching, we should have "match" somewhere in there.
            }

            // Empty Author
            {
                Assert.IsFalse( this.uut.TryParseAddCommand( "!quote add <> This is a quote!", out quote, out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.IsNull( quote );
                Assert.IsTrue( errorString.Contains( "Author" ) ); // Error due to empty author, we should have "Author" somewhere in there.
            }

            // Empty QuoteText
            {
                Assert.IsFalse( this.uut.TryParseAddCommand( "!quote add <me>  ", out quote, out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.IsNull( quote );
                Assert.IsTrue( errorString.Contains( "Quote Text" ) ); // Error due to empty quote text, we should have "Quote Text" somewhere in there.
            }
        }

        [Test]
        public void ParseDeleteTest()
        {
            string errorString;
            long quoteId;

            // Good test.
            {
                Assert.IsTrue( this.uut.TryParseDeleteCommand( "!quote delete 13", out quoteId, out errorString ) );
                Assert.IsEmpty( errorString );
                Assert.AreEqual( 13, quoteId );
            }

            // Does Not Match
            {
                Assert.IsFalse( this.uut.TryParseDeleteCommand( "!Derp", out quoteId, out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.AreEqual( -1, quoteId );
                Assert.IsTrue( errorString.Contains( "match" ) ); // Error due to matching, we should have "match" somewhere in there.
            }

            // Bad ID
            {
                Assert.IsFalse( this.uut.TryParseDeleteCommand( "!quote delete derp", out quoteId, out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.AreEqual( -1, quoteId );
                Assert.IsTrue( errorString.Contains( "valid int" ) ); // Error due to not valid int, we should have "valid int" somewhere in there.
            }

            // Negative ID
            {
                Assert.IsFalse( this.uut.TryParseDeleteCommand( "!quote delete -2", out quoteId, out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.AreEqual( -1, quoteId );
                Assert.IsTrue( errorString.Contains( "negative" ) ); // Error due to a negative value, we should have "negative" somewhere in there.
            }
        }

        [Test]
        public void ParseRandomTest()
        {
            string errorString;

            // Good test.
            {
                Assert.IsTrue( this.uut.TryParseRandomCommand( "!quote random", out errorString ) );
                Assert.IsEmpty( errorString );
            }

            // Does Not Match
            {
                Assert.IsFalse( this.uut.TryParseRandomCommand( "!Derp", out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.IsTrue( errorString.Contains( "match" ) ); // Error due to matching, we should have "match" somewhere in there.
            }
        }

        [Test]
        public void ParseGetTest()
        {
            string errorString;
            long quoteId;

            // Good test.
            {
                Assert.IsTrue( this.uut.TryParseGetCommand( "!quote get 13", out quoteId, out errorString ) );
                Assert.IsEmpty( errorString );
                Assert.AreEqual( 13, quoteId );
            }

            // Does Not Match
            {
                Assert.IsFalse( this.uut.TryParseGetCommand( "!Derp", out quoteId, out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.AreEqual( -1, quoteId );
                Assert.IsTrue( errorString.Contains( "match" ) ); // Error due to matching, we should have "match" somewhere in there.
            }

            // Bad ID
            {
                Assert.IsFalse( this.uut.TryParseGetCommand( "!quote get derp", out quoteId, out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.AreEqual( -1, quoteId );
                Assert.IsTrue( errorString.Contains( "valid int" ) ); // Error due to not valid int, we should have "valid int" somewhere in there.
            }

            // Negative ID
            {
                Assert.IsFalse( this.uut.TryParseGetCommand( "!quote get -2", out quoteId, out errorString ) );
                Assert.IsNotEmpty( errorString );
                Assert.AreEqual( -1, quoteId );
                Assert.IsTrue( errorString.Contains( "negative" ) ); // Error due to a negative value, we should have "negative" somewhere in there.
            }
        }
    }
}
