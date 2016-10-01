//
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis;
using NUnit.Framework;

namespace Tests
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
                new AssemblyConfig( null, "TestAssembly.Class" )
            );

            Assert.Throws<ArgumentNullException>( () =>
                new AssemblyConfig( string.Empty, "TestAssembly.Class" )
            );

            Assert.Throws<ArgumentNullException>( () =>
                new AssemblyConfig( "myPath", null )
            );

            Assert.Throws<ArgumentNullException>( () =>
                new AssemblyConfig( "myPath", string.Empty )
            );
        }
    }
}