//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.CoreTests.Handlers.AnyInterPluginEvent
{
    [TestFixture]
    public class AnyInterPluginEventHandlerArgsTests
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures the properties get set correctly during construction.
        /// </summary>
        [Test]
        public void ConstructorTest()
        {
            Mock<IIrcWriter> writer = new Mock<IIrcWriter>( MockBehavior.Strict );
            const string line = "line";

            AnyInterPluginEventHandlerArgs uut = new AnyInterPluginEventHandlerArgs(
                writer.Object,
                line
            );

            Assert.AreSame( writer.Object, uut.Writer );
            Assert.AreEqual( line, uut.Line );
        }
    }
}
