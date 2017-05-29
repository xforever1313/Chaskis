//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Plugins.RssBot;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Tests.Plugins.RssBot
{
    [TestFixture]
    public class RssBotConfigTest
    {
        // ---------------- Setup / Teardown ----------------

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures the Feed's Clone() function works as expected.
        /// </summary>
        [Test]
        public void FeedCloneTest()
        {
            Feed feed1 = new Feed();
            feed1.Url = RssBotTestHelpers.TestUrl1;
            feed1.RefreshInterval = TimeSpan.MaxValue;

            Feed clonedFeed = feed1.Clone();

            Assert.AreNotSame( feed1, clonedFeed );
            Assert.AreEqual( feed1.Url, clonedFeed.Url );
            Assert.AreEqual( feed1.RefreshInterval, clonedFeed.RefreshInterval );
        }

        /// <summary>
        /// Ensures our TryValidate function on Feed works as expected.
        /// </summary>
        [Test]
        public void FeedValidateTest()
        {
            Feed feed = new Feed();
            feed.Url = RssBotTestHelpers.TestUrl1;
            feed.RefreshInterval = TimeSpan.Zero + TimeSpan.FromTicks( 1 ); // Positive is good.

            // Good case.
            {
                string errorString;
                Assert.IsTrue( feed.TryValidate( out errorString ) );
                Assert.IsEmpty( errorString );
            }

            // Empty URL.
            {
                feed.Url = string.Empty;
                string errorString;
                Assert.IsFalse( feed.TryValidate( out errorString ) );
                Assert.IsNotEmpty( errorString );
            }

            // Null URL.
            {
                feed.Url = null;
                string errorString;
                Assert.IsFalse( feed.TryValidate( out errorString ) );
                Assert.IsNotEmpty( errorString );
            }

            // Whitespace URL.
            {
                feed.Url = "   ";
                string errorString;
                Assert.IsFalse( feed.TryValidate( out errorString ) );
                Assert.IsNotEmpty( errorString );
            }

            feed.Url = RssBotTestHelpers.TestUrl1;

            // Zero timespan.
            {
                feed.RefreshInterval = TimeSpan.Zero;
                string errorString;
                Assert.IsFalse( feed.TryValidate( out errorString ) );
                Assert.IsNotEmpty( errorString );
            }

            // Negative Timespan
            {
                feed.RefreshInterval = TimeSpan.MinValue;
                string errorString;
                Assert.IsFalse( feed.TryValidate( out errorString ) );
                Assert.IsNotEmpty( errorString );
            }
        }

        /// <summary>
        /// Ensures RssConfig's validation function works as expected.
        /// </summary>
        [Test]
        public void RssConfigValidateTest()
        {
            Feed feed1 = new Feed();
            feed1.Url = RssBotTestHelpers.TestUrl1;
            feed1.RefreshInterval = TimeSpan.Zero + TimeSpan.FromTicks( 1 ); // Positive is good.

            Feed feed2 = new Feed();
            feed2.Url = RssBotTestHelpers.TestUrl2;
            feed2.RefreshInterval = TimeSpan.MaxValue;

            RssBotConfig config = new RssBotConfig();

            // Empty list should fail.
            Assert.Throws<ValidationException>( () => config.Validate() );

            config.AddFeed( feed1 );
            config.AddFeed( feed2 );
            Assert.DoesNotThrow( () => config.Validate() );

            // Ensure things get cloned by changing feed 1, and validation should still pass.
            feed1.Url = null;
            Assert.DoesNotThrow( () => config.Validate() );

            // Now add bad feed 1, and we should fail validation.
            config.AddFeed( feed1 );
            Assert.Throws<ValidationException>( () => config.Validate() );
        }
    }
}
