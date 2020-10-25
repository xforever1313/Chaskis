//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Connected
{
    [TestFixture]
    public class ConnectedEventConfigTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ValidateTest()
        {
            ConnectedEventConfig config = new ConnectedEventConfig();

            config.LineAction = null;
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            config.LineAction = delegate ( ConnectedEventArgs args )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            ConnectedEventConfig config = new ConnectedEventConfig();
            ConnectedEventConfig clone = config.Clone();

            Assert.AreNotSame( config, clone );
        }
    }
}
