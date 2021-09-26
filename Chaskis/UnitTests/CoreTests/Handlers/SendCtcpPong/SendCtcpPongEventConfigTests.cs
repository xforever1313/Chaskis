//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.SendCtcpPong
{
    [TestFixture]
    public sealed class SendCtcpPongEventConfigTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ValidateTest()
        {
            SendCtcpPongEventConfig config = new SendCtcpPongEventConfig
            {
                LineAction = null
            };
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            config.LineAction = delegate ( SendCtcpPongEventArgs args )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            SendCtcpPongEventConfig config = new SendCtcpPongEventConfig();
            SendCtcpPongEventConfig clone = config.Clone();

            Assert.AreNotSame( config, clone );
        }
    }
}
