//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core.IO;
using Cake.Frosting;


namespace DevOps.Tasks
{
    [TaskName( "specfile" )]
    [TaskDescription( "Creates the RPM package on Fedora" )]
    public sealed class SpecFileTask : DefaultTask
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
            else
            {
                return true;
            }
        }
        
        public override void Run( ChaskisContext context )
        {
            SpecFileRunner runner = new SpecFileRunner( context );
            runner.BuildSpecFile();
        }
    }

    public sealed class SpecFileRunner
    {
        // ---------------- Fields ----------------

        private readonly ChaskisContext context;

        private readonly ImportantPaths paths;

        // ---------------- Constructor ----------------

        public SpecFileRunner( ChaskisContext context )
        {
            this.context = context;
            this.paths = context.Paths;
        }

        // ---------------- Functions ----------------

        public void BuildSpecFile()
        {
            FilePath specFile = this.paths.SpecFileOutputPath;

            this.context.EnsureDirectoryExists( specFile.GetDirectory() );
            this.context.CleanDirectory( specFile.GetDirectory() );

            File.WriteAllText(
                specFile.ToString(),
                GetSpecFileContents()
            );
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
%define source https://files.shendrick.net/projects/chaskis/releases/{this.context.TemplateConstants.ChaskisVersion}/debian/chaskis.deb
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
    }
}
