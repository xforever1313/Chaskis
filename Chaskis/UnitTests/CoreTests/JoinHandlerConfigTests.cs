//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests
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

            config.JoinAction = delegate ( IIrcWriter writer, IrcResponse response )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void DeepCopyTest()
        {
            JoinHandlerConfig config1 = new JoinHandlerConfig();
            JoinHandlerConfig clone = config1.DeepCopy();

            Assert.AreNotSame( config1, clone );
        }
    }
}
