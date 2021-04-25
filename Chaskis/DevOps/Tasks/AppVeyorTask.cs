//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "appveyor" )]
    [TaskDescription( "Runs when building AppVeyor AFTER compiling the debug build." )]
    [IsDependentOn( typeof( RunUnitTestTask ) )]
    [IsDependentOn( typeof( BuildMsiTask ) )]
    [IsDependentOn( typeof( NuGetPackTask ) )]
    [IsDependentOn( typeof( ChocolateyPackTask ) )]
    public class AppVeyorTask : DefaultTask
    {
    }
}
