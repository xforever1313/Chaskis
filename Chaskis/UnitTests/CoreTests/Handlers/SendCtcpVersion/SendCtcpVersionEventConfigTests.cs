//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.SendCtcpVersion
{
    [TestFixture]
    public class SendCtcpVersionEventConfigTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ValidateTest()
        {
            SendCtcpVersionEventConfig config = new SendCtcpVersionEventConfig
            {
                LineAction = null
            };
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            config.LineAction = delegate ( SendCtcpVersionEventArgs args )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            SendCtcpVersionEventConfig config = new SendCtcpVersionEventConfig();
            SendCtcpVersionEventConfig clone = config.Clone();

            Assert.AreNotSame( config, clone );
        }
    }
}
