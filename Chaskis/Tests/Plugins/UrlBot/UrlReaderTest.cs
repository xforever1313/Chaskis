
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using Chaskis.Plugins.UrlBot;
using NUnit.Framework;

namespace Tests.Plugins.UrlBot
{
    [TestFixture]
    public class UrlReaderTest
    {
        // ---------------- Fields ----------------

        private UrlReader reader;

        private string urlTestFiles;

        private string goodSizeFile;

        private string bigSizeFile;

        // ---------------- Setup / Teardown ----------------

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.urlTestFiles = Path.GetFullPath(
                Path.Combine(
                    TestHelpers.TestsBaseDir,
                    "Plugins",
                    "UrlBot",
                    "TestFiles"
                )
            );

            this.goodSizeFile = Path.Combine( urlTestFiles, "GoodSize.html" );
            this.bigSizeFile = Path.Combine( urlTestFiles, "BigSize.html" );
        }

        [SetUp]
        public void TestSetup()
        {
            this.reader = new UrlReader();
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        [Test]
        public async void GoodFileTest()
        {
            string url = string.Format( "file:///{0}", this.goodSizeFile.Replace( "\\", "/" ) );

            UrlResponse response = await this.reader.GetDescription( url );

            Assert.IsTrue( response.IsValid );
            Assert.AreEqual( "My Title", response.Title );
            Assert.AreEqual( "My Title", response.TitleShortened );
        }

        [Test]
        public async void BigFileTest()
        {
            string url = string.Format( "file:///{0}", this.bigSizeFile.Replace( "\\", "/" ) );

            UrlResponse response = await this.reader.GetDescription( url );

            Assert.IsFalse( response.IsValid );
        }
    }
}
