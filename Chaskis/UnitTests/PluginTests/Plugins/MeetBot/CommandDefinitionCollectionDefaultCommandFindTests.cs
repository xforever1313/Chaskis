//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chaskis.Plugins.MeetBot;
using NUnit.Framework;
using SethCS.Basic;

namespace Chaskis.UnitTests.PluginTests.Plugins.MeetBot
{
    [TestFixture]
    public class CommandDefinitionCollectionDefaultCommandFindTests
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

            IList<CommandDefinition> defs = this.xmlLoader.ParseDefaultFile();
            this.collection = new CommandDefinitionCollection( defs );

            // Ensure our collection validates.
            Assert.DoesNotThrow( () => collection.InitStage1_ValidateDefinitions() );
            Assert.DoesNotThrow( () => collection.InitStage2_FilterOutOverrides() );
            Assert.DoesNotThrow( () => collection.InitStage3_BuildDictionaries() );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        [Test]
        public void StartMeetingTest()
        {
            DoFindCommandTest(
                collection,
                "#startmeeting My Meeting",
                MeetingAction.StartMeeting,
                "#startmeeting",
                "My Meeting"
            );
        }

        [Test]
        public void EndMeetingTest()
        {
            DoFindCommandTest(
                collection,
                "#endmeeting",
                MeetingAction.EndMeeting,
                "#endmeeting",
                string.Empty
            );
        }

        [Test]
        public void TopicTest()
        {
            DoFindCommandTest(
                collection,
                "#topic New Topic",
                MeetingAction.Topic,
                "#topic",
                "New Topic"
            );
        }

        [Test]
        public void AgreeTest()
        {
            // Good test since #agree overlaps with #agreed.

            DoFindCommandTest(
                collection,
                "#agree  We should do this thing!",
                MeetingAction.Agree,
                "#agree",
                "We should do this thing!"
            );

            DoFindCommandTest(
                collection,
                "#agreed",
                MeetingAction.Agree,
                "#agreed",
                string.Empty
            );
        }

        [Test]
        public void ChairTest()
        {
            DoFindCommandTest(
                collection,
                "#chair xforever1313",
                MeetingAction.Chair,
                "#chair",
                "xforever1313"
            );
        }

        [Test]
        public void UnChairTest()
        {
            DoFindCommandTest(
                collection,
                "#unchair xforever1313 chaskisbot",
                MeetingAction.Unchair,
                "#unchair",
                "xforever1313 chaskisbot"
            );
        }

        [Test]
        public void ActionTest()
        {
            DoFindCommandTest(
                collection,
                "#action Sleep",
                MeetingAction.Action,
                "#action",
                "Sleep"
            );
        }

        [Test]
        public void InfoTest()
        {
            DoFindCommandTest(
                collection,
                "#info C# is better than Java.",
                MeetingAction.Info,
                "#info",
                "C# is better than Java."
            );
        }

        [Test]
        public void LinkTest()
        {
            DoFindCommandTest(
                collection,
                "#link https://shendrick.net",
                MeetingAction.Link,
                "#link",
                "https://shendrick.net"
            );
        }

        [Test]
        public void UnlinkTest()
        {
            DoFindCommandTest(
                collection,
                "#unlink https://github.com",
                MeetingAction.Unlink,
                "#unlink",
                "https://github.com"
            );
        }

        [Test]
        public void MeetingTopicTest()
        {
            DoFindCommandTest(
                collection,
                "#meetingtopic Some Topic",
                MeetingAction.MeetingTopic,
                "#meetingtopic",
                "Some Topic"
            );
        }

        [Test]
        public void HelpTest()
        {
            DoFindCommandTest(
                collection,
                "#help",
                MeetingAction.Help,
                "#help",
                string.Empty
            );

            DoFindCommandTest(
                collection,
                "#halp #info",
                MeetingAction.Help,
                "#halp",
                "#info"
            );

            DoFindCommandTest(
                collection,
                "#commands",
                MeetingAction.Help,
                "#commands",
                string.Empty
            );
        }

        [Test]
        public void AcceptedTest()
        {
            DoFindCommandTest(
                collection,
                "#accepted",
                MeetingAction.Accept,
                "#accepted",
                string.Empty
            );

            DoFindCommandTest(
                collection,
                "#accept",
                MeetingAction.Accept,
                "#accept",
                string.Empty
            );
        }

        [Test]
        public void RejectedTest()
        {
            DoFindCommandTest(
                collection,
                "#rejected",
                MeetingAction.Reject,
                "#rejected",
                string.Empty
            );

            DoFindCommandTest(
                collection,
                "#reject C is better than C#",
                MeetingAction.Reject,
                "#reject",
                "C is better than C#"
            );
        }

        [Test]
        public void SaveTest()
        {
            DoFindCommandTest(
                collection,
                "#save",
                MeetingAction.Save,
                "#save",
                string.Empty
            );
        }

        [Test]
        public void CancelMeetingTest()
        {
            DoFindCommandTest(
                collection,
                "#cancelmeeting",
                MeetingAction.CancelMeeting,
                "#cancelmeeting",
                string.Empty
            );
        }

        [Test]
        public void PurgeTest()
        {
            DoFindCommandTest(
                collection,
                // Test to make sure we are good with caps.
                "#PURGE xforever1313",
                MeetingAction.Purge,
                "#PURGE",
                "xforever1313"
            );
        }

        [Test]
        public void SilenceTest()
        {
            DoFindCommandTest(
                collection,
                "#silence me",
                MeetingAction.Silence,
                "#silence",
                "me"
            );

            DoFindCommandTest(
                collection,
                "#quiet you",
                MeetingAction.Silence,
                "#quiet",
                "you"
            );
        }

        [Test]
        public void VoiceTest()
        {
            DoFindCommandTest(
                collection,
                "#VOICE me",
                MeetingAction.Voice,
                "#VOICE",
                "me"
            );

        }

        [Test]
        public void BanTest()
        {
            DoFindCommandTest(
                collection,
                "#banish spambot",
                MeetingAction.Banish,
                "#banish",
                "spambot"
            );

            DoFindCommandTest(
                collection,
                "#ban spambot2",
                MeetingAction.Banish,
                "#ban",
                "spambot2"
            );
        }

        // ---------------- Test Helpers ----------------

        private void DoFindCommandTest(
            CommandDefinitionCollection list,
            string fullCommand,
            MeetingAction expectedAction,
            string expectedPrefix,
            string expectedArgs
        )
        {
            CommandDefinitionFindResult foundDef = list.Find( fullCommand );

            // Ensure only one command is returned, or we have duplicates.
            Assert.IsNotNull( foundDef );

            Assert.AreEqual( expectedPrefix, foundDef.CommandPrefix );
            Assert.AreEqual( expectedArgs, foundDef.CommandArgs );
            Assert.AreEqual( expectedAction, foundDef.FoundDefinition.MeetingAction );

            // Check dictionaries for sameness.
            Assert.AreSame(
                list.MeetingActionToCommandDef[foundDef.FoundDefinition.MeetingAction],
                foundDef.FoundDefinition
            );

            Regex foundRegex = list.MeetingActionToRegex[foundDef.FoundDefinition.MeetingAction];
            Assert.AreSame(
                list.RegexToCommandDef[foundRegex],
                foundDef.FoundDefinition
            );
        }
    }
}
