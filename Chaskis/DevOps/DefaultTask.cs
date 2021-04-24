//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Runtime.ExceptionServices;
using Cake.Common.Diagnostics;
using Cake.Frosting;

namespace DevOps
{
    public abstract class DefaultTask : FrostingTask<ChaskisContext>
    {
        // ---------------- Functions ----------------

        public override void OnError( Exception exception, ChaskisContext context )
        {
            // We want the stack trace to print out when all is said and done.
            // The way to do this is to set the verbosity to the maximum,
            // and then re-throw the exception.  Use the weird DispatchInfo
            // class so we don't get a new stack trace.
            // We need to re-throw the exception, or cake will exit with a zero exit code.
            context.DiagnosticVerbosity();
            ExceptionDispatchInfo.Capture( exception ).Throw();
        }
    }
}
