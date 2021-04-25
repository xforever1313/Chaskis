//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using Chaskis.Core;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.CoreTests.Handlers.Part
{
    [TestFixture]
    public class PartHandlerArgsTests
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
            const string reason = "reason";
            Regex regex = new Regex( ".+" );
            System.Text.RegularExpressions.Match match = regex.Match( "something" );

            PartHandlerArgs uut = new PartHandlerArgs(
                writer.Object,
                user,
                channel,
                reason,
                regex,
                match
            );

            Assert.AreSame( writer.Object, uut.Writer );
            Assert.AreEqual( user, uut.User );
            Assert.AreEqual( channel, uut.Channel );
            Assert.AreSame( reason, uut.Reason );
            Assert.AreSame( regex, uut.Regex );
            Assert.AreSame( match, uut.Match );
        }
    }
}
