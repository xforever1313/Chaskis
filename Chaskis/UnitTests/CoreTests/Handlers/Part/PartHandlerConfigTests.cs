//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Part
{
    public class PartHandlerConfigTests
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures the Validate() function works as expected.
        /// </summary>
        [Test]
        public void ValidateTest()
        {
            PartHandlerConfig config = new PartHandlerConfig();

            config.PartAction = null;
            Assert.Throws<ValidationException>( () => config.Validate() );

            config.PartAction = delegate ( PartHandlerArgs args )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            PartHandlerConfig config1 = new PartHandlerConfig();
            PartHandlerConfig clone = config1.Clone();

            Assert.AreNotSame( config1, clone );
        }
    }
}
