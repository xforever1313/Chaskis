//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using SethCS.Basic;

namespace Chaskis.RegressionTests.TestCore
{
    public static class Step
    {
        public static void Run( string message, Action action )
        {
            Run(
                Logger.GetConsoleOutLog(),
                message,
                action
            );
        }

        public static void Run( GenericLogger log, string message, Action action )
        {
            try
            {
                log.WriteLine( message + "..." );
                action();
                log.WriteLine( message + "... Done!" );
            }
            catch( Exception e )
            {
                log.WriteLine( message + ": failed due to crash." );
                throw new Exception( "Rethrow", e );
            }
        }
    }
}
