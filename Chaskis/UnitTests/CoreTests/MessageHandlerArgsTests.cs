//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using Chaskis.Core;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.CoreTests
{
    [TestFixture]
    public class MessageHandlerArgsTests
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures the properties get set correctly during construction.
        /// </summary>
        [Test]
        public void ConstructorTest()
        {
            Mock<IIrcWriter> writer = new Mock<IIrcWriter>( MockBehavior.Strict );
            const string user = "user";
            const string channel = "channel";
            const string message = "My Message";
            Regex regex = new Regex( ".+" );
            System.Text.RegularExpressions.Match match = regex.Match( "something" );

            MessageHandlerArgs uut = new MessageHandlerArgs(
                writer.Object,
                user,
                channel,
                message,
                regex,
                match
            );

            Assert.AreSame( writer.Object, uut.Writer );
            Assert.AreEqual( user, uut.User );
            Assert.AreEqual( channel, uut.Channel );
            Assert.AreEqual( message, uut.Message );
            Assert.AreSame( regex, uut.Regex );
            Assert.AreSame( match, uut.Match );
        }
    }
}
