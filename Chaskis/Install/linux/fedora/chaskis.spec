%define name chaskis
%define version 0.3.0
%define unmangled_version 0.3.0
%define release 1
%define source https://github.com/xforever1313/Chaskis/archive/%{unmangled_version}.tar.gz
%define untardir Chaskis-%{unmangled_version}
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
BuildRequires: nuget git mono-devel wget
Vendor: Seth Hendrick <seth@shendrick.net>
Url: https://github.com/xforever1313/Chaskis/

# RPM, you're not smart, you are not to be trusted with this.
# Otherwise, it Requires Mono(System.Collections) to be provided, which mono-devel
# does not provide apparently (lolwut).
AutoReq: no

%description
Chaskis is a framework for creating IRC Bots in an easy way.  It is a plugin-based architecture written in C# that can be run on Windows or Linux (with the use of Mono).  Users of the bot can add or remove plugins to run, or even write their own.

Chaskis is named after the [Chasqui](https://en.wikipedia.org/wiki/Chasqui), messengers who ran trails in the Inca Empire to deliver messages.

%prep
wget %{source} -O %{_sourcedir}/%{unmangled_version}.tar.gz

%check
cd %{untardir}/Chaskis
mono ./packages/NUnit.ConsoleRunner.3.5.0/tools/nunit3-console.exe ./Tests/bin/Release/Tests.dll

%build
mkdir %{_builddir}/%{untardir}
tar -xvf %{_sourcedir}/%{unmangled_version}.tar.gz -C %{_builddir}/
cd %{untardir}
git clone https://github.com/xforever1313/sethcs SethCS

# Fedora 26 has a woefully out-of-date version of nuget.
# We need to grab it ourselves like a savage.
wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
mono ./nuget.exe restore ./Chaskis/Chaskis.sln
xbuild /p:Configuration=Release ./Chaskis/Chaskis.sln

%install
cd %{untardir}
mkdir -p %{buildroot}%{libdir}
mono ./Chaskis/Install/ChaskisCliInstaller/bin/Release/ChaskisCliInstaller.exe ./Chaskis %{buildroot}/%{libdir} ./Chaskis/Install/windows/Product.wxs Release

mkdir -p %{buildroot}%{libdir}systemd/user
cp ./Chaskis/Install/linux/systemd/chaskis.service %{buildroot}%{libdir}/systemd/user/chaskis.service

mkdir -p %{buildroot}%{_bindir}/
cp ./Chaskis/Install/linux/bin/chaskis %{buildroot}%{_bindir}/chaskis

%files
%{libdir}/Chaskis/*
%{_bindir}/chaskis
%{libdir}/systemd/user/chaskis.service

