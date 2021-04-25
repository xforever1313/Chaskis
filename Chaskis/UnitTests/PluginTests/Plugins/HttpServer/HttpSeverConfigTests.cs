//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Plugins.HttpServer;
using Chaskis.UnitTests.Common;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.HttpServer
{
    [TestFixture]
    public class HttpSeverConfigTests
    {
        // ----------------- Tests -----------------

        [Test]
        public void EqualsTest()
        {
            HttpServerConfig config1 = new HttpServerConfig();
            HttpServerConfig config2 = new HttpServerConfig();

            // Null check
            Assert.IsFalse( config1.Equals( null ) );
            Assert.IsFalse( config1.Equals( 1 ) );

            TestHelpers.EqualsTest( config1, config2 );

            config1.Port = (ushort)( config2.Port + 1 );
            TestHelpers.NotEqualsTest( config1, config2 );
            config1.Port = config2.Port;
        }
    }
}
