//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Plugins.MessageFixerBot;
using NUnit.Framework;

namespace Tests.Plugins.SearchAndReplaceBot
{
    [TestFixture]
    public class MessageFixerTests
    {
        // ---------------- Fields ----------------

        private MessageFixer uut;

        private const string remoteUser = "user";

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.uut = new MessageFixer();
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures that if a user sends an edit request,
        /// but has not sent a message prior, we print an error message.
        /// </summary>
        [Test]
        public void NoExistingMessageTest()
        {
            MessageFixerResult result = this.uut.RecordNewMessage(
                remoteUser,
                "s/hello/world"
            );

            // Expect failure, but we want to tell the user they send a previous message.
            Assert.IsFalse( result.Success );
            Assert.IsNotEmpty( result.Message );
        }

        /// <summary>
        /// Ensures we ignore case.
        /// </summary>
        [Test]
        public void IgnoreCaseTest()
        {
            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "Hello World!"
                );

                // First result, expect no messages in result.
                Assert.IsFalse( result.Success );
                Assert.IsEmpty( result.Message );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/world/there"
                );

                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "Hello there!" ) );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/WORLD/Mars"
                );

                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "Hello Mars!" ) );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/World/Earth"
                );

                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "Hello Earth!" ) );
            }
        }

        /// <summary>
        /// Ensures regexes work.
        /// </summary>
        [Test]
        public void RegexTest()
        {
            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "Hello World!"
                );

                // First result, expect no messages in result.
                Assert.IsFalse( result.Success );
                Assert.IsEmpty( result.Message );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/.+/Goodbye Earth!"
                );

                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "Goodbye Earth!" ) );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/[h]/J"
                );

                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "Jello World!" ) );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/(H|W)/M"
                );

                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "Mello Morld!" ) );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    @"s/\s+/_"
                );

                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "Hello_World!" ) );
            }

            
        }

        /// <summary>
        /// Ensures bad regexes do not work.
        /// </summary>
        [Test]
        public void BadRegexTest()
        {
            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "Hello World!"
                );

                // First result, expect no messages in result.
                Assert.IsFalse( result.Success );
                Assert.IsEmpty( result.Message );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/[]/J"
                );

                Assert.IsFalse( result.Success );
                Assert.IsNotEmpty( result.Message );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/(/J"
                );

                Assert.IsFalse( result.Success );
                Assert.IsNotEmpty( result.Message );
            }
        }

        /// <summary>
        /// Ensures escaped characters work.
        /// </summary>
        [Test]
        public void EscapedCharacterTest()
        {
            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "Hello Wor/d!"
                );

                // First result, expect no messages in result.
                Assert.IsFalse( result.Success );
                Assert.IsEmpty( result.Message );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    @"s/Wor\/d/World"
                );

                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "Hello World!" ) );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    @"s/\//l"
                );

                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "Hello World!" ) );
            }
        }

        /// <summary>
        /// Ensures that if a user sends an edit request,
        /// but the message's pattern doesn't match the previous
        /// message we received, the result is an error message.
        /// </summary>
        [Test]
        public void NoMatchThenMatchTest()
        {
            const string message = "I am a loser!";
            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    message
                );

                // First result, expect no messages in result.
                Assert.IsFalse( result.Success );
                Assert.IsEmpty( result.Message );

                // Last message should be added.
                Assert.AreEqual( message, this.uut.LastMessages[remoteUser] );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/hello/world"
                );

                // Expect failure, but we want to tell the user they send a previous message.
                Assert.IsFalse( result.Success );
                Assert.IsNotEmpty( result.Message );

                // Last message should not be changed... s/ doesn't change the previous message.
                Assert.AreEqual( message, this.uut.LastMessages[remoteUser] );
            }

            {
                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    "s/loser/winner"
                );

                // For the third result, we expect success.
                Assert.IsTrue( result.Success );
                Assert.IsNotEmpty( result.Message );
                Assert.IsTrue( result.Message.Contains( "I am a winner!" ) );

                // Last message should not be changed... s/ doesn't change the previous message.
                Assert.AreEqual( message, this.uut.LastMessages[remoteUser] );
            }

            {
                const string msg = "s/hello/world/"; // Trailing '/', should not match.

                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    msg
                );

                // Expect failure, with no message (didn't match).
                Assert.IsFalse( result.Success );
                Assert.IsEmpty( result.Message );

                // Last message should be updated.
                Assert.AreEqual( msg, this.uut.LastMessages[remoteUser] );
            }

            {
                const string msg = "/s/hello/world"; // Starting '/', should not match.

                MessageFixerResult result = this.uut.RecordNewMessage(
                    remoteUser,
                    msg
                );

                // Expect failure, with no message (didn't match).
                Assert.IsFalse( result.Success );
                Assert.IsEmpty( result.Message );

                // Last message should not be changed... s/ doesn't change the previous message.
                Assert.AreEqual( msg, this.uut.LastMessages[remoteUser] );
            }
        }
    }
}
