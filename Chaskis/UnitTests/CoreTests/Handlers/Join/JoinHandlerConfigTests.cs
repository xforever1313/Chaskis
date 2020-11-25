//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Join
{
    [TestFixture]
    public class JoinHandlerConfigTests
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures the Validate() function works as expected.
        /// </summary>
        [Test]
        public void ValidateTest()
        {
            JoinHandlerConfig config = new JoinHandlerConfig();

            config.JoinAction = null;
            Assert.Throws<ValidationException>( () => config.Validate() );

            config.JoinAction = delegate ( JoinHandlerArgs args )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            JoinHandlerConfig config1 = new JoinHandlerConfig();
            JoinHandlerConfig clone = config1.Clone();

            Assert.AreNotSame( config1, clone );
        }
    }
}
