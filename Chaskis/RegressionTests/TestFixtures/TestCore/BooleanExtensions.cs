//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using NUnit.Framework;

namespace Chaskis.RegressionTests.TestCore
{
    public static class BooleanExtensions
    {
        public static void FailIfFalse( this bool value, string message )
        {
            if( value == false )
            {
                Assert.Fail( message );
            }
        }
    }
}
