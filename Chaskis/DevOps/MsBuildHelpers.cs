//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

#if CAKE
#else
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Tools.DotNetCore.MSBuild;
using Cake.Core;
using Cake.Core.IO;

namespace DevOps
{
#endif
    public static class MsBuildHelpers
    {
        /// <summary>
        /// Calls MSBuild to compile Chaskis
        /// </summary>
        /// <param name="configuration">The configuration to use (e.g. Debug, Release, etc.).</param>
        public static void DoMsBuild( ICakeContext context, FilePath sln, string configuration )
        {
            DotNetCoreMSBuildSettings msBuildSettings = new DotNetCoreMSBuildSettings
            {
                WorkingDirectory = sln.GetDirectory().ToString()
            }
            .SetMaxCpuCount( System.Environment.ProcessorCount )
            .SetConfiguration( configuration );

            DotNetCoreBuildSettings settings = new DotNetCoreBuildSettings
            {
                MSBuildSettings = msBuildSettings
            };

            context.DotNetCoreBuild( sln.ToString(), settings );
        }
    }

#if CAKE
#else
}
#endif
