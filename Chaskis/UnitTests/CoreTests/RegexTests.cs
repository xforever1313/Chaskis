//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using Chaskis.Core;
using NUnit.Framework;

namespace Chaskis.UnitTests.CoreTests
{
    [TestFixture]
    public class RegexTests
    {
        [Test]
        public void IrcPrefixTest()
        {
            Regex uut = new Regex( Regexes.IrcMessagePrefix );

            // Nick, User, Host
            {
                Match match = uut.Match( ":someone[m]!somename@blah.org" );

                Assert.AreEqual( "someone[m]", match.Groups["nickOrServer"].Value );
                Assert.AreEqual( "somename", match.Groups["username"].Value );
                Assert.AreEqual( "blah.org", match.Groups["host"].Value );
            }

            // Nick, ~User, Host
            {
                Match match = uut.Match( ":someone[m]!~somename@blah.org" );

                Assert.AreEqual( "someone[m]", match.Groups["nickOrServer"].Value );
                Assert.AreEqual( "~somename", match.Groups["username"].Value );
                Assert.AreEqual( "blah.org", match.Groups["host"].Value );
            }

            // Just server
            {
                Match match = uut.Match( ":server.blah.org" );

                Assert.AreEqual( "server.blah.org", match.Groups["nickOrServer"].Value );
                Assert.AreEqual( string.Empty, match.Groups["username"].Value );
                Assert.AreEqual( string.Empty, match.Groups["host"].Value );
            }

            // Just nickname
            {
                Match match = uut.Match( ":someone[m]" );

                Assert.AreEqual( "someone[m]", match.Groups["nickOrServer"].Value );
                Assert.AreEqual( string.Empty, match.Groups["username"].Value );
                Assert.AreEqual( string.Empty, match.Groups["host"].Value );
            }

            // Just nickname and host
            {
                Match match = uut.Match( ":someone@blah.org" );

                Assert.AreEqual( "someone", match.Groups["nickOrServer"].Value );
                Assert.AreEqual( string.Empty, match.Groups["username"].Value );
                Assert.AreEqual( "blah.org", match.Groups["host"].Value );
            }
        }
    }
}
