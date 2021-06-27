//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core.IO;
using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "debian_pack" )]
    public class DebianBuildTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override bool ShouldRun( ChaskisContext context )
        {
            return context.IsRunningOnWindows() == false;
        }

        public override void Run( ChaskisContext context )
        {
            // We will be creating the .deb file in the Debian install directory.
            // Like with building C# Assemblies, we will do all of the work
            // in the obj folder.
            DirectoryPath objFolder = context.Paths.DebianLinuxInstallConfigFolder.Combine(
                new DirectoryPath( "obj" )
            );

            // First, create and obj and output folders, and ensure they are clean.
            DirectoryPath outputFolder = context.Paths.DebDirectory;

            context.EnsureDirectoryExists( objFolder );
            context.CleanDirectory( objFolder );
            context.SetDirectoryPermission( objFolder, "755" );
            context.EnsureDirectoryExists( outputFolder );
            context.CleanDirectory( outputFolder );

            // Directory structure:
            // obj
            //  L chaskis
            //       L DEBIAN (must be 755)
            //            L control (control file)
            //       L bin
            //           L chaskis  (chaskis startup script)
            //       L usr
            //          L lib
            //             L systemd
            //                   L user
            //                       L chaskis.service
            //             L chaskis install directory

            // Create the directories.
            DirectoryPath chaskisWorkDir = CombineAndCreateDirectory( context, objFolder, "chaskis" );
            DirectoryPath debianFolder = CombineAndCreateDirectory( context, chaskisWorkDir, "DEBIAN" );
            DirectoryPath binFolder = CombineAndCreateDirectory( context, chaskisWorkDir, "bin" );
            DirectoryPath usrFolder = CombineAndCreateDirectory( context, chaskisWorkDir, "usr" );
            DirectoryPath usrLibFolder = CombineAndCreateDirectory( context, usrFolder, "lib" );
            DirectoryPath systemdFolder = CombineAndCreateDirectory( context, usrLibFolder, "systemd" );
            DirectoryPath systemdUserFolder = CombineAndCreateDirectory( context, systemdFolder, "user" );

            // Move the linux files around.
            FilePath controlFile = context.Paths.DebianLinuxInstallConfigFolder.CombineWithFilePath(
                new FilePath( "control" )
            );
            CopyFile( context, controlFile, debianFolder );
            CopyFile( context, context.Paths.LinuxBinFile, binFolder );
            CopyFile( context, context.Paths.SystemdFile, systemdUserFolder );

            // Next, need to run the distro creator to put everything
            // in the usr/lib/Chaskis folder.
            DistroCreatorConfig distroConfig = new DistroCreatorConfig
            {
                OutputLocation = usrLibFolder.ToString(),
                Target = "Release"
            };

            DistroCreator distroRunner = new DistroCreator(
                context,
                distroConfig
            );
            distroRunner.CreateDistro();

            // Lastly, need to package everything up.
            ProcessArgumentBuilder arguments = ProcessArgumentBuilder.FromString( "--root-owner-group --build chaskis" );
            ProcessSettings settings = new ProcessSettings
            {
                Arguments = arguments,
                WorkingDirectory = objFolder
            };
            int exitCode = context.StartProcess( "dpkg-deb", settings );
            if( exitCode != 0 )
            {
                throw new ApplicationException(
                    "Could not package deb, got exit code: " + exitCode
                );
            }

            // Copy from the obj folder to the bin folder.
            CopyFile(
                context,
                objFolder.CombineWithFilePath( new FilePath( "chaskis.deb" ) ),
                outputFolder
            );

            // Run the checksum.
            context.GenerateSha256(
                context.Paths.DebPath,
                context.Paths.DebChecksumFile
            );
        }

        private DirectoryPath CombineAndCreateDirectory( ChaskisContext context, DirectoryPath parentFolder, string childFolderName )
        {
            DirectoryPath combinedPath = parentFolder.Combine(
                new DirectoryPath( childFolderName )
            );
            context.EnsureDirectoryExists( combinedPath );

            // All directories should be 755 to be consistent
            // with the old installation method.
            context.SetDirectoryPermission( combinedPath, "755" );

            return combinedPath;
        }

        private void CopyFile( ChaskisContext context, FilePath source, DirectoryPath destination )
        {
            context.Information( $"Moving '{source}' to '${destination}'" );
            context.CopyFileToDirectory( source, destination );
        }
    }
}
