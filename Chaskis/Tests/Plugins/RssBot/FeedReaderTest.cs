//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using Chaskis.Plugins.RssBot;
using NUnit.Framework;
using SethCS.Extensions;

namespace Tests.Plugins.RssBot
{
    [TestFixture]
    public class FeedReaderTest
    {
        // ---------------- Fields ----------------

        private string initFeed;
        private string initLessItems;
        private string updateFeed;
        private string updateSameAmountFeed;

        private string testFeedPath;
        private string testFeedUri;

        private FeedReader uut;

        // ---------------- Setup / Teardown ----------------

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.initFeed = Path.GetFullPath(
                Path.Combine( RssBotTestHelpers.RssTestFilesPath, "Reddit_Init.xml" )
            );

            this.initLessItems = Path.GetFullPath(
                Path.Combine( RssBotTestHelpers.RssTestFilesPath, "Reddit_Init_Less_Items.xml" )
            );

            this.updateFeed = Path.GetFullPath(
                Path.Combine( RssBotTestHelpers.RssTestFilesPath, "Reddit_Update.xml" )
            );

            this.updateSameAmountFeed = Path.GetFullPath(
                Path.Combine( RssBotTestHelpers.RssTestFilesPath, "Reddit_Update_Same_Number.xml" )
            );



            this.testFeedPath = Path.GetFullPath(
                "Test.xml"
            );

            this.testFeedUri = SethPath.ToUri( this.testFeedPath );
        }

        [TestFixtureTearDown]
        public void FixtureTeardown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
            File.Copy( this.initFeed, this.testFeedPath, true );

            Feed feedConfig = new Feed();
            feedConfig.RefreshInterval = TimeSpan.FromMinutes( 1 );
            feedConfig.Url = this.testFeedUri;
            feedConfig.AddChannel( TestHelpers.GetTestIrcConfig().Channels[0] );

            this.uut = new FeedReader( feedConfig, TestHelpers.HttpClient );
        }

        [TearDown]
        public void TestTearDown()
        {
            if( File.Exists( testFeedPath ) )
            {
                File.Delete( testFeedPath );
            }
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Does our feed return nothing if there are no updates?
        /// </summary>
        [Test]
        public void NoNewItemsTest()
        {
            this.uut.Init();

            // Don't change the RSS file, expect no updates to return.
            IList<SyndicationItem> items = this.uut.UpdateAsync().Result;

            Assert.AreEqual( 0, items.Count );
        }

        /// <summary>
        /// What happens if we update our feed, but nothing changes, but we have less items?
        /// Should get no items.
        /// </summary>
        [Test]
        public void NoNewItemsWithLessItemsTest()
        {
            this.uut.Init();

            File.Copy( this.initLessItems, this.testFeedPath, true );

            // Don't change the RSS file, expect no updates to return.
            IList<SyndicationItem> items = this.uut.UpdateAsync().Result;

            Assert.AreEqual( 0, items.Count );
        }

        /// <summary>
        /// Does our feed return items if there is an update,
        /// and there are more items.
        /// </summary>
        [Test]
        public void NewItemsTest()
        {
            this.uut.Init();

            File.Copy( this.updateFeed, this.testFeedPath, true );

            IList<SyndicationItem> items = this.uut.UpdateAsync().Result;

            // 3 new items.
            Assert.AreEqual( 3, items.Count );

            {
                // Oldest should be in slot zero.
                SyndicationItem item0 = items[0];
                Assert.AreEqual(
                    "https://www.reddit.com/r/csharp/comments/3xn6sm/welcome_to_rcsharp_read_this_post_before/",
                    item0.Links[0].Uri.AbsoluteUri
                );

                Assert.AreEqual(
                    "Welcome to /r/csharp! Read this post before submitting.",
                    item0.Title.Text
                );
            }
            {
                // Newer one should be in the next index.
                SyndicationItem item1 = items[1];
                Assert.AreEqual(
                    "https://www.reddit.com/r/csharp/comments/6ea2vi/penguinupload_file_hosting_service_built_with/",
                    item1.Links[0].Uri.AbsoluteUri
                );

                Assert.AreEqual(
                    "PenguinUpload - file hosting service built with ASP.NET Core, NancyFx, and Vue.js",
                    item1.Title.Text
                );
            }
            {
                // Newest should have the highest index.
                SyndicationItem item2 = items[2];
                Assert.AreEqual(
                    "https://www.reddit.com/r/csharp/comments/6ebyxj/c_7_series_part_2_async_main/",
                    item2.Links[0].Uri.AbsoluteUri
                );

                Assert.AreEqual(
                    "C# 7 Series, Part 2: Async Main",
                    item2.Title.Text
                );
            }

            // Do another update, should get nothing.
            items = this.uut.UpdateAsync().Result;
            Assert.AreEqual( 0, items.Count );
        }

        /// <summary>
        /// Does our feed return items if there is an update, but there are
        /// the same number of items.
        /// </summary>
        [Test]
        public void NewItemsTest_SameAmount()
        {
            this.uut.Init();

            File.Copy( this.updateSameAmountFeed, this.testFeedPath, true );

            IList<SyndicationItem> items = this.uut.UpdateAsync().Result;

            // 3 new items.
            Assert.AreEqual( 3, items.Count );

            {
                // Oldest should be in slot zero.
                SyndicationItem item0 = items[0];
                Assert.AreEqual(
                    "https://www.reddit.com/r/csharp/comments/3xn6sm/welcome_to_rcsharp_read_this_post_before/",
                    item0.Links[0].Uri.AbsoluteUri
                );

                Assert.AreEqual(
                    "Welcome to /r/csharp! Read this post before submitting.",
                    item0.Title.Text
                );
            }
            {
                // Newer one should be in the next index.
                SyndicationItem item1 = items[1];
                Assert.AreEqual(
                    "https://www.reddit.com/r/csharp/comments/6ea2vi/penguinupload_file_hosting_service_built_with/",
                    item1.Links[0].Uri.AbsoluteUri
                );

                Assert.AreEqual(
                    "PenguinUpload - file hosting service built with ASP.NET Core, NancyFx, and Vue.js",
                    item1.Title.Text
                );
            }
            {
                // Newest should have the highest index.
                SyndicationItem item2 = items[2];
                Assert.AreEqual(
                    "https://www.reddit.com/r/csharp/comments/6ebyxj/c_7_series_part_2_async_main/",
                    item2.Links[0].Uri.AbsoluteUri
                );

                Assert.AreEqual(
                    "C# 7 Series, Part 2: Async Main",
                    item2.Title.Text
                );
            }

            // Do another update, should get nothing.
            items = this.uut.UpdateAsync().Result;
            Assert.AreEqual( 0, items.Count );
        }
    }
}
