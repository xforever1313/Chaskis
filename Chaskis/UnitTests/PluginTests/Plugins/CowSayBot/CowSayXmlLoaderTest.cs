//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using Chaskis.Plugins.CowSayBot;
using Chaskis.UnitTests.Common;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.CowSayBot
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

        private static readonly string cowsayLocation = Path.Combine(
            TestFilesPath,
            "cowsay"
        );

        private static readonly string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<cowsaybotconfig>
    <command><![CDATA[!{%saycmd%} (?<msg>.+)]]></command>
    <path>" + cowsayLocation + @"</path>
    <cowsaycooldown>5</cowsaycooldown>
    <cowfiles>
        <cowfile command = ""cowsay"" name=""DEFAULT"" />
        <cowfile command = ""vadersay"" name=""vader"" />
        <cowfile command = ""tuxsay"" name=""tux"" />
        <cowfile command = ""moosesay"" name=""moose"" />
        <cowfile command = ""lionsay"" name=""moofasa"" />
    </cowfiles>
</cowsaybotconfig>
";

        // -------- Tests --------

        /// <summary>
        /// Ensures a valid cowsay config works.
        /// </summary>
        [Test]
        public void GoodFileTest()
        {
            string goodFile = Path.Combine( TestContext.CurrentContext.TestDirectory, "GoodFile.xml" );

            try
            {
                // Need to do this since we need a run-time value for the exe path.
                File.WriteAllText( goodFile, xmlString );

                CowSayBotConfig config = XmlLoader.LoadCowSayBotConfig( goodFile );

                Assert.AreEqual( @"!{%saycmd%} (?<msg>.+)", config.ListenRegex );
                Assert.AreEqual( cowsayLocation, config.ExeCommand );
                Assert.AreEqual( 5, config.CoolDownTimeSeconds );

                Assert.AreEqual( 5, config.CowFileInfoList.CommandList.Count );
                Assert.AreEqual( "DEFAULT", config.CowFileInfoList.CommandList["cowsay"] );
                Assert.AreEqual( "vader", config.CowFileInfoList.CommandList["vadersay"] );
                Assert.AreEqual( "tux", config.CowFileInfoList.CommandList["tuxsay"] );
                Assert.AreEqual( "moose", config.CowFileInfoList.CommandList["moosesay"] );
                Assert.AreEqual( "moofasa", config.CowFileInfoList.CommandList["lionsay"] );
            }
            finally
            {
                if( File.Exists( goodFile ) )
                {
                    File.Delete( goodFile );
                }
            }
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