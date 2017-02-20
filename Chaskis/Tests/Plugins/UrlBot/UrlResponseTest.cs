
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Text;
using Chaskis.Plugins.UrlBot;
using NUnit.Framework;

namespace Tests.Plugins.UrlBot
{
    [TestFixture]
    public class UrlResponseTest
    {
        // ---------------- Fields ----------------

        // ---------------- Setup / Teardown ----------------

        // ---------------- Tests ----------------

        [Test]
        public void EightyCharacterTest()
        {
            const string str = "12345678911234567892123456789312345678941234567895123456789612345678971234567898";
            Assert.AreEqual( 80, str.Length );

            UrlResponse response = new UrlResponse();
            response.Title = str;

            Assert.IsTrue( response.IsValid );
            Assert.AreEqual( str, response.Title );
            Assert.AreEqual( str, response.TitleShortened );
        }

        [Test]
        public void NewLineTest()
        {
            string str = "Hello" + Environment.NewLine + "World";

            // Need a for-loop due to line endings
            StringBuilder builder = new StringBuilder();
            builder.Append( "Hello" );
            foreach( char ch in Environment.NewLine )
            {
                builder.Append( " " );
            }
            builder.Append( "World" );

            UrlResponse response = new UrlResponse();
            response.Title = str;

            Assert.IsTrue( response.IsValid );
            Assert.AreEqual( str, response.Title );
            Assert.AreEqual( builder.ToString(), response.TitleShortened );
        }

        [Test]
        public void EightyOneCharacterTest()
        {
            const string str = "123456789112345678921234567893123456789412345678951234567896123456789712345678981";
            Assert.AreEqual( 81, str.Length );

            const string shortenedStr = "12345678911234567892123456789312345678941234567895123456789612345678971234567...";
            Assert.AreEqual( 80, shortenedStr.Length );

            UrlResponse response = new UrlResponse();
            response.Title = str;

            Assert.IsTrue( response.IsValid );
            Assert.AreEqual( str, response.Title );
            Assert.AreEqual( shortenedStr, response.TitleShortened );
        }

        [Test]
        public void NullEmptyTest()
        {
            UrlResponse uut = new UrlResponse();
            uut.Title = null;
            Assert.IsFalse( uut.IsValid );

            uut.Title = string.Empty;
            Assert.IsFalse( uut.IsValid );

            uut.Title = "                  ";
            Assert.IsFalse( uut.IsValid );
        }
    }
}
