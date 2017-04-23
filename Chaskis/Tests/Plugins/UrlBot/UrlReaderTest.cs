//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Threading.Tasks;
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
        public void GoodFileTest()
        {
            string url;

            // On UNIX systems, need to strip out one '/' since it adds one as the root directory.
            if ( Environment.OSVersion.Platform == PlatformID.Unix )
            {
                url = string.Format( "file://{0}", this.goodSizeFile.Replace( "\\", "/" ) );
            }
            else
            {
                url = string.Format( "file:///{0}", this.goodSizeFile.Replace( "\\", "/" ) );
            }

            Task<UrlResponse> response = this.reader.GetDescription( url );
            response.Wait();

            Assert.IsTrue( response.Result.IsValid );
            Assert.AreEqual( "My Title", response.Result.Title );
            Assert.AreEqual( "My Title", response.Result.TitleShortened );
        }

        [Test]
        public void BigFileTest()
        {
            string url;

            // On UNIX systems, need to strip out one '/' since it adds one as the root directory.
            if ( Environment.OSVersion.Platform == PlatformID.Unix )
            {
                url = string.Format( "file://{0}", this.bigSizeFile.Replace( "\\", "/" ) );
            }
            else
            {
                url = string.Format( "file:///{0}", this.bigSizeFile.Replace( "\\", "/" ) );
            }

            Task<UrlResponse> response = this.reader.GetDescription( url );
            response.Wait();
            Assert.IsFalse( response.Result.IsValid );
        }
    }
}
