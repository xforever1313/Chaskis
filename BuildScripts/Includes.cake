// ---------------- Addins ----------------

#addin nuget:?package=Cake.ArgumentBinder&version=0.1.2
#addin nuget:?package=Cake.FileHelpers&version=3.2.1

// ---------------- Tools ----------------

// For intellisense in VS Code
#tool nuget:?package=Cake.Bakery&version=0.4.1

// For unit tests
#tool nuget:?package=NUnit.ConsoleRunner&version=3.9.0
#tool nuget:?package=OpenCover&version=4.6.519
#tool nuget:?package=ReportGenerator&version=4.0.10

// For regression tests
#tool nuget:?package=NetRunner&version=1.0.11

// ---------------- Using Statements ----------------

using Cake.ArgumentBinder;
using System.Diagnostics;
using System.Text.RegularExpressions;

// ---------------- Includes ----------------

#load "Common.cake"
#load "Deb.cake"
#load "DistroCreator.cake"
#load "ImportantPaths.cake"
#load "MSBuild.cake"
#load "RegressionTest.cake"
#load "Templatize.cake"
#load "UnitTest.cake"

// ---------------- Globals ----------------

const string frameworkTarget = "netcoreapp3.1";
const string pluginTarget = "netstandard2.0";
