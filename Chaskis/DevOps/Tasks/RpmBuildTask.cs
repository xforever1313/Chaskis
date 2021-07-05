//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Linq;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core.IO;
using Cake.Frosting;

namespace DevOps.Tasks
{
    [TaskName( "rpmbuild" )]
    [TaskDescription( "Creates the RPM package on Fedora" )]
    public class RpmBuildTask : DefaultTask
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
            var runner = new RpmRunner( context );
            runner.BuildRpmPkg();
        }
    }

    public class RpmRunner
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

            // Need the .deb file to exist in the source folder
            this.context.CopyFileToDirectory( this.context.Paths.DebPath, this.workDir );

            FilePath specFile = this.workDir.CombineWithFilePath(
                new FilePath( "chaskis.spec" )
            );
            File.WriteAllText(
                specFile.ToString(),
                GetSpecFileContents()
            );

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
            this.CopyFile( specFile, outputFolder );
        }

        private string GetSpecFileContents()
        {
            string checksum = File.ReadAllText( this.context.Paths.DebChecksumFile.ToString() ).Trim();

            return
@$"
%define name chaskis
%define version {this.context.TemplateConstants.ChaskisVersion}
%define unmangled_version {this.context.TemplateConstants.ChaskisVersion}
%define release 1
%define source chaskis.deb
%define libdir /usr/lib/
%define _binaries_in_noarch_packages_terminate_build   0

Summary: {this.context.TemplateConstants.Summary}
Name: %{{name}}
Version: %{{version}}
Release: %{{release}}
Source0: %{{source}}
License: BSL
Prefix: %{{_prefix}}
BuildArch: noarch
Requires: dotnet-runtime-3.1
BuildRequires: tar
Vendor: {this.context.TemplateConstants.Author} {this.context.TemplateConstants.AuthorEmail}
Url: {this.context.TemplateConstants.ProjectUrl}

# Since there is already a .deb file that we compile and upload to our server,
# there is no need to recompile.  Just unpack the .deb and call it a day.

%description
Chaskis is a framework for creating IRC Bots in an easy way.  It is a plugin-based architecture written in C# that can be run on Windows or Linux.  Users of the bot can add or remove plugins to run, or even write their own.

Chaskis is named after the [Chasqui](https://en.wikipedia.org/wiki/Chasqui), messengers who ran trails in the Inca Empire to deliver messages.

%prep

%check
cd %{{_sourcedir}}
echo '{checksum} chaskis.deb' | sha256sum --check

%build
# unarchive the .deb file.  The .deb file
# has files that need to be installed in the data.tar.xz file.
# put that through tar, and everything will end up in a
# usr directory.
cd %{{_sourcedir}}
ar p %{{_sourcedir}}/chaskis.deb data.tar.xz | tar xJ -C %{{_builddir}}
chmod -R g-w %{{_builddir}}/usr
chmod -R g-w %{{_builddir}}/bin

%install
mv %{{_builddir}}/usr %{{buildroot}}/usr
mv %{{_builddir}}/bin %{{buildroot}}/usr/bin

%files
%{{libdir}}/Chaskis/*
%{{_bindir}}/chaskis
%{{libdir}}/systemd/user/chaskis.service
";
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
