//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core.IO;
using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "pkgbuild" )]
    [TaskDescription( "Creates the PKGBUILD on Arch Linux" )]
    public class PkgBuildTask : DefaultTask
    {
        public override bool ShouldRun( ChaskisContext context )
        {
            if( context.FileExists( context.Paths.DebPath ) == false )
            {
                context.Error( ".deb checksum file does not exist, was it built?" );
                return false;
            }
            else if( context.FileExists( context.Paths.DebChecksumFile ) == false )
            {
                context.Error( "Debian checksum file does not exist, was it built?" );
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
            PkgBuildRunner runner = new PkgBuildRunner( context );
            runner.BuildArchPkg();
        }
    }

    public class PkgBuildRunner
    {
        // ---------------- Fields ----------------

        private readonly ChaskisContext context;

        private readonly ImportantPaths paths;

        private readonly DirectoryPath workDir;

        private readonly FilePath srcInfoFile;

        // ---------------- Constructor ----------------

        public PkgBuildRunner( ChaskisContext context )
        {
            this.context = context;
            this.paths = context.Paths;

            this.workDir = this.paths.ArchLinuxInstallConfigFolder.Combine(
                new DirectoryPath( "obj" )
            );

            this.srcInfoFile = this.workDir.CombineWithFilePath(
                new FilePath( ".SRCINFO" )
            );
        }

        // ---------------- Functions ----------------

        public void BuildArchPkg()
        {
            if( this.context.IsRunningOnWindows() )
            {
                throw new InvalidOperationException( "Can only build PKGBUILD package on Arch Linux." );
            }

            DirectoryPath outputFolder = this.paths.OutputPackages.Combine(
                new DirectoryPath( "arch_linux" )
            );

            // Create directories
            this.context.EnsureDirectoryExists( this.workDir );
            this.context.CleanDirectory( this.workDir );
            this.context.SetDirectoryPermission( this.workDir, "755" );
            this.context.EnsureDirectoryExists( outputFolder );
            this.context.CleanDirectory( outputFolder );

            // According the the makepkg documentation https://www.archlinux.org/pacman/makepkg.8.html
            // the build script must be in the same directory makepkg is called from.  We really
            // don't want all of that stuff to crowd the repo, so we'll stuff all the building into
            // the obj folder. However, this means we need to write the PKGBUILD file
            // into the obj folder.
            FilePath pkgFile = this.workDir.CombineWithFilePath( new FilePath( "PKGBUILD" ) );
            File.WriteAllText(
                pkgFile.ToString(),
                GetPkgBuildContents()
            );

            this.BuildPkgFile();
            this.BuildSrcInfo();

            FilePath glob = new FilePath( "*.pkg.tar.*" );
            FilePathCollection files = this.context.GetFiles( this.workDir.CombineWithFilePath( glob ).ToString() );

            if( files.Count != 1 )
            {
                throw new ApplicationException( "Found 2 files to glob, something weird happened. Got: " + files.Count );
            }

            FilePath buildPackageFile = files.First();
            this.context.Information( "Arch Pkg: " + buildPackageFile.ToString() );

            FilePath sha256File = new FilePath( buildPackageFile.ToString() + ".sha256" );
            this.context.GenerateSha256( buildPackageFile, sha256File );

            this.CopyFile( buildPackageFile, outputFolder );
            this.CopyFile( sha256File, outputFolder );
            this.CopyFile( this.srcInfoFile, outputFolder );
            this.CopyFile( pkgFile, outputFolder );

            // Also copy the deb file so its easy to commit that to the AUR repo later.
            this.CopyFile( this.paths.DebPath, outputFolder );
        }

        private string GetPkgBuildContents()
        {
            // First, need to set the checksum of the MSI file.
            string checksum = File.ReadAllText( this.context.Paths.DebChecksumFile.ToString() ).Trim();

            string pkgbuild =
$@"
# Maintainer: Seth Hendrick <seth@shendrick.net>

# This format is taken from msbuild, which also grabs a compiled .deb and installs it from that.
# https://aur.archlinux.org/cgit/aur.git/tree/PKGBUILD?h=msbuild-stable
# We do this because of https://github.com/mono/mono/issues/9280.  Arch linux's msbuild package
# doesn't include MSBuildSdkResolver and libhostfxr.so.  So, this is the best we can do without
# user having to install all kinds of weird things that are not in the AUR or in pacman.

pkgname=chaskis
pkgver={this.context.TemplateConstants.ChaskisVersion}
pkgrel=1
pkgdesc=""{this.context.TemplateConstants.Summary}""
arch=('any')
url=""{this.context.TemplateConstants.ProjectUrl}""
license=('BSL')
depends=('dotnet-runtime-3.1>=3.1.0')
provides=('chaskis')
conflicts=('chaskis')
# This means that chaskis.deb must be committed to the AUR repo.
source=(""chaskis.deb"")
sha256sums=('{checksum}')
validpgpkeys=()

package() {{
    cd ""${{srcdir}}""

    bsdtar xf data.tar.xz

    chmod -R g-w usr
    mv usr ""${{pkgdir}}""
}}
";
            // PKGBUILD files MUST end in \n, not \r\n.
            return pkgbuild.Replace( "\r\n", "\n" );
        }

        private void BuildPkgFile()
        {
            this.context.Information( "Building PKG file..." );

            ProcessSettings settings = new ProcessSettings
            {
                WorkingDirectory = this.workDir
            };
            int exitCode = this.context.StartProcess( "makepkg", settings );
            if( exitCode != 0 )
            {
                throw new ApplicationException(
                    "Could not package for Arch Linux, got exit code: " + exitCode
                );
            }

            this.context.Information( "Building PKG file... Done!" );
        }

        private void BuildSrcInfo()
        {
            this.context.Information( "Building .SRCINFO file..." );

            string arguments = "--printsrcinfo";
            ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );

            ProcessSettings settings = new ProcessSettings
            {
                Arguments = argumentsBuilder,
                WorkingDirectory = this.workDir,
                RedirectStandardOutput = true
            };

            IEnumerable<string> stdOut;
            int exitCode = this.context.StartProcess( "makepkg", settings, out stdOut );
            if( exitCode != 0 )
            {
                throw new ApplicationException(
                    "Could not make SRCINFO, got exit code: " + exitCode
                );
            }

            System.IO.File.WriteAllLines( this.srcInfoFile.ToString(), stdOut );

            this.context.Information( "Building .SRCINFO file... Done!" );
        }

        private void CopyFile( FilePath source, DirectoryPath destination )
        {
            this.context.Information( $"Moving '{source}' to '${destination}'" );
            this.context.CopyFileToDirectory( source, destination );
        }
    }
}
