//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests
{
    [TestFixture]
    public class MessageHandlerConfigTests
    {
        /// <summary>
        /// Ensures the default values when constructing are correct.
        /// </summary>
        [Test]
        public void DefaultValueTest()
        {
            MessageHandlerConfig uut = new MessageHandlerConfig();

            Assert.AreEqual( 0, uut.CoolDown );
            Assert.AreEqual( RegexOptions.None, uut.RegexOptions );
            Assert.AreEqual( ResponseOptions.ChannelAndPms, uut.ResponseOption );
            Assert.IsFalse( uut.RespondToSelf );
        }

        /// <summary>
        /// Ensures the Validate() function works as expected.
        /// </summary>
        [Test]
        public void ValidateTest()
        {
            MessageHandlerConfig config = new MessageHandlerConfig();

            // No action should not validate.
            config.LineAction = null;
            config.LineRegex = @"!bot\s+help";
            Assert.Throws<ValidationException>( () => config.Validate() );

            // Empty regex should not validate.
            config.LineAction = delegate ( IIrcWriter writer, MessageHandlerArgs response )
            {
            };
            config.LineRegex = string.Empty;
            Assert.Throws<ValidationException>( () => config.Validate() );

            // Null regex should not validate.
            config.LineAction = delegate ( IIrcWriter writer, MessageHandlerArgs response )
            {
            };
            config.LineRegex = null;
            Assert.Throws<ValidationException>( () => config.Validate() );

            // Cooldown can not be less than 0.
            config.LineAction = delegate ( IIrcWriter writer, MessageHandlerArgs response )
            {
            };
            config.LineRegex = @"!bot\s+help";
            config.CoolDown = -1;
            Assert.Throws<ValidationException>( () => config.Validate() );

            // This should validate.
            config.LineAction = delegate ( IIrcWriter writer, MessageHandlerArgs response )
            {
            };
            config.LineRegex = @"!bot\s+help";
            config.CoolDown = 1;
            Assert.DoesNotThrow( () => config.Validate() );
        }

        [Test]
        public void CloneTest()
        {
            MessageHandlerConfig config1 = new MessageHandlerConfig();
            MessageHandlerConfig clone = config1.Clone();

            Assert.AreNotSame( config1, clone );
        }
    }
}
