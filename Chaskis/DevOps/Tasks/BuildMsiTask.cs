//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "build_msi" )]
    [TaskDescription( "Builds the MSI on Windows." )]
    public class BuildMsiTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override bool ShouldRun( ChaskisContext context )
        {
            if( context.RunningRelease )
            {
                context.Information( "Can only run Debug Version of DevOps.exe" );
                return false;
            }
            else if( context.IsRunningOnWindows() == false )
            {
                context.Information( "Can only run on Windows." );
                return false;
            }

            return true;
        }

        public override void Run( ChaskisContext context )
        {
            DirectoryPath outputPath = context.Paths.OutputPackages.Combine( "windows" );
            context.EnsureDirectoryExists( outputPath );
            context.CleanDirectory( outputPath );

            DirectoryPath msiLocation = context.Paths.MsiDirectory;

            FilePath msiFile = context.Paths.MsiPath;
            FilePath checksumLocation = msiLocation.CombineWithFilePath( "ChaskisInstaller.msi.sha256" );

            MSBuildSettings settings = new MSBuildSettings
            {
                Configuration = "Install",
                MaxCpuCount = 0,
                PlatformTarget = PlatformTarget.x64,
                Restore = true,
                WorkingDirectory = context.Paths.SourceFolder
            };

            if( context.IsJenkins )
            {
                settings.NodeReuse = false;
                settings.Verbosity = Verbosity.Normal;
                settings.ToolVersion = MSBuildToolVersion.VS2019;
            }

            // For WiX, need to call MSBuild, not dotnet core build.  Wix doesn't work with dotnet core.
            // Its... probably fine??
            context.MSBuild( context.Paths.SolutionPath, settings );

            context.GenerateSha256(
                msiFile,
                checksumLocation
            );

            context.CopyFileToDirectory(
                msiFile,
                outputPath
            );

            context.CopyFileToDirectory(
                checksumLocation,
                outputPath
            );
        }
    }
}
