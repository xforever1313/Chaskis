//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.Chocolatey;
using Cake.Common.Tools.Chocolatey.Pack;
using Cake.Core.IO;
using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "choco_pack" )]
    [TaskDescription( "Creates the Chocolatey Package (Windows Only)." )]
    public class ChocolateyPackTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override bool ShouldRun( ChaskisContext context )
        {
            return context.IsRunningOnWindows();
        }

        public override void Run( ChaskisContext context )
        {
            DirectoryPath outputPath = context.Paths.OutputPackages.Combine( "chocolatey" );
            context.EnsureDirectoryExists( outputPath );
            context.CleanDirectory( outputPath );

            DirectoryPath workingPath = context.Paths.ChocolateyInstallConfigFolder.Combine( "package" );

            ChocolateyPackSettings settings = new ChocolateyPackSettings
            {
                OutputDirectory = outputPath,
                WorkingDirectory = workingPath
            };

            context.ChocolateyPack(
                workingPath.CombineWithFilePath( new FilePath( "chaskis.nuspec" ) ),
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
