
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using GenericIrcBot;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void TestCase()
        {
            Assert.Throws<Exception>(
                delegate ()
                {
                    IrcBot bot = new IrcBot();
                    bot.Derp();
                }
            );
        }
    }
}

