//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using Chaskis.Plugins.MeetBot;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.MeetBot
{
    [TestFixture]
    public class MeetingTests
    {
        // ---------------- Fields ----------------

        private const string user1 = "someuser";
        private const string user2 = "someotheruser";
        private const string adminName = "botadmin";
        private const string channel = "#channel";
        private const string meetingTopic = "My Topic";
        private const string owner = "owner";

        private Mock<IMeetingInfo> meetingInfo;
        private List<string> botAdmins;
        private DateTime startTime;
        private DateTime testTime;

        private Meeting uut;

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.meetingInfo = new Mock<IMeetingInfo>( MockBehavior.Strict );
            this.meetingInfo.Setup(
                m => m.Channel
            ).Returns( channel );
            this.meetingInfo.Setup(
                m => m.MeetingTopic
            ).Returns( meetingTopic );
            this.meetingInfo.Setup(
                m => m.Owner
            ).Returns( owner );

            this.meetingInfo.Setup(
                m => m.StartTime
            ).Returns( this.startTime );

            this.botAdmins = new List<string> { adminName };
            this.startTime = DateTime.UtcNow;
            this.testTime = this.startTime + new TimeSpan( 0, 5, 0 );

            this.uut = new Meeting( this.meetingInfo.Object, this.botAdmins );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures when a meeting is created, we get the correct things set.
        /// </summary>
        [Test]
        public void ConstructionTest()
        {
            Assert.AreEqual( 0, uut.MeetingNotes.Count );
            EnsureDefaultLists();
        }

        // -------- Standard Message --------

        [Test]
        public void StandardMessageTest()
        {
            AddAndCheckMessage(
                "Hello, World!",
                1,
                user1
            );

            EnsureDefaultLists();
        }

        // -------- Chair / Unchair --------

        /// <summary>
        /// Can we chair 1 user at a time?
        /// </summary>
        [Test]
        public void ChairUnchair1PersonTest()
        {
            // Chair 1 user.
            {
                const string prefix = "#chair";
                const string args = user1;
                const MeetingAction action = MeetingAction.Chair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                // Owner is chair by default.
                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should be a successful parse.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 1, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // New chair should show up in list.
                Assert.AreEqual( 2, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );
                Assert.IsTrue( uut.Chairs.Contains( user1 ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }

            // Ensure chair can not unchair owner.
            {
                const string prefix = "#unchair";
                const string args = owner;
                const MeetingAction action = MeetingAction.Unchair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, user1, this.testTime );

                // Should have correct error message and added to list.
                Assert.AreEqual( ParseMessageResult.CanNotDoThisToOwner, result );
                Assert.AreEqual( 2, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), user1 );

                // Chair list should not be modified.
                Assert.AreEqual( 2, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );
                Assert.IsTrue( uut.Chairs.Contains( user1 ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }

            // Ensure nonchair can not unchair chair
            {
                const string prefix = "#unchair";
                const string args = user1;
                const MeetingAction action = MeetingAction.Unchair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, user2, this.testTime );

                // Should have correct error message and added to list.
                Assert.AreEqual( ParseMessageResult.ChairOnlyCommand, result );
                Assert.AreEqual( 3, uut.MeetingNotes.Count );

                // Invalid permission, meeting message becomes standard message so it
                // doesn't show up in the log as something special.
                IReadOnlyMeetingNote note = uut.MeetingNotes.Last();
                Assert.AreEqual( MeetingAction.Unknown, note.MeetingAction );
                Assert.AreEqual( $"{msg.CommandPrefix} {msg.CommandArgs}", note.Message );
                Assert.AreEqual( user2, note.UserName );

                // Chair list should not be modified.
                Assert.AreEqual( 2, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );
                Assert.IsTrue( uut.Chairs.Contains( user1 ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }

            // Ensure owner can unchair chair.
            {
                const string prefix = "#unchair";
                const string args = user1;
                const MeetingAction action = MeetingAction.Unchair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should have correct error message and added to list.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 4, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // User should no longer be a chair.
                Assert.AreEqual( 1, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }
        }

        /// <summary>
        /// Can we chair multiple users at a time?
        /// </summary>
        [Test]
        public void ChairUnchair2PeopleTest()
        {
            // Chair 1 user.
            {
                const string prefix = "#chair";
                string args = $"{user1} {user2.ToUpper()}";
                const MeetingAction action = MeetingAction.Chair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                // Owner is chair by default.
                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should be a successful parse.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 1, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // New chair should show up in list.
                Assert.AreEqual( 3, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );
                Assert.IsTrue( uut.Chairs.Contains( user1 ) );
                Assert.IsTrue( uut.Chairs.Contains( user2 ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }

            // Ensure owner can unchair multiple chairs.
            {
                const string prefix = "#unchair";
                string args = $"{user1} {user2.ToUpper()}";
                const MeetingAction action = MeetingAction.Unchair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should have correct error message and added to list.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 2, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // User should no longer be a chair.
                Assert.AreEqual( 1, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }
        }

        /// <summary>
        /// If we chair someone twice, or unchair someone twice,
        /// are we still in a good state?
        /// </summary>
        [Test]
        public void ChairUnchairSameUserTest()
        {
            // Chair 1 user.
            {
                const string prefix = "#chair";
                const string args = user1;
                const MeetingAction action = MeetingAction.Chair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                // Owner is chair by default.
                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should be a successful parse.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 1, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // New chair should show up in list.
                Assert.AreEqual( 2, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );
                Assert.IsTrue( uut.Chairs.Contains( user1 ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }

            // Chair 1 user again!
            {
                const string prefix = "#chair";
                const string args = user1;
                const MeetingAction action = MeetingAction.Chair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                // Owner is chair by default.
                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should be a successful parse.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 2, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // New chair should show up in list, but only once!
                Assert.AreEqual( 2, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );
                Assert.IsTrue( uut.Chairs.Contains( user1 ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }

            {
                const string prefix = "#unchair";
                const string args = user1;
                const MeetingAction action = MeetingAction.Unchair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should have correct error message and added to list.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 3, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // User should no longer be a chair.
                Assert.AreEqual( 1, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }

            {
                const string prefix = "#unchair";
                const string args = user1;
                const MeetingAction action = MeetingAction.Unchair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should have correct error message and added to list.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 4, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // User should no longer be a chair.
                Assert.AreEqual( 1, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }
        }

        /// <summary>
        /// Ensures the owner can't accidently unchair themselves.
        /// </summary>
        [Test]
        public void OwnerCantUnchairThemselfTest()
        {
            // Ensure owner can unchair chair.
            {
                const string prefix = "#unchair";
                const string args = owner;
                const MeetingAction action = MeetingAction.Unchair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should have correct error message and added to list.
                Assert.AreEqual( ParseMessageResult.CanNotDoThisToOwner, result );
                Assert.AreEqual( 1, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // Should still be a chair
                Assert.AreEqual( 1, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }
        }

        /// <summary>
        /// A chair is able to unchair themselves.
        /// </summary>
        [Test]
        public void ChairCanUnchairThemSelfTest()
        {
            // Chair 1 user.
            {
                const string prefix = "#chair";
                const string args = user1;
                const MeetingAction action = MeetingAction.Chair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                // Owner is chair by default.
                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should be a successful parse.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 1, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                // New chair should show up in list.
                Assert.AreEqual( 2, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );
                Assert.IsTrue( uut.Chairs.Contains( user1 ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }

            // Unchair themself.
            {
                const string prefix = "#unchair";
                const string args = user1;
                const MeetingAction action = MeetingAction.Unchair;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                // Owner is chair by default.
                ParseMessageResult result = this.uut.ParseMessage( msg, user1, this.testTime );

                // Should be a successful parse.
                Assert.AreEqual( ParseMessageResult.Success, result );
                Assert.AreEqual( 2, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), user1 );

                // No longer should appear in list.
                Assert.AreEqual( 1, uut.Chairs.Count() );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );

                // Other lists should be empty
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 0, uut.BannedUsers.Count );
            }
        }

        /// <summary>
        /// Ensures if there is extra whitespace in the chair command,
        /// we don't trip all over it.
        /// </summary>
        [Test]
        public void ChairWithSeveralWhitespacesTest()
        {
            const string prefix = "#chair";
            string args = $"  {user1}      {user2}    ";
            const MeetingAction action = MeetingAction.Chair;
            const CommandRestriction restrict = CommandRestriction.ChairsOnly;

            IMeetingMessage msg = MakeMessage(
                prefix,
                args,
                action,
                restrict
            );

            // Owner is chair by default.
            ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

            // Should be a successful parse.
            Assert.AreEqual( ParseMessageResult.Success, result );
            Assert.AreEqual( 1, uut.MeetingNotes.Count );
            CompareNotes( msg, uut.MeetingNotes.Last(), owner );

            // New chair should show up in list.
            Assert.AreEqual( 3, uut.Chairs.Count() );
            Assert.IsTrue( uut.Chairs.Contains( owner ) );
            Assert.IsTrue( uut.Chairs.Contains( user1 ) );
            Assert.IsTrue( uut.Chairs.Contains( user2 ) );

            // Other lists should be empty
            Assert.AreEqual( 0, uut.SilencedUsers.Count );
            Assert.AreEqual( 0, uut.BannedUsers.Count );
        }

        // -------- Purge / Banish Tests --------

        [Test]
        public void PurgeUserTest()
        {
            // Add some messages

            AddAndCheckMessage(
                "Hello World 1",
                1,
                user1
            );

            AddAndCheckMessage(
                "Hello World 2",
                2,
                user1
            );

            // Purge the user.
            {
                const string prefix = "#purge";
                const string args = user1;
                const MeetingAction action = MeetingAction.Purge;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );

                // Should be a successful parse.
                Assert.AreEqual( ParseMessageResult.Success, result );

                // Only 1 message should remain; the purge one.
                Assert.AreEqual( 1, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );

                EnsureDefaultLists();
            }

            // User should still be able to chat though.  Should be 2 messages now
            // (new message + purge message).

            AddAndCheckMessage(
                "Hello World 2",
                2,
                user1
            );
        }

        [Test]
        public void BanUserTest()
        {
            // Add some messages

            AddAndCheckMessage(
                "Hello World 1",
                1,
                user1
            );

            AddAndCheckMessage(
                "Hello World 2",
                2,
                user1
            );

            void CheckLists()
            {
                // 1 user should be added to the banned users.
                Assert.AreEqual( 1, uut.BannedUsers.Count );
                Assert.IsTrue( uut.BannedUsers.Contains( user1 ) );

                // 1 Person should still be in the chairs, no one should be silenced.
                Assert.AreEqual( 0, uut.SilencedUsers.Count );
                Assert.AreEqual( 1, uut.Chairs.Count );
                Assert.IsTrue( uut.Chairs.Contains( owner ) );
            }

            // Ban the user.
            IMeetingMessage banMsg;
            {
                const string prefix = "#ban";
                const string args = user1;
                const MeetingAction action = MeetingAction.Banish;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                banMsg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( banMsg, owner, this.testTime );

                // Should be a successful parse.
                Assert.AreEqual( ParseMessageResult.Success, result );

                // Only 1 message should remain; the ban one.
                Assert.AreEqual( 1, uut.MeetingNotes.Count );
                CompareNotes( banMsg, uut.MeetingNotes.Last(), owner );

                CheckLists();
            }

            // User should no longer be able to chat.
            // Last message received should still be the ban message.

            {
                AddMessage(
                    "Hello World 3",
                    user1,
                    ParseMessageResult.UserIsSilenced
                );

                Assert.AreEqual( 1, uut.MeetingNotes.Count );
                CompareNotes( banMsg, uut.MeetingNotes.Last(), owner );

                CheckLists();
            }
        }

        [Test]
        public void CanNotBanOwnerOrChairTest()
        {
            // Make user1 a chair.
            AddChairMessage( user1, owner );

            // Sanity check
            Assert.IsTrue( this.uut.Chairs.Contains( user1 ) );

            void CheckLists()
            {
                Assert.AreEqual( 0, this.uut.BannedUsers.Count );
                Assert.AreEqual( 0, this.uut.SilencedUsers.Count );
                Assert.AreEqual( 2, this.uut.Chairs.Count );
                Assert.IsTrue( this.uut.Chairs.Contains( owner ) );
                Assert.IsTrue( this.uut.Chairs.Contains( user1 ) );
            }

            // Make sure we can not ban an owner.
            {
                const string prefix = "#ban";
                const string args = owner;
                const MeetingAction action = MeetingAction.Banish;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, user1, this.testTime );
                // Error message is the generic "Can not do this to chair", even if we try
                // to ban the owner. It just makes things easier that way.
                Assert.AreEqual( ParseMessageResult.CanNotDoThisToChair, result );

                CheckLists();

                // 2 Messages: the first chair and the failed ban attempt.
                Assert.AreEqual( 2, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), user1 );
            }

            // Make sure a not-chair can not ban a chair.
            {
                const string prefix = "#ban";
                const string args = user1;
                const MeetingAction action = MeetingAction.Banish;
                const CommandRestriction restrict = CommandRestriction.Anyone;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, user2, this.testTime );
                Assert.AreEqual( ParseMessageResult.CanNotDoThisToChair, result );

                CheckLists();

                // 3 Messages: the first chair and the failed ban attempt.
                Assert.AreEqual( 3, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), user2 );
            }

            // Make sure chair and owner can still chat
            AddAndCheckMessage(
                "Owner Check",
                4,
                owner
            );

            AddAndCheckMessage(
                "Chair Check",
                5,
                user1
            );
        }

        // -------- Silence / Voice Tests --------

        /// <summary>
        /// Ensures if we are a bot admin, we can silence someone, if
        /// configured to do so.
        /// </summary>
        [Test]
        public void BotAdminSilenceTest()
        {
            const string prefix = "#silence";
            const string args = user1;
            const MeetingAction action = MeetingAction.Silence;
            const CommandRestriction restrict = CommandRestriction.ChairsAndBotAdmins;

            IMeetingMessage silenceMsg = MakeMessage(
                prefix,
                args,
                action,
                restrict
            );

            ParseMessageResult result = this.uut.ParseMessage( silenceMsg, adminName, this.testTime );
            Assert.AreEqual( ParseMessageResult.Success, result );

            Assert.AreEqual( 1, uut.SilencedUsers.Count );
            Assert.IsTrue( uut.SilencedUsers.Contains( user1 ) );

            Assert.AreEqual( 1, uut.MeetingNotes.Count );
            CompareNotes( silenceMsg, uut.MeetingNotes.Last(), adminName );
        }

        /// <summary>
        /// Ensures a normal user can not silence someone
        /// if the setting is set to chair and bot admin.
        /// </summary>
        [Test]
        public void NormalUserCantSilenceWithBotAdminSettingTest()
        {
            const string prefix = "#silence";
            const string args = user2;
            const MeetingAction action = MeetingAction.Silence;
            const CommandRestriction restrict = CommandRestriction.ChairsAndBotAdmins;

            IMeetingMessage silenceMsg = MakeMessage(
                prefix,
                args,
                action,
                restrict
            );

            ParseMessageResult result = this.uut.ParseMessage( silenceMsg, user1, this.testTime );
            Assert.AreEqual( ParseMessageResult.ChairBotAdminOnlyMessage, result );

            EnsureDefaultLists();

            Assert.AreEqual( 1, uut.MeetingNotes.Count );
            CompareNotes( silenceMsg, uut.MeetingNotes.Last(), user1, expectedMeetingAction: MeetingAction.Unknown );
        }

        /// <summary>
        /// Ensures a normal user can not silence someone
        /// if the setting is set to chair and bot admin.
        /// </summary>
        [Test]
        public void NormalUserCantSilenceWithChairOnlySettingTest()
        {
            const string prefix = "#silence";
            const string args = user2;
            const MeetingAction action = MeetingAction.Silence;
            const CommandRestriction restrict = CommandRestriction.ChairsOnly;

            IMeetingMessage silenceMsg = MakeMessage(
                prefix,
                args,
                action,
                restrict
            );

            ParseMessageResult result = this.uut.ParseMessage( silenceMsg, user1, this.testTime );
            Assert.AreEqual( ParseMessageResult.ChairOnlyCommand, result );

            EnsureDefaultLists();

            Assert.AreEqual( 1, uut.MeetingNotes.Count );
            CompareNotes( silenceMsg, uut.MeetingNotes.Last(), user1, expectedMeetingAction: MeetingAction.Unknown );
        }

        [Test]
        public void SilenceVoiceTest()
        {
            // Make a standard message from a user
            AddAndCheckMessage(
                "Hello, world",
                1,
                user1
            );

            void CheckLists()
            {
                Assert.AreEqual( 0, this.uut.BannedUsers.Count );
                Assert.AreEqual( 1, this.uut.Chairs.Count );
                Assert.IsTrue( this.uut.Chairs.Contains( owner ) );
            }

            // User said something dumb, silence him!
            IMeetingMessage silenceMsg;
            {
                const string prefix = "#silence";
                const string args = user1;
                const MeetingAction action = MeetingAction.Silence;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                silenceMsg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( silenceMsg, owner, this.testTime );
                Assert.AreEqual( ParseMessageResult.Success, result );

                CheckLists();
                Assert.AreEqual( 1, uut.SilencedUsers.Count );
                Assert.IsTrue( uut.SilencedUsers.Contains( user1 ) );

                // 2 Messages: the first message and the silence.
                Assert.AreEqual( 2, uut.MeetingNotes.Count );
                CompareNotes( silenceMsg, uut.MeetingNotes.Last(), owner );
            }

            // Silence user message should not be added to the logs.
            {
                AddMessage(
                    "Can you hear me?",
                    user1,
                    ParseMessageResult.UserIsSilenced
                );

                CheckLists();
                Assert.AreEqual( 1, uut.SilencedUsers.Count );
                Assert.IsTrue( uut.SilencedUsers.Contains( user1 ) );

                // Most recent message should be the silence.
                Assert.AreEqual( 2, uut.MeetingNotes.Count );
                CompareNotes( silenceMsg, uut.MeetingNotes.Last(), owner );
            }

            // Unsilence the user.
            {
                const string prefix = "#voice";
                const string args = user1;
                const MeetingAction action = MeetingAction.Voice;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );
                Assert.AreEqual( ParseMessageResult.Success, result );

                CheckLists();
                Assert.AreEqual( 0, uut.SilencedUsers.Count );

                // 3 Messages: now including the voice message.
                Assert.AreEqual( 3, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), owner );
            }

            // User should now be able to chat.
            AddAndCheckMessage(
                "Hello, world, again!",
                4,
                user1
            );
        }

        [Test]
        public void CanNotSilenceOwnerOrChairTest()
        {
            // Make user1 a chair.
            AddChairMessage( user1, owner );

            // Sanity check
            Assert.IsTrue( this.uut.Chairs.Contains( user1 ) );

            void CheckLists()
            {
                Assert.AreEqual( 0, this.uut.BannedUsers.Count );
                Assert.AreEqual( 0, this.uut.SilencedUsers.Count );
                Assert.AreEqual( 2, this.uut.Chairs.Count );
                Assert.IsTrue( this.uut.Chairs.Contains( owner ) );
                Assert.IsTrue( this.uut.Chairs.Contains( user1 ) );
            }

            // Make sure we can not silence an owner.
            {
                const string prefix = "#silence";
                const string args = owner;
                const MeetingAction action = MeetingAction.Silence;
                const CommandRestriction restrict = CommandRestriction.ChairsOnly;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, user1, this.testTime );
                // Error message is the generic "Can not do this to chair", even if we try
                // to silence the owner. It just makes things easier that way.
                Assert.AreEqual( ParseMessageResult.CanNotDoThisToChair, result );

                CheckLists();

                // 2 Messages: the first chair and the failed silence attempt.
                Assert.AreEqual( 2, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), user1 );
            }

            // Make sure a not-chair can not silence a chair.
            {
                const string prefix = "#silence";
                const string args = user1;
                const MeetingAction action = MeetingAction.Silence;
                const CommandRestriction restrict = CommandRestriction.Anyone;

                IMeetingMessage msg = MakeMessage(
                    prefix,
                    args,
                    action,
                    restrict
                );

                ParseMessageResult result = this.uut.ParseMessage( msg, user2, this.testTime );
                Assert.AreEqual( ParseMessageResult.CanNotDoThisToChair, result );

                CheckLists();

                // 3 Messages: the first chair and the failed silence attempt.
                Assert.AreEqual( 3, uut.MeetingNotes.Count );
                CompareNotes( msg, uut.MeetingNotes.Last(), user2 );
            }

            // Make sure chair and owner can still chat
            AddAndCheckMessage(
                "Owner Check",
                4,
                owner
            );

            AddAndCheckMessage(
                "Chair Check",
                5,
                user1
            );
        }

        // -------- Help Tests --------

        /// <summary>
        /// Ensure help messages are not added to the meeting notes
        /// (why clutter them?).
        /// </summary>
        [Test]
        public void HelpTest()
        {
            const string prefix = "#help";
            const string args = "#link";
            const MeetingAction action = MeetingAction.Help;
            const CommandRestriction restrict = CommandRestriction.Anyone;

            IMeetingMessage msg = MakeMessage(
                prefix,
                args,
                action,
                restrict
            );

            ParseMessageResult result = this.uut.ParseMessage( msg, owner, this.testTime );
            Assert.AreEqual( ParseMessageResult.Success, result );

            EnsureDefaultLists();

            // Should be nothing in the meeting notes, helps are not added.
            Assert.AreEqual( 0, uut.MeetingNotes.Count );
        }

        // ---------------- Test Helpers ----------------

        private IMeetingMessage AddChairMessage(
            string userToChair,
            string userThatChairs,
            ParseMessageResult expectedResult = ParseMessageResult.Success
        )
        {
            const string prefix = "#chair";
            string args = userToChair;
            const MeetingAction action = MeetingAction.Chair;
            const CommandRestriction restrict = CommandRestriction.ChairsOnly;

            IMeetingMessage msg = MakeMessage(
                prefix,
                args,
                action,
                restrict
            );

            ParseMessageResult result = this.uut.ParseMessage( msg, userThatChairs, this.testTime );
            Assert.AreEqual( expectedResult, result );

            return msg;
        }

        /// <summary>
        /// Adds a normal message to the meeting notes,
        /// and returns the message that was added.
        /// </summary>
        private IMeetingMessage AddMessage(
            string message,
            string user,
            ParseMessageResult expectedResult = ParseMessageResult.Success,
            DateTime? timestamp = null
        )
        {
            const string prefix = "";
            string args = message;
            const MeetingAction action = MeetingAction.Unknown;
            const CommandRestriction restrict = CommandRestriction.Unknown;

            IMeetingMessage msg = MakeMessage(
                prefix,
                args,
                action,
                restrict
            );

            ParseMessageResult result = this.uut.ParseMessage( msg, user, timestamp ?? this.testTime );
            Assert.AreEqual( expectedResult, result );

            return msg;
        }

        /// <summary>
        /// Adds a normal message to the meeting notes,
        /// and checks it to make sure it got added correctly.
        /// </summary>
        private void AddAndCheckMessage(
            string message,
            int expectedNumberOfMessages,
            string user,
            DateTime? timestamp = null
        )
        {
            IMeetingMessage msg = AddMessage(
                message,
                user,
                ParseMessageResult.Success,
                timestamp
            );

            // Should be a successful parse.
            Assert.AreEqual( expectedNumberOfMessages, uut.MeetingNotes.Count );
            CompareNotes( msg, uut.MeetingNotes.Last(), user );
        }

        private Mock<IMeetingMessage> MakeMockMessage(
            string prefix,
            string args,
            MeetingAction action,
            CommandRestriction restriction
        )
        {
            Mock<IMeetingMessage> message = new Mock<IMeetingMessage>( MockBehavior.Strict );

            message.Setup(
                m => m.CommandPrefix
            ).Returns( prefix );

            message.Setup(
                m => m.CommandArgs
            ).Returns( args );

            message.Setup(
                m => m.MeetingAction
            ).Returns( action );

            message.Setup(
                m => m.Restriction
            ).Returns( restriction );

            return message;
        }

        private IMeetingMessage MakeMessage(
            string prefix,
            string args,
            MeetingAction action, 
            CommandRestriction restriction
        )
        {
            return MakeMockMessage( prefix, args, action, restriction ).Object;
        }

        /// <summary>
        /// Ensures the list are set to the default values set at the start of the meeting.
        /// </summary>
        private void EnsureDefaultLists()
        {
            // Everything should be empty EXCEPT for chairs, which should have the owner.
            Assert.AreEqual( 1, uut.Chairs.Count );
            Assert.IsTrue( uut.Chairs.Contains( owner ) );

            Assert.AreEqual( 0, uut.SilencedUsers.Count );
            Assert.AreEqual( 0, uut.BannedUsers.Count );
        }

        private void CompareNotes(
            IMeetingMessage msg,
            IReadOnlyMeetingNote note,
            string userName = user1,
            DateTime? time = null,
            MeetingAction? expectedMeetingAction = null
        )
        {
            Assert.AreEqual( expectedMeetingAction ?? msg.MeetingAction, note.MeetingAction );
            Assert.AreEqual( userName, note.UserName );
            Assert.AreEqual( time ?? this.testTime, note.TimeStamp );

            // Unknown should be just the args, everything else is prefix then args.
            if( msg.MeetingAction == MeetingAction.Unknown )
            {
                Assert.AreEqual( msg.CommandArgs, note.Message );
            }
            else
            {
                Assert.AreEqual( $"{msg.CommandPrefix} {msg.CommandArgs}", note.Message );
            }
        }
    }
}
