//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using Chaskis.Cli;
using NUnit.Framework;

namespace Chaskis.UnitTests.ChaskisTests
{
    [TestFixture]
    public class AssemblyConfigTest
    {
        /// <summary>
        /// Ensures passing in string.Empty or null to the constructor results in an error.
        /// </summary>
        [Test]
        public void InvalidArgumentTest()
        {
            Assert.Throws<ArgumentNullException>( () =>
                new AssemblyConfig( null, new List<string>() )
            );

            Assert.Throws<ArgumentNullException>( () =>
                new AssemblyConfig( string.Empty, new List<string>() )
            );

            Assert.Throws<ArgumentNullException>( () =>
                new AssemblyConfig( "myPath", null )
            );

            Assert.Throws<ArgumentException>( () =>
                new AssemblyConfig( "myPath", new List<string>() { null } )
            );

            Assert.Throws<ArgumentException>( () =>
                new AssemblyConfig( "myPath", new List<string>() { string.Empty } )
            );

            Assert.Throws<ArgumentException>( () =>
                new AssemblyConfig( "myPath", new List<string>() { "     " } )
            );

            Assert.DoesNotThrow( () =>
                new AssemblyConfig( "myPath", new List<string>() )
            );

            Assert.DoesNotThrow( () =>
                new AssemblyConfig( "myPath", new List<string>() { "#mychannel" } )
            );
        }
    }
}