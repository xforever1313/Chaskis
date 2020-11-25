//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Plugins.RssBot;
using Chaskis.UnitTests.Common;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.RssBot
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
            feed1.AddChannel( TestHelpers.GetTestIrcConfig().Channels[0] );

            Feed clonedFeed = feed1.Clone();

            Assert.AreNotSame( feed1, clonedFeed );
            Assert.AreEqual( feed1.Url, clonedFeed.Url );
            Assert.AreEqual( feed1.RefreshInterval, clonedFeed.RefreshInterval );
            Assert.AreNotSame( feed1.Channels, clonedFeed.Channels );
            Assert.AreEqual( feed1.Channels[0], clonedFeed.Channels[0] );

            feed1.AddChannel( "#SomeChannel" );
            Assert.AreNotEqual( feed1.Channels.Count, clonedFeed.Channels.Count );
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

            // Empty Channel
            {
                string errorString;
                Assert.IsFalse( feed.TryValidate( out errorString ) );
                Assert.IsNotEmpty( errorString );
            }

            feed.AddChannel( TestHelpers.GetTestIrcConfig().Channels[0] );

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
            feed1.AddChannel( TestHelpers.GetTestIrcConfig().Channels[0] );

            Feed feed2 = new Feed();
            feed2.Url = RssBotTestHelpers.TestUrl2;
            feed2.RefreshInterval = TimeSpan.MaxValue;
            feed2.AddChannel( TestHelpers.GetTestIrcConfig().Channels[0] );

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

        /// <summary>
        /// Ensures the add channels function works as expected.
        /// </summary>
        [Test]
        public void AddChannelsTest()
        {
            Feed uut = new Feed();
            Assert.Throws<ArgumentNullException>( () => uut.AddChannel( null ) );
            Assert.Throws<ArgumentNullException>( () => uut.AddChannel( string.Empty ) );

            string channel = TestHelpers.GetTestIrcConfig().Channels[0];

            uut.AddChannel( channel );
            Assert.AreEqual( 1, uut.Channels.Count );
            Assert.AreEqual( channel, uut.Channels[0] );
        }
    }
}
