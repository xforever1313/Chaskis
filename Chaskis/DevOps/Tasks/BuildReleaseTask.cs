//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.Common.Diagnostics;
using Cake.Frosting;

namespace DevOps.Tasks
{
    /// <remarks>
    /// This task only works because DevOps.exe has its Debug build run
    /// when this task is run.  This task will fail if running DevOp's release build.
    /// </remarks>
    [TaskName( "build_release" )]
    [TaskDescription( "Builds the release build of Chaskis" )]
    public class BuildReleaseTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override bool ShouldRun( ChaskisContext context )
        {
            if( context.RunningRelease )
            {
                context.Information( "Can only run Debug Version of DevOps.exe" );
                return false;
            }
            return true;
        }

        public override void Run( ChaskisContext context )
        {
            MsBuildHelpers.DoMsBuild( context, context.Paths.SolutionPath, "Release" );
        }
    }
}
