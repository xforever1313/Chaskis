//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chaskis.Plugins.MeetBot;
using NUnit.Framework;
using SethCS.Basic;

namespace Chaskis.UnitTests.PluginTests.Plugins.MeetBot
{
    [TestFixture]
    public class XmlLoadOverideCommandTests
    {
        // ---------------- Fields ----------------

        private GenericLogger logger;

        private XmlLoader xmlLoader;

        private CommandDefinitionCollection collection;

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.logger = new GenericLogger();
            this.xmlLoader = new XmlLoader( this.logger );

            string xmlString;
            using( Stream s = typeof( CommandDefinitionCollection ).Assembly.GetManifestResourceStream( "Chaskis.Plugins.MeetBot.Config.SampleCommands.xml" ) )
            {
                using( StreamReader reader = new StreamReader( s ) )
                {
                    xmlString = reader.ReadToEnd();
                }
            }

            // Replace all occurrances with !command instead of #command.
            xmlString = xmlString.Replace( "<prefix>#", "<prefix>!" );

            // Load both default and user-defined.  User-defined will override the default ones.
            IList<CommandDefinition> defaultDefs = this.xmlLoader.ParseDefaultFile();
            IList<CommandDefinition> userDefs = this.xmlLoader.ParseCommandFileFromString( xmlString );

            List<CommandDefinition> allDefs = new List<CommandDefinition>( defaultDefs );
            allDefs.AddRange( userDefs );
            this.collection = new CommandDefinitionCollection( allDefs );

            // Ensure our collection validates.
            Assert.DoesNotThrow( () => collection.InitStage1_ValidateDefinitions() );
            Assert.DoesNotThrow( () => collection.InitStage2_FilterOutOverrides() );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        [Test]
        public void AllNotDefaultTest()
        {
            // All actions here are not the default.
            // They were all overridden.
            Assert.IsTrue( collection.CommandDefinitions.All( d => d.IsDefault == false ) );
        }

        [Test]
        public void StartMeetingTest()
        {
            CommandExists(
                collection,
                "!startmeeting",
                MeetingAction.StartMeeting,
                CommandRestriction.Anyone
            );
        }

        [Test]
        public void EndMeetingTest()
        {
            CommandExists(
                collection,
                "!endmeeting",
                MeetingAction.EndMeeting,
                CommandRestriction.ChairsAndBotAdmins
            );
        }

        [Test]
        public void TopicTest()
        {
            CommandExists(
                collection,
                "!topic",
                MeetingAction.Topic,
                CommandRestriction.ChairsOnly
            );
        }

        [Test]
        public void AgreeTest()
        {
            CommandExists(
                collection,
                "!agree",
                MeetingAction.Agree,
                CommandRestriction.ChairsOnly
            );

            CommandExists(
                collection,
                "!agreed",
                MeetingAction.Agree,
                CommandRestriction.ChairsOnly
            );
        }

        [Test]
        public void ChairTest()
        {
            CommandExists(
                collection,
                "!chair",
                MeetingAction.Chair,
                CommandRestriction.ChairsOnly
            );
        }

        [Test]
        public void UnChairTest()
        {
            CommandExists(
                collection,
                "!unchair",
                MeetingAction.Unchair,
                CommandRestriction.ChairsAndBotAdmins
            );
        }

        [Test]
        public void ActionTest()
        {
            CommandExists(
                collection,
                "!action",
                MeetingAction.Action,
                CommandRestriction.Anyone
            );
        }

        [Test]
        public void InfoTest()
        {
            CommandExists(
                collection,
                "!info",
                MeetingAction.Info,
                CommandRestriction.Anyone
            );
        }

        [Test]
        public void LinkTest()
        {
            CommandExists(
                collection,
                "!link",
                MeetingAction.Link,
                CommandRestriction.Anyone
            );
        }

        [Test]
        public void UnlinkTest()
        {
            CommandExists(
                collection,
                "!unlink",
                MeetingAction.Unlink,
                CommandRestriction.Anyone
            );
        }

        [Test]
        public void MeetingTopicTest()
        {
            CommandExists(
                collection,
                "!meetingtopic",
                MeetingAction.MeetingTopic,
                CommandRestriction.ChairsOnly
            );
        }

        [Test]
        public void HelpTest()
        {
            CommandExists(
                collection,
                "!help",
                MeetingAction.Help,
                CommandRestriction.Anyone
            );

            CommandExists(
                collection,
                "!halp",
                MeetingAction.Help,
                CommandRestriction.Anyone
            );

            CommandExists(
                collection,
                "!commands",
                MeetingAction.Help,
                CommandRestriction.Anyone
            );
        }

        [Test]
        public void AcceptedTest()
        {
            CommandExists(
                collection,
                "!accepted",
                MeetingAction.Accept,
                CommandRestriction.ChairsOnly
            );

            CommandExists(
                collection,
                "!accept",
                MeetingAction.Accept,
                CommandRestriction.ChairsOnly
            );
        }

        [Test]
        public void RejectedTest()
        {
            CommandExists(
                collection,
                "!rejected",
                MeetingAction.Reject,
                CommandRestriction.ChairsOnly
            );

            CommandExists(
                collection,
                "!reject",
                MeetingAction.Reject,
                CommandRestriction.ChairsOnly
            );
        }

        [Test]
        public void SaveTest()
        {
            CommandExists(
                collection,
                "!save",
                MeetingAction.Save,
                CommandRestriction.ChairsOnly
            );
        }

        [Test]
        public void CancelMeetingTest()
        {
            CommandExists(
                collection,
                "!cancelmeeting",
                MeetingAction.CancelMeeting,
                CommandRestriction.ChairsAndBotAdmins
            );
        }

        [Test]
        public void PurgeTest()
        {
            CommandExists(
                collection,
                "!purge",
                MeetingAction.Purge,
                CommandRestriction.ChairsOnly
            );
        }

        [Test]
        public void SilenceTest()
        {
            CommandExists(
                collection,
                "!silence",
                MeetingAction.Silence,
                CommandRestriction.ChairsOnly
            );

            CommandExists(
                collection,
                "!quiet",
                MeetingAction.Silence,
                CommandRestriction.ChairsOnly
            );
        }

        [Test]
        public void VoiceTest()
        {
            CommandExists(
                collection,
                "!voice",
                MeetingAction.Voice,
                CommandRestriction.ChairsOnly
            );

        }

        [Test]
        public void BanTest()
        {
            CommandExists(
                collection,
                "!banish",
                MeetingAction.Banish,
                CommandRestriction.ChairsOnly
            );

            CommandExists(
                collection,
                "!ban",
                MeetingAction.Banish,
                CommandRestriction.ChairsOnly
            );
        }

        // ---------------- Test Helpers ----------------

        private void CommandExists(
            CommandDefinitionCollection list,
            string prefix,
            MeetingAction expectedAction,
            CommandRestriction expectedRestriction
        )
        {
            IEnumerable<CommandDefinition> defs = list.CommandDefinitions.Where( d => d.Prefixes.Contains( prefix ) );

            // Ensure only one command is returned, or we have duplicates.
            Assert.AreEqual( 1, defs.Count() );

            // Ensure everything matches what we expect.
            CommandDefinition def = defs.First();

            Assert.AreEqual( expectedAction, def.MeetingAction );
            Assert.AreEqual( expectedRestriction, def.Restriction );
            Assert.IsFalse( def.IsDefault );
            Assert.IsTrue( def.GetPrefixRegex().IsMatch( prefix ) );
        }
    }
}
