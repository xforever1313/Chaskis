//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
using System;
using Chaskis.Plugins.UserListBot;
using NUnit.Framework;

namespace Tests.Plugins.UserListBot
{
    [TestFixture]
    public class UserListTest
    {
        // -------- Fields --------

        /// <summary>
        /// Unit Under Test.
        /// </summary>
        private UserList uut;

        /// <summary>
        /// The name of the bot.
        /// </summary>
        private const string botName = "MyBot";

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.uut = new UserList();

            // Start Empty.
            Assert.AreEqual( 0, this.uut.UsersPerChannel.Count );
        }

        [TearDown]
        public void Teardown()
        {
        }

        // -------- Tests --------

        /// <summary>
        /// Ensure messages such as private message
        /// or joins are ignored.
        /// </summary>
        [Test]
        public void IgnoreTest()
        {
            string ircString = TestHelpers.ConstructIrcString( "guy", "PRIMSG", "#MyChannel", "My Message" );
            this.uut.ParseNameResponse( ircString );

            // Should be empty, bad parse.
            Assert.AreEqual( 0, this.uut.UsersPerChannel.Count );
            Assert.IsNull( this.uut.CheckAndHandleEndMessage( ircString ) );

            ircString = TestHelpers.ConstructPingString( "12345" );
            this.uut.ParseNameResponse( ircString );

            // Should be empty, bad parse.
            Assert.AreEqual( 0, this.uut.UsersPerChannel.Count );
            Assert.IsNull( this.uut.CheckAndHandleEndMessage( ircString ) );
        }

        /// <summary>
        /// Ensures the names works for multiple channels.
        /// </summary>
        [Test]
        public void NamesTest()
        {
            const string chan1 = "#chan1";
            const string chan2 = "#chan2";
            const string names1_1 = "kevin chris +ben ~tim @seth";
            const string names1_2 = "alex kennedy kyle";
            const string names2 = "randy grant dan lori bob pete jenna";

            // Add first response from channel 1.
            {
                string name1_1String = BuildUserNameResponseString( chan1, names1_1 );
                this.uut.ParseNameResponse( name1_1String );

                // Should end up in our user dictionary.
                Assert.AreEqual( 1, this.uut.UsersPerChannel.Count );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan1 ) );
                Assert.AreEqual( names1_1, this.uut.UsersPerChannel[chan1] );

                Assert.IsNull( this.uut.CheckAndHandleEndMessage( name1_1String ) );

                // Should not clear.
                Assert.AreEqual( 1, this.uut.UsersPerChannel.Count );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan1 ) );
                Assert.AreEqual( names1_1, this.uut.UsersPerChannel[chan1] );
            }

            // Add response from channel 2.
            {
                string name2String = BuildUserNameResponseString( chan2, names2 );
                this.uut.ParseNameResponse( name2String );

                // Should end up in our user dictionary.
                Assert.AreEqual( 2, this.uut.UsersPerChannel.Count );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan1 ) ); // Still contain channel 1.
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan2 ) );
                Assert.AreEqual( names1_1, this.uut.UsersPerChannel[chan1] );
                Assert.AreEqual( names2, this.uut.UsersPerChannel[chan2] );

                Assert.IsNull( this.uut.CheckAndHandleEndMessage( name2String ) );

                // Should not clear.
                Assert.AreEqual( 2, this.uut.UsersPerChannel.Count );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan1 ) ); // Still contain channel 1.
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan2 ) );
                Assert.AreEqual( names1_1, this.uut.UsersPerChannel[chan1] );
                Assert.AreEqual( names2, this.uut.UsersPerChannel[chan2] );
            }

            // Add second response from channel 1.
            {
                string name1_2String = BuildUserNameResponseString( chan1, names1_2 );
                this.uut.ParseNameResponse( name1_2String );

                // Should end up in our user dictionary.
                Assert.AreEqual( 2, this.uut.UsersPerChannel.Count );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan1 ) ); // Still contain channel 1.
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan2 ) );
                Assert.AreEqual( names1_1 + " " + names1_2, this.uut.UsersPerChannel[chan1] );
                Assert.AreEqual( names2, this.uut.UsersPerChannel[chan2] );

                Assert.IsNull( this.uut.CheckAndHandleEndMessage( name1_2String ) );

                // Should not clear.
                Assert.AreEqual( 2, this.uut.UsersPerChannel.Count );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan1 ) ); // Still contain channel 1.
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan2 ) );
                Assert.AreEqual( names1_1 + " " + names1_2, this.uut.UsersPerChannel[chan1] );
                Assert.AreEqual( names2, this.uut.UsersPerChannel[chan2] );
            }

            // Receive end-of-names from channel 1.
            {
                string endOfNames1 = BuildEndOfNamesResponseString( chan1 );
                this.uut.ParseNameResponse( endOfNames1 );

                // Nothing should change yet!
                Assert.AreEqual( 2, this.uut.UsersPerChannel.Count );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan1 ) );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan2 ) );
                Assert.AreEqual( names1_1 + " " + names1_2, this.uut.UsersPerChannel[chan1] );
                Assert.AreEqual( names2, this.uut.UsersPerChannel[chan2] );

                // Okay, now send end-of-names.
                Tuple<string, string> userList = this.uut.CheckAndHandleEndMessage( endOfNames1 );
                Assert.IsNotNull( userList );

                // Get the correct response.
                Assert.AreEqual( chan1, userList.Item1 ); // Expect channel 1
                Assert.AreEqual( names1_1 + " " + names1_2, userList.Item2 );

                // Channel 1 should be clear from the dictionary, but not channel 2.
                Assert.AreEqual( 1, this.uut.UsersPerChannel.Count );
                Assert.IsFalse( this.uut.UsersPerChannel.ContainsKey( chan1 ) );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan2 ) );
                Assert.AreEqual( names2, this.uut.UsersPerChannel[chan2] );
            }

            // Receive end-of-names from channel 2.
            {
                string endOfNames2 = BuildEndOfNamesResponseString( chan2 );
                this.uut.ParseNameResponse( endOfNames2 );

                // Nothing should change yet!
                Assert.AreEqual( 1, this.uut.UsersPerChannel.Count );
                Assert.IsTrue( this.uut.UsersPerChannel.ContainsKey( chan2 ) );
                Assert.AreEqual( names2, this.uut.UsersPerChannel[chan2] );

                // Okay, now send end-of-names.
                Tuple<string, string> userList = this.uut.CheckAndHandleEndMessage( endOfNames2 );
                Assert.IsNotNull( userList );

                // Get the correct response.
                Assert.AreEqual( chan2, userList.Item1 ); // Expect channel 2
                Assert.AreEqual( names2, userList.Item2 );

                // Nothing should be left.
                Assert.AreEqual( 0, this.uut.UsersPerChannel.Count );
            }
        }

        // -------- Test Helpers --------

        /// <summary>
        /// Constructs a response that could come from the server
        /// when names is queried.
        /// </summary>
        /// <param name="channel">The channel that the bot is on.</param>
        /// <param name="nameList">The list of names that returns.</param>
        /// <returns>:my.server.net 353 MyBot @ #MyChannel :MyBot @otherUser</returns>
        private string BuildUserNameResponseString( string channel, string nameList )
        {
            return string.Format(
                ":my.server.net 353 {0} @ {1} :{2}",
                botName,
                channel,
                nameList
            );
        }

        /// <summary>
        /// Constructs a response that could come from the server
        /// when end of names is reached.
        /// </summary>
        /// <param name="channel">The channel that the bot is on.</param>
        /// <returns>:my.server.net 366 MyBot #MyChannel :End of /NAMES list.</returns>
        private string BuildEndOfNamesResponseString( string channel )
        {
            return string.Format(
                ":my.server.net 366 {0} {1} :End of /NAMES list.",
                botName,
                channel
            );
        }
    }
}