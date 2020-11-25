//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Globalization;
using System.IO;
using Chaskis.Plugins.MeetBot;
using NUnit.Framework;
using XmlLoader = Chaskis.Plugins.MeetBot.XmlLoader;

namespace Chaskis.UnitTests.PluginTests.Plugins.MeetBot
{
    [TestFixture]
    public class XmlLoaderParseMeetbotConfigTests
    {
        // ---------------- Fields ----------------

        private static readonly string meetbotPath = Path.Combine(
            "Chaskis",
            "Plugins",
            "MeetBot"
        );

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures we parse the meetbot config settings correctly.
        /// Generator settings have a more indepth test later.
        /// </summary>
        [Test]
        public void ParseMeetbotConfig1Test()
        {
            const string xml =
@"
<meetbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/meetbot/2020/MeetBotConfig.xsd"">
    <commandconfig>{%meetbotroot%}/Commands.xml</commandconfig>
    <enablebackups>true</enablebackups>
    <generators>
        <generator type=""xml"">
        </generator>
    </generators>
</meetbotconfig>
";
            XmlLoader uut = new XmlLoader( null );
            MeetBotConfig parsedConfig = uut.ParseConfigAsString( xml, meetbotPath );

            Assert.AreEqual( "{%meetbotroot%}/Commands.xml", parsedConfig.CommandConfigPath );
            Assert.AreEqual( $"{meetbotPath}/Commands.xml", parsedConfig.GetCommandConfigPath() );
            Assert.IsTrue( parsedConfig.EnableBackups );
            Assert.AreEqual( 1, parsedConfig.Generators.Count );

            GeneratorConfig parsedGeneratorConfig = parsedConfig.Generators[0];

            // Everything else should be defaulted.
            GeneratorConfig defaultConfig = new GeneratorConfig( meetbotPath );
            defaultConfig.Type = MeetingNotesGeneratorType.xml;
            CompareGeneratorConfig( defaultConfig, parsedGeneratorConfig );
        }

        /// <summary>
        /// Ensures we parse the meetbot config settings correctly.
        /// Generator settings have a more indepth test later.
        /// </summary>
        [Test]
        public void ParseMeetbotConfig2Test()
        {
            const string xml =
@"
<meetbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/meetbot/2020/MeetBotConfig.xsd"">
    <enablebackups>false</enablebackups>
    <generators>
        <generator type=""xml"">
            <channel>
                #chaskis
            </channel>
            <channel>
                #rit
            </channel>
            <output>
                {%meetbotroot%}/notes/{%channel%}/
            </output>
            <filename>
                <timestamp utc=""true"" culture=""en-US"">
                    yyyy-MM-dd_HH-mm-ss-ffff
                </timestamp>
                <name>
                    {%meetingtopic%}-{%timestamp%}.{%generatortype%}
                </name>
            </filename>
            <postsaveaction>
                chmod 644 {%fullfilepath%}
            </postsaveaction>
            <postsavemsg>
                Meeting Notes Saved, view them at https://files.shendrick.net/meetingnotes/{%filename%}
            </postsavemsg>
        </generator>
        <generator type=""html"">
            <templatepath>{%meetbotroot%}/templates/default.cshtml</templatepath>
            <output>{%meetbotroot%}/notes/{%channel%}/</output>
            <filename>
                <timestamp utc=""false"">yyyy</timestamp>
                <name>{%meetingtopic%}-{%timestamp%}.{%generatortype%}</name>
            </filename>
        </generator>
    </generators>
</meetbotconfig>
";
            XmlLoader uut = new XmlLoader( null );
            MeetBotConfig parsedConfig = uut.ParseConfigAsString( xml, meetbotPath );

            Assert.IsNull( parsedConfig.CommandConfigPath );
            Assert.IsNull( parsedConfig.GetCommandConfigPath() );
            Assert.IsFalse( parsedConfig.EnableBackups );
            Assert.AreEqual( 2, parsedConfig.Generators.Count );

            // Compare 1st generator
            {
                GeneratorConfig expected = new GeneratorConfig( meetbotPath )
                {
                    FileName = "{%meetingtopic%}-{%timestamp%}.{%generatortype%}",
                    Output = "{%meetbotroot%}/notes/{%channel%}/",
                    PostSaveAction = "chmod 644 {%fullfilepath%}",
                    PostSaveMessage = "Meeting Notes Saved, view them at https://files.shendrick.net/meetingnotes/{%filename%}",
                    TemplatePath = null,
                    TimeStampCulture = new CultureInfo( "en-US" ),
                    TimeStampFormat = "yyyy-MM-dd_HH-mm-ss-ffff",
                    TimeStampUseUtc = true,
                    Type = MeetingNotesGeneratorType.xml
                };

                CompareGeneratorConfig( expected, parsedConfig.Generators[0] );

                Assert.AreEqual( 2, parsedConfig.Generators[0].Channels.Count );
                Assert.IsTrue( parsedConfig.Generators[0].Channels.Contains( "#chaskis" ) );
                Assert.IsTrue( parsedConfig.Generators[0].Channels.Contains( "#rit" ) );
            }

            // Compare 2nd generator
            {
                GeneratorConfig expected = new GeneratorConfig( meetbotPath )
                {
                    FileName = "{%meetingtopic%}-{%timestamp%}.{%generatortype%}",
                    Output = "{%meetbotroot%}/notes/{%channel%}/",
                    TemplatePath = "{%meetbotroot%}/templates/default.cshtml",
                    TimeStampFormat = "yyyy",
                    TimeStampUseUtc = false,
                    Type = MeetingNotesGeneratorType.html
                };

                CompareGeneratorConfig( expected, parsedConfig.Generators[1] );
            }
        }

        // ---------------- Test Helpers ----------------

        private void CompareGeneratorConfig( GeneratorConfig expectedConfig, GeneratorConfig actualConfig )
        {
            Assert.AreEqual( expectedConfig.Type, actualConfig.Type );
            Assert.AreEqual( expectedConfig.FileName, actualConfig.FileName );
            Assert.AreEqual( expectedConfig.Output, actualConfig.Output );
            Assert.AreEqual( expectedConfig.PostSaveAction, actualConfig.PostSaveAction );
            Assert.AreEqual( expectedConfig.PostSaveMessage, actualConfig.PostSaveMessage );
            Assert.AreEqual( expectedConfig.TemplatePath, actualConfig.TemplatePath );
            Assert.AreEqual( expectedConfig.TimeStampCulture, actualConfig.TimeStampCulture );
            Assert.AreEqual( expectedConfig.TimeStampFormat, actualConfig.TimeStampFormat );
            Assert.AreEqual( expectedConfig.TimeStampUseUtc, actualConfig.TimeStampUseUtc );
        }
    }
}
