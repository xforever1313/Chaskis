//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;
using Chaskis.Plugins.RssBot;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Tests.Plugins.RssBot
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

            Assert.AreEqual( config.Feeds[0].Url, RssBotTestHelpers.TestUrl1 );
            Assert.AreEqual( config.Feeds[0].RefreshInterval, RssBotTestHelpers.Interval1 );

            Assert.AreEqual( config.Feeds[1].Url, RssBotTestHelpers.TestUrl2 );
            Assert.AreEqual( config.Feeds[1].RefreshInterval, RssBotTestHelpers.Interval2 );
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