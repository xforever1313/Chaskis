//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.AnyChaskisEvent
{
    [TestFixture]
    public sealed class AnyChaskisEventHandlerConfigTests
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures the Validate() function works as expected.
        /// </summary>
        [Test]
        public void ValidateTest()
        {
            AnyChaskisEventHandlerConfig config = new AnyChaskisEventHandlerConfig();

            config.LineAction = null;
            Assert.Throws<ValidationException>( () => config.Validate() );

            config.LineAction = delegate ( AnyChaskisEventHandlerArgs args )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            AnyChaskisEventHandlerConfig config1 = new AnyChaskisEventHandlerConfig();
            AnyChaskisEventHandlerConfig clone = config1.Clone();

            Assert.AreNotSame( config1, clone );
        }
    }
}
