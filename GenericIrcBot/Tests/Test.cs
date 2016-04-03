using NUnit.Framework;
using System;
using GenericIrcBot;

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

