//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.FinishedJoiningChannels
{
    [TestFixture]
    public class FinishedJoiningChannelsEventConfigTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ValidateTest()
        {
            FinishedJoiningChannelsEventConfig config = new FinishedJoiningChannelsEventConfig
            {
                LineAction = null
            };
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            config.LineAction = delegate ( FinishedJoiningChannelsEventArgs args )
            {
            };

            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            FinishedJoiningChannelsEventConfig config = new FinishedJoiningChannelsEventConfig();
            FinishedJoiningChannelsEventConfig clone = config.Clone();

            Assert.AreNotSame( config, clone );
        }
    }
}
