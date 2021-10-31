//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.CtcpVersion
{
    [TestFixture]
    public sealed class CtcpVersionConfigTests
    {
        /// <summary>
        /// Ensures the default values when constructing are correct.
        /// </summary>
        [Test]
        public void DefaultValueTest()
        {
            CtcpVersionHandlerConfig uut = new CtcpVersionHandlerConfig();

            Assert.AreEqual( 0, uut.CoolDown );
            Assert.AreEqual( RegexOptions.None, uut.RegexOptions );
            Assert.AreEqual( ResponseOptions.PmsOnly, uut.ResponseOption );
            Assert.IsFalse( uut.RespondToSelf );
        }

        /// <summary>
        /// Ensures the Validate() function works as expected.
        /// </summary>
        [Test]
        public void ValidateTest()
        {
            CtcpVersionHandlerConfig config = new CtcpVersionHandlerConfig();

            // No action should not validate.
            config.LineAction = null;
            config.LineRegex = @".+";
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            // Empty regex should not validate.
            config.LineAction = delegate ( CtcpVersionHandlerArgs args )
            {
            };
            config.LineRegex = string.Empty;
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            // Null regex should not validate.
            config.LineAction = delegate ( CtcpVersionHandlerArgs args )
            {
            };
            config.LineRegex = null;
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            // Cooldown can not be less than 0.
            config.LineAction = delegate ( CtcpVersionHandlerArgs args )
            {
            };
            config.LineRegex = @".+";
            config.CoolDown = -1;
            Assert.Throws<ListedValidationException>( () => config.Validate() );

            // This should validate.
            config.LineAction = delegate ( CtcpVersionHandlerArgs args )
            {
            };
            config.LineRegex = @".+";
            config.CoolDown = 1;
            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            CtcpVersionHandlerConfig config1 = new CtcpVersionHandlerConfig();
            CtcpVersionHandlerConfig clone = config1.Clone();

            Assert.AreNotSame( config1, clone );
        }
    }
}
