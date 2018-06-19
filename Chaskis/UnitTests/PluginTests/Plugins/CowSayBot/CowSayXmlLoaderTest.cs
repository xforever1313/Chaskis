//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using Chaskis.Plugins.CowSayBot;
using NUnit.Framework;

namespace Tests.Plugins.CowSayBot
{
    [TestFixture]
    public class CowSayXmlLoaderTest
    {
        // -------- Fields --------

        /// <summary>
        /// Path to the xml directory with the xml cowsay configs are.
        /// </summary>
        private static readonly string TestFilesPath = Path.Combine(
            TestHelpers.PluginTestsDir, "Plugins", "CowSayBot", "TestFiles"
        );

        // -------- Tests --------

        /// <summary>
        /// Ensures a valid cowsay config works.
        /// </summary>
        [Test]
        public void GoodFileTest()
        {
            string goodFile = Path.Combine( TestFilesPath, "GoodFile.xml" );

            CowSayBotConfig config = XmlLoader.LoadCowSayBotConfig( goodFile );

            Assert.AreEqual( @"!{%saycmd%} (?<msg>.+)", config.ListenRegex );
            Assert.AreEqual( "../../Plugins/CowSayBot/TestFiles/cowsay", config.ExeCommand );
            Assert.AreEqual( 5, config.CoolDownTimeSeconds );

            Assert.AreEqual( 5, config.CowFileInfoList.CommandList.Count );
            Assert.AreEqual( "DEFAULT", config.CowFileInfoList.CommandList["cowsay"] );
            Assert.AreEqual( "vader", config.CowFileInfoList.CommandList["vadersay"] );
            Assert.AreEqual( "tux", config.CowFileInfoList.CommandList["tuxsay"] );
            Assert.AreEqual( "moose", config.CowFileInfoList.CommandList["moosesay"] );
            Assert.AreEqual( "moofasa", config.CowFileInfoList.CommandList["lionsay"] );
        }

        /// <summary>
        /// Ensures a missing file results in a FileNotFoundException.
        /// </summary>
        [Test]
        public void FileNotFoundTest()
        {
            Assert.Throws<FileNotFoundException>( () => XmlLoader.LoadCowSayBotConfig( "derp.xml" ) );
        }
    }
}