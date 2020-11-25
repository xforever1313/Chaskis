//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.SendJoin
{
    [TestFixture]
    public class SendJoinEventConfigTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ValidateTest()
        {
            SendJoinEventConfig config = new SendJoinEventConfig
            {
                LineAction = null
            };
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            config.LineAction = delegate ( SendJoinEventArgs args )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            SendJoinEventConfig config = new SendJoinEventConfig();
            SendJoinEventConfig clone = config.Clone();

            Assert.AreNotSame( config, clone );
        }
    }
}
