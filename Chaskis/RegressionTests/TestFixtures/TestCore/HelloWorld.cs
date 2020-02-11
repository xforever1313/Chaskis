//
//          Copyright Seth Hendrick 2017-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace RegressionTests.TestCore
{
    /// <summary>
    /// This Fixture is used to ensure our setup is working correctly.
    /// </summary>
    public class HelloWorld 
    {
        /// <summary>
        /// This test just ensures our FitNesse environment is working correctly.
        /// </summary>
        public bool IsHelloWorld( string inputLine )
        {
            return string.Equals( inputLine, "Hello World" );
        }
    }
}
