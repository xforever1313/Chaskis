%define name chaskis
%define version 0.3.0
%define unmangled_version 0.3.0
%define release 1

Summary: A generic framework written in C# for making IRC Bots.
Name: %{name}
Version: %{version}
Release: %{release}
Source0: https://github.com/xforever1313/Chaskis/archive/%{unmangled_version}.tar.gz
License: BSL
BuildRoot: %{_tmppath}/%{name}-%{version}-%{release}-buildroot
Prefix: %{_prefix}
BuildArch: noarch
Requires: mono-core
BuildRequires: nuget git mono-devel
Vendor: Seth Hendrick <seth@shendrick.net>
Url: https://github.com/xforever1313/Chaskis/

%description
Chaskis is a framework for creating IRC Bots in an easy way.  It is a plugin-based architecture written in C# that can be run on Windows or Linux (with the use of Mono).  Users of the bot can add or remove plugins to run, or even write their own.

Chaskis is named after the [Chasqui](https://en.wikipedia.org/wiki/Chasqui), messengers who ran trails in the Inca Empire to deliver messages.

%prep
%setup -n %{name}-%{unmangled_version} -n %{name}-%{unmangled_version}

%check
cd Chaskis
mono ./packages/NUnit.ConsoleRunner.3.5.0/tools/nunit3-console.exe ./Tests/bin/Release/Tests.dll

%build
git submodule update --init SethCS
nuget restore ./Chaskis/Chaskis.sln
xbuild /p:Configuration=Release ./Chaskis/Chaskis.sln

%install
mkdir -p %{buildroot}%{_libdir}
mono ./Chaskis/Install/ChaskisCliInstaller/bin/Release/ChaskisCliInstaller.exe ./Chaskis %{buildroot}/%{_libdir} ./Chaskis/Install/windows/Product.wxs Release

mkdir -p %{buildroot}%{_libdir}systemd/user
cp ./Chaskis/Install/linux/systemd/chaskis.service %{buildroot}%{_libdir}systemd/user/chaskis.service

mkdir -p %{buildroot}%{_bindir}/
cp ./Chaskis/Install/linux/bin/chaskis %{buildroot}%{_bindir}/chaskis

%files
%{_libdir}/Chaskis/*
%{_bindir}/chaskis
%{_libdir}/systemd/user/chaskis.service

