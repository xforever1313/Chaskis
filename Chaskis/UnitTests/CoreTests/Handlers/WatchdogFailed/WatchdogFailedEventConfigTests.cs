//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.WatchdogFailed
{
    [TestFixture]
    public sealed class WatchdogFailedEventConfigTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ValidateTest()
        {
            WatchdogFailedEventConfig config = new WatchdogFailedEventConfig
            {
                LineAction = null
            };
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            config.LineAction = delegate ( WatchdogFailedEventArgs args )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            WatchdogFailedEventConfig config = new WatchdogFailedEventConfig();
            WatchdogFailedEventConfig clone = config.Clone();

            Assert.AreNotSame( config, clone );
        }
    }
}
