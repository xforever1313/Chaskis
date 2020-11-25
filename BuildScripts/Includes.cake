// ---------------- Addins ----------------

#addin nuget:?package=Cake.ArgumentBinder&version=0.2.2
#addin nuget:?package=Cake.FileHelpers&version=3.2.1
#addin nuget:?package=Cake.LicenseHeaderUpdater&version=0.0.1

// ---------------- Tools ----------------

// For intellisense in VS Code
#tool nuget:?package=Cake.Bakery&version=0.4.1

// So we can copy over to our docker build container
// Note: if the version changes, it will need to changed
// in the docker file as well.
#tool nuget:?package=NuGet.CommandLine&version=5.5.1

// For unit tests
#tool nuget:?package=NUnit.ConsoleRunner&version=3.9.0
#tool nuget:?package=OpenCover&version=4.6.519
#tool nuget:?package=ReportGenerator&version=4.0.10

// ---------------- Using Statements ----------------

using Cake.ArgumentBinder;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

// ---------------- Includes ----------------

#load "Common.cake"
#load "Deb.cake"
#load "Docker.cake"
#load "DistroCreator.cake"
#load "ImportantPaths.cake"
#load "LicenseHeader.cake"
#load "MSBuild.cake"
#load "Pkgbuild.cake"
#load "RegressionTest.cake"
#load "TestRunner.cake"
#load "Templatize.cake"
#load "UnitTest.cake"
#load "VersionDump.cake"

// ---------------- Globals ----------------

const string frameworkTarget = "netcoreapp3.1";
const string pluginTarget = "netstandard2.0";
