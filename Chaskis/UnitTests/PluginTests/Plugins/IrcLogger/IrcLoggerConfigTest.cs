//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Plugins.IrcLogger;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.IrcLogger
{
    [TestFixture]
    public sealed class IrcLoggerConfigTest
    {
        /// <summary>
        /// Ensures the default values match
        /// the documentation.
        /// </summary>
        [Test]
        public void DefaultValueTest()
        {
            IrcLoggerConfig uut = new IrcLoggerConfig();
            Assert.IsNull( uut.LogName );
            Assert.IsNull( uut.LogFileLocation );
            Assert.AreEqual( 1000, uut.MaxNumberMessagesPerLog );
        }
    }
}