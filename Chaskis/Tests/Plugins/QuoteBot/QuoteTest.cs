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
    public class QuoteTest
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures our validate function works as expected.
        /// </summary>
        [Test]
        public void ValidateTest()
        {
            Quote quote = new Quote();
            quote.Author = "author";
            quote.QuoteText = "Some Quote.";

            string errorString;
            Assert.IsTrue( quote.TryValidate( out errorString ) );
            Assert.IsEmpty( errorString );

            // Change Author:
            quote.Author = null;
            Assert.IsFalse( quote.TryValidate( out errorString ) );
            Assert.IsNotEmpty( errorString );

            quote.Author = string.Empty;
            Assert.IsFalse( quote.TryValidate( out errorString ) );
            Assert.IsNotEmpty( errorString );

            quote.Author = "             ";
            Assert.IsFalse( quote.TryValidate( out errorString ) );
            Assert.IsNotEmpty( errorString );

            // Change Quote Text:
            quote.QuoteText = null;
            Assert.IsFalse( quote.TryValidate( out errorString ) );
            Assert.IsNotEmpty( errorString );

            quote.QuoteText = string.Empty;
            Assert.IsFalse( quote.TryValidate( out errorString ) );
            Assert.IsNotEmpty( errorString );

            quote.QuoteText = "             ";
            Assert.IsFalse( quote.TryValidate( out errorString ) );
            Assert.IsNotEmpty( errorString );
        }
    }
}
