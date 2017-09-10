//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using NetRunner.ExternalLibrary;

namespace RegressionTests
{
    /// <summary>
    /// This Fixture is used to ensure our setup is working correctly.
    /// </summary>
    public class HelloWorld : BaseTestContainer
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
