//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.UnitTests.Common;
using Chaskis.Core;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.CoreTests.Handlers
{
    [TestFixture]
    public class ChaskisEventHandlerTest
    {
        // ---------------- Fields ---------------

        private ChaskisEventHandlerLineActionArgs argsCaptured;

        private IrcConfig ircConfig;

        private Mock<IIrcWriter> mockWriter;

        private ChaskisEventFactory factoryInstance;
        //private string sourcePluginName;
        private string creatorPluginName;

        private IChaskisEventCreator creator;

        // ---------------- Setup / Teardown ---------------

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.factoryInstance = TestHelpers.CreateEventFactory();
            //this.sourcePluginName = "USERLISTBOT";

            string creatorPluginName = TestHelpers.FactoryPluginNames[1];
            this.creator = this.factoryInstance.EventCreators[creatorPluginName];
            this.creatorPluginName = creatorPluginName.ToUpper();
        }

        [OneTimeTearDown]
        public void TestFixtureTeardown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
            this.argsCaptured = null;
            this.mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ---------------

        /// <summary>
        /// Ensure that if we give bad args, exceptions get thrown.
        /// </summary>
        // [Test]
        // public void InvalidConstructorTest()
        // {
        //     // Pattern can not be null.
        //     Assert.Throws<ArgumentNullException>(
        //         () => this.creator.CreateCoreEventHandler( ChaskisEventProtocol.IRC, this.LineAction )
        //     );
        //     Assert.Throws<ArgumentNullException>(
        //         () => this.creator.CreatePluginEventHandler( this.LineAction )
        //     );
        //     Assert.Throws<ArgumentNullException>(
        //         () => this.creator.CreatePluginEventHandler( this.sourcePluginName, this.LineAction )
        //     );
        // 
        //     // Line action can not be null.
        //     Assert.Throws<ArgumentNullException>(
        //         () => this.creator.CreateCoreEventHandler( ChaskisEventProtocol.IRC, null )
        //     );
        //     Assert.Throws<ArgumentNullException>(
        //         () => this.creator.CreatePluginEventHandler( null )
        //     );
        //     Assert.Throws<ArgumentNullException>(
        //         () => this.creator.CreatePluginEventHandler( this.sourcePluginName, null )
        //     );
        // }
        // 
        // // Four Possible Cases:
        // //    Chaskis Source SourceName   Target       Arguments
        // // 1. CHASKIS CORE   Protocol     BCAST        ARGS
        // // 2. CHASKIS CORE   PluginName   TargetPlugin ARGS  <- Not possible ATM (Core will ALWAYS Be a bcast).
        // // 3. CHASKIS PLUGIN PluginName   BCAST        ARGS
        // // 4. CHASKIS PLUGIN SourcePlugin TargetPlugin ARGS
        // 
        // // -------- Case 1 --------
        // 
        // /// <summary>
        // /// Tests to ensure we can capture a core event.
        // /// Tests Case 1 from above list.
        // /// </summary>
        // [Test]
        // public void CaptureCoreIrcEvent()
        // {
        //     ChaskisEventHandler handler = this.creator.CreateCoreEventHandler(
        //         Regexes.ChaskisIrcDisconnectEvent,
        //         ChaskisEventProtocol.IRC,
        //         this.LineAction
        //     );
        // 
        //     this.DoCase1( handler, true );
        // }
        // 
        // /// <summary>
        // /// Ensures that if we are not expecting a core event, we ignore it.
        // /// Tests the negative case 1 from the above list.
        // /// </summary>
        // [Test]
        // public void IgnoresCoreIrcEvent()
        // {
        //     ChaskisEventHandler handler = this.creator.CreatePluginEventHandler(
        //         @"USERLIST\s+(?<users>.+)",
        //         this.LineAction
        //     );
        // 
        //     this.DoCase1( handler, false );
        // }
        // 
        // private void DoCase1( ChaskisEventHandler handler, bool expectSuccess )
        // {
        //     string expectedArgs = string.Format(
        //         "DISCONNECT FROM {0} AS {1}",
        //         this.ircConfig.Server,
        //         this.ircConfig.Nick
        //     );
        // 
        //     string disconnectEventStr = "CHASKIS CORE IRC BCAST " + expectedArgs;
        // 
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( disconnectEventStr );
        // 
        //     handler.HandleEvent( handlerArgs );
        // 
        //     if( expectSuccess )
        //     {
        //         Assert.IsNotNull( this.argsCaptured );
        //         Assert.AreEqual( expectedArgs, this.argsCaptured.EventArgs );
        //         Assert.AreEqual( "IRC", this.argsCaptured.PluginName );
        //     }
        //     else
        //     {
        //         Assert.IsNull( this.argsCaptured );
        //     }
        // }
        // 
        // // -------- Case 3 --------
        // 
        // /// <summary>
        // /// Ensures that if we are expecting an any plugin event, we capture it.
        // /// Tests case 3 from list above, but expecting any plugin.
        // /// In this case, we expect the event to NOT fire... BCAST events need
        // /// to subscribe to a specific plugin.
        // /// </summary>
        // [Test]
        // public void CaptureAnyPluginEvent_Bcast()
        // {
        //     ChaskisEventHandler handler = this.creator.CreatePluginEventHandler(
        //         @"USERLIST\s+(?<users>.+)",
        //         this.LineAction
        //     );
        // 
        //     this.DoCase3( handler, false );
        // }
        // 
        // /// <summary>
        // /// Ensures that if we are expecting a specfiic target plugin event, we capture it.
        // /// Tests case 3 from list above, but expecting a specific target plugin.
        // /// </summary>
        // [Test]
        // public void CaptureSpecificPluginEvent_Bcast()
        // {
        //     ChaskisEventHandler handler = this.creator.CreatePluginEventHandler(
        //         @"USERLIST\s+(?<users>.+)",
        //         "userlistbot",
        //         this.LineAction
        //     );
        // 
        //     this.DoCase3( handler, true );
        // }
        // 
        // 
        // /// <summary>
        // /// Ensures that during a bcast, if it is not a plugin we care about,
        // /// we ignore it.
        // /// </summary>
        // [Test]
        // public void IgnoreSpecificPluginEvent_BCast()
        // {
        //     ChaskisEventHandler handler = this.creator.CreatePluginEventHandler(
        //         @"USERLIST\s+(?<users>.+)",
        //         "notmyplugin",
        //         this.LineAction
        //     );
        // 
        //     this.DoCase3( handler, false );
        // }
        // 
        // private void DoCase3( ChaskisEventHandler handler, bool expectSuccess )
        // {
        //     // In this case, this is UserListBot boardcasting all the users.
        //     // We want to capture this, since we might care about it.
        // 
        //     string expectedArgs = "USERLIST EVERGREEN MARKEM HARRIS";
        // 
        //     string eventStr = "CHASKIS PLUGIN USERLISTBOT BCAST " + expectedArgs;
        // 
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( eventStr );
        // 
        //     handler.HandleEvent( handlerArgs );
        // 
        //     if( expectSuccess )
        //     {
        //         Assert.IsNotNull( this.argsCaptured );
        //         Assert.AreEqual( expectedArgs, this.argsCaptured.EventArgs );
        //         Assert.AreEqual( "USERLISTBOT", this.argsCaptured.PluginName );
        //         Assert.AreEqual( "EVERGREEN MARKEM HARRIS", this.argsCaptured.Match.Groups["users"].Value );
        //     }
        //     else
        //     {
        //         Assert.IsNull( this.argsCaptured );
        //     }
        // }
        // 
        // // -------- Case 4 --------
        // 
        // /// <summary>
        // /// Ensures that if we are expecting an any plugin event, we capture it.
        // /// Tests case 4 from list above, but we are expecting any plugin.
        // /// </summary>
        // [Test]
        // public void CaptureAnyPluginEvent_Specific()
        // {
        //     // This will technically work with any Plugin, not just USERLISTBOT.
        //     ChaskisEventHandler handler = this.creator.CreatePluginEventHandler(
        //         @"USERLIST\s+(?<users>.+)",
        //         this.LineAction
        //     );
        // 
        //     this.DoCase4( handler, true );
        // }
        // 
        // /// <summary>
        // /// Ensures that if we are expecting an specific plugin event, we capture it.
        // /// Tests case 4 from list above, but we are expecting a specifc plugin.
        // /// </summary>
        // [Test]
        // public void CaptureSpecificPluginEvent_Specific()
        // {
        //     // This will only respond to UserListBot.
        //     ChaskisEventHandler handler = this.creator.CreatePluginEventHandler(
        //         @"USERLIST\s+(?<users>.+)",
        //         "userlistbot",
        //         this.LineAction
        //     );
        // 
        //     this.DoCase4( handler, true );
        // }
        // 
        // /// <summary>
        // /// Ensures that if we are waiting for a specific plugin,
        // /// and we don't see it, we don't fire.
        // /// </summary>
        // [Test]
        // public void IgnoreSpecificPluginEvent_Specific()
        // {
        //     // This will only respond to notmyplugin, not USERLISTBOT.
        //     ChaskisEventHandler handler = this.creator.CreatePluginEventHandler(
        //         @"USERLIST\s+(?<users>.+)",
        //         "notmyplugin",
        //         this.LineAction
        //     );
        // 
        //     this.DoCase4( handler, false );
        // }
        // 
        // private void DoCase4( ChaskisEventHandler handler, bool expectSuccess )
        // {
        //     // For this test case, let's pretend our creator plugin asked UserListBot
        //     // for a userlist.  This is userlist responding to our creator plugin.
        // 
        //     string expectedArgs = "USERLIST EVERGREEN MARKEM HARRIS";
        // 
        //     //                                SourceOfPlugin   EventDestination
        //     string eventStr = "CHASKIS PLUGIN USERLISTBOT " + this.creatorPluginName + " " + expectedArgs;
        // 
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( eventStr );
        // 
        //     handler.HandleEvent( handlerArgs );
        // 
        //     if( expectSuccess )
        //     {
        //         Assert.IsNotNull( this.argsCaptured );
        //         Assert.AreEqual( expectedArgs, this.argsCaptured.EventArgs );
        //         Assert.AreEqual( "USERLISTBOT", this.argsCaptured.PluginName );
        //         Assert.AreEqual( "EVERGREEN MARKEM HARRIS", this.argsCaptured.Match.Groups["users"].Value );
        //     }
        //     else
        //     {
        //         Assert.IsNull( this.argsCaptured );
        //     }
        // }
        // 
        // // -------- Ignores --------
        // 
        // /// <summary>
        // /// Ensures if we get an unknown protocol/plugin, we don't fire.
        // /// </summary>
        // [Test]
        // public void IgnoreUnknownProtocol()
        // {
        //     string expectedArgs = "USERLIST EVERGREEN MARKEM HARRIS";
        // 
        //     string unknownProtocolString = "CHASKIS UNKNOWN USERLISTBOT BCAST " + expectedArgs;
        // 
        //     ChaskisEventHandler handler = this.creator.CreatePluginEventHandler(
        //         @"USERLIST\s+(?<users>.+)",
        //         "userlistbot",
        //         this.LineAction
        //     );
        // 
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( unknownProtocolString );
        //     handler.HandleEvent( handlerArgs );
        // 
        //     Assert.IsNull( this.argsCaptured );
        // }
        // 
        // /// <summary>
        // /// Ensure that if everything matches BUT our pattern, we don't fire.
        // /// </summary>
        // [Test]
        // public void IgnoreNoMatch()
        // {
        //     string expectedArgs = "USERLIST EVERGREEN MARKEM HARRIS";
        // 
        //     string messageStr = "CHASKIS PLUGIN USERLISTBOT BCAST " + expectedArgs;
        // 
        //     ChaskisEventHandler handler = this.creator.CreatePluginEventHandler(
        //         @"SOMETHINGELSE\s+(?<users>.+)",
        //         "userlistbot",
        //         this.LineAction
        //     );
        // 
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( messageStr );
        //     handler.HandleEvent( handlerArgs );
        // 
        //     Assert.IsNull( this.argsCaptured );
        // }
        // 
        // // -------- Ignore non-Chaskis events --------
        // 
        // /// <summary>
        // /// Ensures we ignore join.
        // /// </summary>
        // [Test]
        // public void IgnoreJoin()
        // {
        //     ChaskisEventHandler handler = this.creator.CreateCoreEventHandler(
        //         Regexes.ChaskisIrcDisconnectEvent,
        //         ChaskisEventProtocol.IRC,
        //         this.LineAction
        //     );
        // 
        //     string ircString = TestHelpers.ConstructIrcString(
        //         "user",
        //         JoinHandler.IrcCommand,
        //         "#channel",
        //         null
        //     );
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
        //     handler.HandleEvent( handlerArgs );
        // 
        //     Assert.IsNull( this.argsCaptured );
        // }
        // 
        // /// <summary>
        // /// Ensures we ignore a part.
        // /// </summary>
        // [Test]
        // public void IgnorePart()
        // {
        //     ChaskisEventHandler handler = this.creator.CreateCoreEventHandler(
        //         Regexes.ChaskisIrcDisconnectEvent,
        //         ChaskisEventProtocol.IRC,
        //         this.LineAction
        //     );
        // 
        //     string ircString = TestHelpers.ConstructIrcString(
        //         "user",
        //         PartHandler.IrcCommand,
        //         "#channel",
        //         null
        //     );
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
        //     handler.HandleEvent( handlerArgs );
        // 
        //     Assert.IsNull( this.argsCaptured );
        // }
        // 
        // /// <summary>
        // /// Ensures we ignore a message.
        // /// </summary>
        // [Test]
        // public void IgnoreMessage()
        // {
        //     ChaskisEventHandler handler = this.creator.CreateCoreEventHandler(
        //         Regexes.ChaskisIrcDisconnectEvent,
        //         ChaskisEventProtocol.IRC,
        //         this.LineAction
        //     );
        // 
        //     string ircString = TestHelpers.ConstructIrcString(
        //         "user",
        //         MessageHandler.IrcCommand,
        //         "#channel",
        //         "This is my message!"
        //     );
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
        //     handler.HandleEvent( handlerArgs );
        // 
        //     Assert.IsNull( this.argsCaptured );
        // }
        // 
        // /// <summary>
        // /// Ensures we ignore a ping.
        // /// </summary>
        // [Test]
        // public void IgnorePing()
        // {
        //     ChaskisEventHandler handler = this.creator.CreateCoreEventHandler(
        //         Regexes.ChaskisIrcDisconnectEvent,
        //         ChaskisEventProtocol.IRC,
        //         this.LineAction
        //     );
        // 
        //     string ircString = TestHelpers.ConstructPingString( "irc.somewhere.com" );
        // 
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
        //     handler.HandleEvent( handlerArgs );
        // 
        //     Assert.IsNull( this.argsCaptured );
        // }
        // 
        // /// <summary>
        // /// Ensures we ignore a pong.
        // /// </summary>
        // [Test]
        // public void IgnorePong()
        // {
        //     ChaskisEventHandler handler = this.creator.CreateCoreEventHandler(
        //         Regexes.ChaskisIrcDisconnectEvent,
        //         ChaskisEventProtocol.IRC,
        //         this.LineAction
        //     );
        // 
        //     string ircString = TestHelpers.ConstringPongString( "irc.somewhere.com", "myMessage" );
        // 
        //     HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
        //     handler.HandleEvent( handlerArgs );
        // 
        //     Assert.IsNull( this.argsCaptured );
        // }

        // ---------------- Test Helpers -----------------

        private void LineAction( ChaskisEventHandlerLineActionArgs args )
        {
            this.argsCaptured = args;
        }

        private HandlerArgs CreateHandlerArgs( string line )
        {
            HandlerArgs handlerArgs = new HandlerArgs();
            handlerArgs.IrcConfig = this.ircConfig;
            handlerArgs.Line = line;
            handlerArgs.BlackListedChannels = null;
            handlerArgs.IrcWriter = this.mockWriter.Object;

            return handlerArgs;
        }
    }
}
