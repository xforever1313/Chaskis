//
//          Copyright Seth Hendrick 2016-2021.
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
using Chaskis.UnitTests.Common;
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
                FileName = "{%meetbotroot%}{%meetingtopic%}-{%channel%}-{%timestamp%}.{%generatortype%}",
                Type = MeetingNotesGeneratorType.html
            };

            // {%meetbotroot%} should NOT be replaced for the file name.
            string expectedString = $"{{%meetbotroot%}}{meetingNameUnderscored}-{channel}-{timestamp.ToFileNameString()}.html";
            string actualString = uut.GetFileName( this.mockMeetingInfo.Object );

            Assert.AreEqual( expectedString, actualString );

            Assert.AreEqual(
                Path.Combine( meetbotPath, expectedString ),
                uut.GetFullOutputPath( this.mockMeetingInfo.Object )
            );
        }

        [Test]
        public void GetPostSaveActionTest()
        {
            GeneratorConfig uut = new GeneratorConfig( meetbotPath )
            {
                FileName = "{%meetbotroot%}{%meetingtopic%}-{%channel%}-{%timestamp%}.{%generatortype%}",
                PostSaveAction = null,
                Type = MeetingNotesGeneratorType.txt
            };

            // If the post save action setting is null, we expect null
            // to return, to show that we want no action to be performed.
            Assert.IsNull( uut.GetPostSaveAction( this.mockMeetingInfo.Object ) );

            uut.PostSaveAction = "cp {%fullfilepath%} {%meetbotroot%}/{%channel%}/notes";

            // {%meetbotroot%} should NOT be replaced for the file name.
            string expectedFileName = $"{{%meetbotroot%}}{meetingNameUnderscored}-{channel}-{timestamp.ToFileNameString()}.txt";
            string expectedCommand = $"cp {Path.Combine( meetbotPath, expectedFileName )} {meetbotPath}/{channel}/notes";

            Assert.AreEqual( expectedCommand, uut.GetPostSaveAction( mockMeetingInfo.Object ) );
        }

        [Test]
        public void GetPostSaveMessageTest()
        {
            GeneratorConfig uut = new GeneratorConfig( meetbotPath )
            {
                FileName = "{%meetingtopic%}-{%channel%}-{%timestamp%}.{%generatortype%}",
                PostSaveMessage = null,
                Type = MeetingNotesGeneratorType.txt
            };

            // If the post save action setting is null, we expect null
            // to return, to show that we want to send the default message.
            Assert.IsNull( uut.GetPostSaveMessage( this.mockMeetingInfo.Object ) );

            uut.PostSaveMessage = "Notes have been posted to http://somewhere/notes/{%channel%}/{%filename%}";

            // {%meetbotroot%} should NOT be replaced for the file name.
            string expectedFileName = $"{meetingNameUnderscored}-{channel}-{timestamp.ToFileNameString()}.txt";
            string expectedCommand = $"Notes have been posted to http://somewhere/notes/{channel}/{expectedFileName}";

            Assert.AreEqual( expectedCommand, uut.GetPostSaveMessage( mockMeetingInfo.Object ) );
        }

        /// <summary>
        /// Not having optional things should validate.
        /// </summary>
        [Test]
        public void ValidateOptionalNotSpecifiedTest()
        {
            GeneratorConfig uut = GetValidConfig();
            uut.Channels.Clear();
            uut.PostSaveAction = null;
            uut.PostSaveMessage = null;
            Assert.AreEqual( 0, uut.TryValidate().Count );
        }

        /// <summary>
        /// Filling in optional things should still validate.
        /// </summary>
        [Test]
        public void ValidateOptionalSpecifiedTest()
        {
            GeneratorConfig uut = GetValidConfig();
            uut.Channels.Add( channel );
            uut.PostSaveAction = "save action";
            uut.PostSaveMessage = "save message";
            Assert.AreEqual( 0, uut.TryValidate().Count );

        }

        /// <summary>
        /// Unknown type should not validate, all others should.
        /// </summary>
        [Test]
        public void TypeValidationTest()
        {
            GeneratorConfig uut = GetValidConfig();

            foreach( MeetingNotesGeneratorType type in Enum.GetValues( typeof( MeetingNotesGeneratorType ) ) )
            {
                uut.Type = type;
                if( type == MeetingNotesGeneratorType.Unknown )
                {
                    Assert.AreEqual( 1, uut.TryValidate().Count );
                }
                else
                {
                    Assert.AreEqual( 0, uut.TryValidate().Count );
                }
            }
        }

        /// <summary>
        /// Invalid channels should not validate.
        /// </summary>
        [Test]
        public void ValidateInvalidChannelsTest()
        { 
            // Invalid channels should not validate.
            GeneratorConfig uut = GetValidConfig();
            uut.Channels.Add( null );
            Assert.AreEqual( 1, uut.TryValidate().Count );

            uut = GetValidConfig();
            uut.Channels.Add( string.Empty );
            Assert.AreEqual( 1, uut.TryValidate().Count );

            uut = GetValidConfig();
            uut.Channels.Add( "    " );
            Assert.AreEqual( 1, uut.TryValidate().Count );
        }

        [Test]
        public void ValidateTemplateTest()
        {
            GeneratorConfig uut = GetValidConfig();
            uut.TemplatePath = null;

            // XML should validate with a null, no others should.
            foreach( MeetingNotesGeneratorType type in Enum.GetValues( typeof( MeetingNotesGeneratorType ) ) )
            {
                uut.Type = type;
                if( type != MeetingNotesGeneratorType.xml )
                {
                    Assert.AreEqual( 1, uut.TryValidate().Count );
                }
                else
                {
                    Assert.AreEqual( 0, uut.TryValidate().Count );
                }
            }

            // Meanwhile, if a template IS specified, but is not found,
            // it should not validate.

            uut.Type = MeetingNotesGeneratorType.html;
            uut.TemplatePath = "lol.cshtml";
            Assert.AreEqual( 1, uut.TryValidate().Count );
        }

        [Test]
        public void OutputValidationTest()
        {
            GeneratorConfig uut = GetValidConfig();

            uut.Output = null;
            Assert.AreEqual( 1, uut.TryValidate().Count );

            uut.Output = string.Empty;
            Assert.AreEqual( 1, uut.TryValidate().Count );

            uut.Output = "       ";
            Assert.AreEqual( 1, uut.TryValidate().Count );
        }

        [Test]
        public void FilenameValidationTest()
        {
            GeneratorConfig uut = GetValidConfig();

            uut.FileName = null;
            Assert.AreEqual( 1, uut.TryValidate().Count );

            uut.FileName = string.Empty;
            Assert.AreEqual( 1, uut.TryValidate().Count );

            uut.FileName = "       ";
            Assert.AreEqual( 1, uut.TryValidate().Count );
        }

        [Test]
        public void TimeStampCultureValidationTest()
        {
            GeneratorConfig uut = GetValidConfig();

            uut.TimeStampCulture = null;
            Assert.AreEqual( 1, uut.TryValidate().Count );
        }

        [Test]
        public void TimeStampFormatValidationTest()
        {
            GeneratorConfig uut = GetValidConfig();

            uut.TimeStampFormat = null;
            Assert.AreEqual( 1, uut.TryValidate().Count );

            uut.TimeStampFormat = string.Empty;
            Assert.AreEqual( 1, uut.TryValidate().Count );

            uut.TimeStampFormat = "       ";
            Assert.AreEqual( 1, uut.TryValidate().Count );

            // Not a real format:
            uut.TimeStampFormat = ";"; // 1 character long triggers the FormatException.
            Assert.AreEqual( 1, uut.TryValidate().Count );
        }

        // ---------------- Test Helpers ----------------

        private GeneratorConfig GetValidConfig()
        {
            GeneratorConfig uut = new GeneratorConfig( meetbotPath )
            {
                Type = MeetingNotesGeneratorType.html,
                TemplatePath = Path.Combine( TestHelpers.PluginDir, "MeetBot", "Templates", "default.cshtml" ),
            };

            return uut;
        }
    }
}
