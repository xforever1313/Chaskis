//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Chaskis.Plugins.MeetBot;
using Moq;
using NUnit.Framework;
using SethCS.Extensions;

namespace Chaskis.UnitTests.PluginTests.Plugins.MeetBot
{
    public class GeneratorConfigTests
    {
        // ---------------- Fields ----------------

        private static readonly string meetbotPath = Path.Combine( "Chaskis", "Plugins", "MeetBot" );
        private static readonly string defaultNotesOutputFolder = Path.Combine( meetbotPath, "notes" );
        private static readonly string defaultTemplateFolder = Path.Combine( meetbotPath, "templates" );

        private const string meetingName = "My  Meeting   Name";
        private const string meetingNameUnderscored = "My_Meeting_Name";
        private static readonly DateTime timestamp = new DateTime( 2020, 8, 7, 12, 13, 14, 999 );
        private const string channel = "#chaskis";

        private Mock<IMeetingInfo> mockMeetingInfo;

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.mockMeetingInfo = new Mock<IMeetingInfo>( MockBehavior.Strict );
         
            this.mockMeetingInfo.Setup(
                m => m.Channel
            ).Returns( channel );

            this.mockMeetingInfo.Setup(
                m => m.MeetingTopic
            ).Returns( meetingName );

            this.mockMeetingInfo.Setup(
                m => m.StartTime
            ).Returns( timestamp );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        [Test]
        public void DefaultSettingTest()
        {
            GeneratorConfig uut = new GeneratorConfig( meetbotPath );

            // Type is unknown; it must be overridden
            Assert.AreEqual( MeetingNotesGeneratorType.Unknown, uut.Type );

            // Channels are defaulted to all channels
            Assert.AreEqual( 0, uut.Channels.Count );

            // Templates set to null to represent using the default template path.
            Assert.IsNull( uut.TemplatePath );

            // Output is equal to the meet bot root, and the specified channel in the notes folder.
            Assert.AreEqual(
                Path.Combine( defaultNotesOutputFolder, "{%channel%}" ),
                uut.Output
            );

            // Filename is the topic, timestamp, and generator type; but as vars.
            Assert.AreEqual(
                "{%meetingtopic%}-{%timestamp%}.{%generatortype%}",
                uut.FileName
            );

            // Yes, use UTC by default.
            Assert.AreEqual(
                true,
                uut.TimeStampUseUtc
            );

            // Culture is invarient.
            Assert.AreEqual(
                CultureInfo.InvariantCulture,
                uut.TimeStampCulture
            );

            // Timestamp format is our DateTime one.
            Assert.AreEqual(
                "yyyy-MM-dd_HH-mm-ss-ffff",
                uut.TimeStampFormat
            );

            // Null for no action
            Assert.IsNull( uut.PostSaveAction );

            // Null for default save message
            Assert.IsNull( uut.PostSaveMessage );
        }

        /// <summary>
        /// Ensures we get the correct template, depending on our settings.
        /// </summary>
        [Test]
        public void GetTemplatePathTests()
        {
            GeneratorConfig uut = new GeneratorConfig( meetbotPath );

            // Unknown should get an exception.
            uut.Type = MeetingNotesGeneratorType.Unknown;
            uut.TemplatePath = null;
            Assert.Throws<InvalidOperationException>( () => uut.GetTemplatePath() );

            // XML does not have a template, it returns null.
            uut.Type = MeetingNotesGeneratorType.xml;
            uut.TemplatePath = null;
            Assert.IsNull( uut.GetTemplatePath() );

            // HTML returns a cshtml file.
            uut.Type = MeetingNotesGeneratorType.html;
            uut.TemplatePath = null;
            Assert.AreEqual(
                Path.Combine( defaultTemplateFolder, "default.cshtml" ),
                uut.GetTemplatePath()
            );

            // txt returns a cstxt file.
            uut.Type = MeetingNotesGeneratorType.txt;
            uut.TemplatePath = null;
            Assert.AreEqual(
                Path.Combine( defaultTemplateFolder, "default.cstxt" ),
                uut.GetTemplatePath()
            );

            // Otherwise, regardless of the type, return the template path.
            string templatePath = Path.Combine( meetbotPath, "here", "there", "everywhere.cssomething" );
            foreach( MeetingNotesGeneratorType type in Enum.GetValues( typeof( MeetingNotesGeneratorType ) ) )
            {
                uut.Type = type;
                uut.TemplatePath = templatePath;
                Assert.AreEqual(
                    templatePath,
                    uut.GetTemplatePath()
                );
            }
        }

        [Test]
        public void GetFileNameTest()
        {
            GeneratorConfig uut = new GeneratorConfig( meetbotPath )
            {
                FileName = "{%meetingtopic%}-{%timestamp%}.{%generatortype%}",
                Type = MeetingNotesGeneratorType.html
            };

            string expectedString = $"{meetingNameUnderscored}-{timestamp.ToFileNameString()}.html";
            string actualString = uut.GetFileName( this.mockMeetingInfo.Object );

            Assert.AreEqual( expectedString, actualString );

            Assert.AreEqual(
                Path.Combine( meetbotPath, expectedString ),
                uut.GetFullOutputPath( this.mockMeetingInfo.Object )
            );
        }
    }
}
