//
//          Copyright Seth Hendrick 2017-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Threading.Tasks;
using Chaskis.Plugins.UrlBot;
using Chaskis.UnitTests.Common;
using NUnit.Framework;
using SethCS.Basic;
using SethCS.Extensions;

namespace Chaskis.UnitTests.PluginTests.Plugins.UrlBot
{
    [TestFixture]
    public class UrlReaderTest
    {
        // ---------------- Fields ----------------

        private UrlReader reader;

        private GenericLogger logger;

        private string urlTestFiles;

        // Don't use file:// for these files because:
        // 1. We no longer suppor that.
        // 2. file://'s HEAD method returns the entire file, which does not emulate what a real HTTP server does.

        private const string goodSizeFile = "https://files.shendrick.net/projects/chaskis/testfiles/urlbot/goodsize.html";
        private const string bigSizeFile = "https://files.shendrick.net/projects/chaskis/testfiles/urlbot/bigsize.html";
        private const string escapedCharactersFile = "https://files.shendrick.net/projects/chaskis/testfiles/urlbot/escapedcharacters.html";

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.urlTestFiles = Path.GetFullPath(
                Path.Combine(
                    TestHelpers.PluginTestsDir,
                    "Plugins",
                    "UrlBot",
                    "TestFiles"
                )
            );
        }

        [SetUp]
        public void TestSetup()
        {
            this.logger = new GenericLogger();
            this.logger.OnWriteLine += ( s => Console.WriteLine( s ) );
            this.logger.OnErrorWriteLine += ( s => Console.WriteLine( s ) );

            this.reader = new UrlReader( this.logger, PluginTestHelpers.HttpClient );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        [Test]
        public void GoodFileTest()
        {
            string url = goodSizeFile;

            Task<UrlResponse> response = this.reader.AsyncGetDescription( url );
            response.Wait();

            Assert.IsTrue( response.Result.IsValid );
            Assert.AreEqual( "My Title", response.Result.Title );
            Assert.AreEqual( "My Title", response.Result.TitleShortened );
        }

        [Test]
        public void BigFileTest()
        {
            string url = bigSizeFile;

            Task<UrlResponse> response = this.reader.AsyncGetDescription( url );
            response.Wait();
            Assert.IsFalse( response.Result.IsValid );
        }

        [Test]
        public void EscapedCharactersTest()
        {
            string url = escapedCharactersFile;

            Task<UrlResponse> response = this.reader.AsyncGetDescription( url );
            response.Wait();

            Assert.IsTrue( response.Result.IsValid );
            Assert.AreEqual( @"<My ""Title"">", response.Result.Title );
            Assert.AreEqual( @"<My ""Title"">", response.Result.TitleShortened );
        }

        /// <summary>
        /// Ensure the file:/// is ignored.
        /// Don't want someone accessing files on our server... that would
        /// be a problem.
        /// </summary>
        [Test]
        public void IgnoreFileProtocol()
        {
            string localGoodSizeFile = Path.Combine( urlTestFiles, "GoodSize.html" );
            string url = SethPath.ToUri( localGoodSizeFile );

            string outStr;
            Assert.IsFalse( UrlReader.TryParseUrl( url, out outStr ) );
            Assert.IsEmpty( outStr );
        }
    }
}
