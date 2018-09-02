//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;
using Chaskis.Plugins.UrlBot;
using ChaskisCore;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.UrlBot
{
    [TestFixture]
    public class UrlResponseTest
    {
        // ---------------- Fields ----------------

        // ---------------- Setup / Teardown ----------------

        // ---------------- Tests ----------------

        [Test]
        public void MaxCharacterTest()
        {
            StringBuilder builder = new StringBuilder();
            for( int i = 0; i < IrcConnection.MaximumLength; ++i )
            {
                builder.Append( (char)( '0' + i ) % 10 );
            }
            string str = builder.ToString();

            Assert.AreEqual( IrcConnection.MaximumLength, str.Length );

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
        public void MoreThanMaxTest()
        {
            StringBuilder builder = new StringBuilder();
            for( int i = 0; i < ( IrcConnection.MaximumLength + 1 ); ++i )
            {
                builder.Append( (char)( '0' + i ) % 10 );
            }
            string str = builder.ToString();

            Assert.AreEqual( IrcConnection.MaximumLength + 1, str.Length );

            string shortenedStr = str.Substring( 0, IrcConnection.MaximumLength - 3 ) + "...";
            Assert.AreEqual( IrcConnection.MaximumLength, shortenedStr.Length );

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
