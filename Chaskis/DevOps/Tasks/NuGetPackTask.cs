//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Core.IO;
using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "nuget_pack" )]
    [TaskDescription( "Creates the Chaskis Core NuGet package." )]
    public class NuGetPackTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override void Run( ChaskisContext context )
        {
            DirectoryPath outputPath = context.Paths.OutputPackages.Combine( "nuget" );
            context.EnsureDirectoryExists( outputPath );
            context.CleanDirectory( outputPath );

            DotNetPackSettings settings = new DotNetPackSettings
            {
                OutputDirectory = outputPath,
                Configuration = "Release",
                NoBuild = true
            };

            context.DotNetPack(
                context.Paths.ChaskisCoreFolder.CombineWithFilePath( "Chaskis.Core.csproj" ).ToString(),
                settings
            );

            FilePath glob = outputPath.CombineWithFilePath( "*.nupkg" );
            foreach( FilePath file in context.GetFiles( glob.ToString() ) )
            {
                context.GenerateSha256(
                    file,
                    new FilePath( file.FullPath + ".sha256" )
                );
            }
        }
    }
}
