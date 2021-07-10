//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Linq;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core.IO;
using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "rpmbuild" )]
    [Dependency( typeof( SpecFileTask ) )]
    [TaskDescription( "Creates the RPM package on Fedora" )]
    public sealed class RpmBuildTask : DefaultTask
    {
        public override bool ShouldRun( ChaskisContext context )
        {
            if( context.FileExists( context.Paths.SpecFileOutputPath ) == false )
            {
                context.Error( "Spec file does not exist, was it built?" );
                return false;
            }
            else if( context.IsRunningOnLinux() == false )
            {
                context.Error( "Can only run on Linux." );
                return false;
            }
            else
            {
                return true;
            }
        }

        public override void Run( ChaskisContext context )
        {
            var runner = new RpmRunner( context );
            runner.BuildRpmPkg();
        }
    }

    public sealed class RpmRunner
    {
        // ---------------- Fields ----------------

        private readonly ChaskisContext context;

        private readonly ImportantPaths paths;

        private readonly DirectoryPath workDir;

        // ---------------- Constructor ----------------

        public RpmRunner( ChaskisContext context )
        {
            this.context = context;
            this.paths = context.Paths;
            this.workDir = this.paths.FedoraLinuxInstallConfigFolder.Combine(
                new DirectoryPath( "obj" )
            );
        }

        // ---------------- Functions ----------------

        public void BuildRpmPkg()
        {
            if( this.context.IsRunningOnWindows() )
            {
                throw new InvalidOperationException( "Can only build RPM packages on Linux." );
            }

            DirectoryPath outputFolder = this.paths.OutputPackages.Combine(
                new DirectoryPath( "fedora" )
            );

            // Create directories
            this.context.EnsureDirectoryExists( this.workDir );
            this.context.CleanDirectory( this.workDir );
            this.context.SetDirectoryPermission( this.workDir, "755" );
            this.context.EnsureDirectoryExists( outputFolder );
            this.context.CleanDirectory( outputFolder );

            FilePath specFile = this.workDir.CombineWithFilePath(
                new FilePath( "chaskis.spec" )
            );
            this.context.CopyFile( this.paths.SpecFileOutputPath, specFile );

            // Build
            this.BuildRpmFile();

            // Move packages
            FilePath glob = new FilePath( "*.rpm" );
            FilePathCollection files = this.context.GetFiles( this.workDir.CombineWithFilePath( glob ).ToString() );

            if( files.Count != 1 )
            {
                throw new ApplicationException( "Found wrong number of files to glob (expected 1), something weird happened. Got: " + files.Count );
            }

            FilePath buildPackageFile = files.First();
            this.context.Information( "RPM File: " + buildPackageFile );

            FilePath sha256File = new FilePath( buildPackageFile.ToString() + ".sha256" );
            this.context.GenerateSha256( buildPackageFile, sha256File );

            this.CopyFile( buildPackageFile, outputFolder );
            this.CopyFile( sha256File, outputFolder );
        }

        private void CopyFile( FilePath source, DirectoryPath destination )
        {
            this.context.Information( $"Moving '{source}' to '${destination}'" );
            this.context.CopyFileToDirectory( source, destination );
        }

        private void BuildRpmFile()
        {
            this.context.Information( "Building RPM file..." );

            ProcessSettings settings = new ProcessSettings
            {
                Arguments = ProcessArgumentBuilder.FromString( "--release f34 local" ),
                WorkingDirectory = this.workDir
            };
            int exitCode = this.context.StartProcess( "fedpkg", settings );
            if( exitCode != 0 )
            {
                throw new ApplicationException(
                    "Could not package for Fedora, got exit code: " + exitCode
                );
            }

            this.context.Information( "Building RPM file... Done!" );
        }
    }
}
