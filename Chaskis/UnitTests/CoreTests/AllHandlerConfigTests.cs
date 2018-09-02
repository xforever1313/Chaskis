//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using ChaskisCore;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Tests
{
    [TestFixture]
    public class AllHandlerConfigTests
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures the Validate() function works as expected.
        /// </summary>
        [Test]
        public void ValidateTest()
        {
            AllHandlerConfig config = new AllHandlerConfig();

            config.AllAction = null;
            Assert.Throws<ValidationException>( () => config.Validate() );

            config.AllAction = delegate ( IIrcWriter writer, IrcResponse response )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }
    }
}
