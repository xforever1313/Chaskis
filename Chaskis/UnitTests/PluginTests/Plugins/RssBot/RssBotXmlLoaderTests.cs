//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;
using Chaskis.Plugins.RssBot;
using Chaskis.UnitTests.Common;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.RssBot
{
    [TestFixture]
    public class RssBotXmlLoaderTests
    {
        /// <summary>
        /// Ensures a good XML results in a good config.
        /// </summary>
        [Test]
        public void GoodConfigTest()
        {
            string rssConfig = Path.Combine(
                TestHelpers.PluginDir,
                "RssBot",
                "Config",
                "SampleRssBotConfig.xml"
            );

            RssBotConfig config = XmlLoader.ParseConfig( rssConfig );

            Assert.AreEqual( RssBotTestHelpers.TestUrl1, config.Feeds[0].Url );
            Assert.AreEqual( RssBotTestHelpers.Interval1, config.Feeds[0].RefreshInterval );
            Assert.AreEqual( 1, config.Feeds[0].Channels.Count );
            Assert.AreEqual( "#MyChannel", config.Feeds[0].Channels[0] );

            Assert.AreEqual( RssBotTestHelpers.TestUrl2, config.Feeds[1].Url );
            Assert.AreEqual( RssBotTestHelpers.Interval2, config.Feeds[1].RefreshInterval );
            Assert.AreEqual( 2, config.Feeds[1].Channels.Count );
            Assert.AreEqual( "#MyChannel", config.Feeds[1].Channels[0] );
            Assert.AreEqual( "#MyOtherChannel", config.Feeds[1].Channels[1] );
        }

        /// <summary>
        /// Ensures that if no feeds are present, a validation error occurs.
        /// </summary>
        [Test]
        public void NoFeedsTest()
        {
            string rssConfig = Path.Combine(
                RssBotTestHelpers.RssTestFilesPath,
                "BadEmptyFeedList.xml"
            );

            Assert.Throws<ValidationException>( () => XmlLoader.ParseConfig( rssConfig ) );
        }

        /// <summary>
        /// Ensures that if we get a bad root node, we get an exception.
        /// </summary>
        [Test]
        public void BadRootNode()
        {
            string rssConfig = Path.Combine(
                RssBotTestHelpers.RssTestFilesPath,
                "BadRootNode.xml"
            );

            Assert.Throws<XmlException>( () => XmlLoader.ParseConfig( rssConfig ) );
        }

        /// <summary>
        /// Ensures that if there is no file found, we get an exception.
        /// </summary>
        [Test]
        public void FileNotFoundTest()
        {
            Assert.Throws<FileNotFoundException>( () => XmlLoader.ParseConfig( "DNE.xml" ) );
        }
    }
}