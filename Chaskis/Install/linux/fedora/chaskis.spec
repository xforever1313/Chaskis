%define name chaskis
%define version 0.20.0
%define unmangled_version 0.20.0
%define release 1
%define source https://files.shendrick.net/projects/chaskis/releases/%{unmangled_version}/linux/debian/chaskis.deb
%define libdir /usr/lib/

Summary: A generic framework written in C# for making IRC Bots.
Name: %{name}
Version: %{version}
Release: %{release}
Source0: %{source}
License: BSL
Prefix: %{_prefix}
BuildArch: noarch
Requires: mono-core
BuildRequires: tar wget
Vendor: Seth Hendrick <seth@shendrick.net>
Url: https://github.com/xforever1313/Chaskis/

# Since there is already a .deb file that we compile and upload to our server,
# there is no need to recompile.  Just unpack the .deb and call it a day.

%description
Chaskis is a framework for creating IRC Bots in an easy way.  It is a plugin-based architecture written in C# that can be run on Windows or Linux (with the use of Mono).  Users of the bot can add or remove plugins to run, or even write their own.

Chaskis is named after the [Chasqui](https://en.wikipedia.org/wiki/Chasqui), messengers who ran trails in the Inca Empire to deliver messages.

%prep
# OpenSuse apparently doesn't like Let's Encrypt for some reason, so hence the --no-check-certificate.
wget %{source} -O %{_sourcedir}/chaskis.deb --no-check-certificate

%check
cd %{_sourcedir}
echo 'fe03a07766eeb681da06c02de35ce03fbc298803d8902318733b1c5107c05b4e  chaskis.deb' | sha256sum --check

%build
# unarchive the .deb file.  The .deb file
# has files that need to be installed in the data.tar.xz file.
# put that through tar, and everything will end up in a
# usr directory.
cd %{_sourcedir}
ar p %{_sourcedir}/chaskis.deb data.tar.xz | tar xJ -C %{_builddir}
chmod -R g-w %{_builddir}/usr

%install
mv %{_builddir}/usr %{buildroot}/usr

%files
%{libdir}/Chaskis/*
%{_bindir}/chaskis
%{libdir}/systemd/user/chaskis.service
