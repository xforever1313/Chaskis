//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
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
        public void StandardMessage()
        {
            const string prefix = "";
            const string args = "Hello, World!";
            const MeetingAction action = MeetingAction.Unknown;
            const CommandRestriction restrict = CommandRestriction.Unknown;

            IMeetingMessage msg = MakeMessage(
                prefix,
                args,
                action,
                restrict
            );

            ParseMessageResult result = this.uut.ParseMessage( msg, user1, this.testTime );

            // Should be a successful parse.
            Assert.AreEqual( ParseMessageResult.Success, result );
            Assert.AreEqual( 1, uut.MeetingNotes.Count );
            CompareNotes( msg, uut.MeetingNotes.Last() );

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
        public void ChairUnchairSameUser()
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
        public void OwnerCantUnchairThemself()
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
        public void ChairCanUnchairThemSelf()
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

        // ---------------- Test Helpers ----------------

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
            DateTime? time = null
        )
        {
            Assert.AreEqual( msg.MeetingAction, note.MeetingAction );
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
