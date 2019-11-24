// ---------------- Addins ----------------

#addin nuget:?package=Cake.ArgumentBinder&version=0.1.2
#addin nuget:?package=Cake.FileHelpers&version=3.2.1

// ---------------- Tools ----------------

#tool nuget:?package=Cake.Bakery&version=0.4.1
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.9.0"
#tool "nuget:?package=OpenCover&version=4.6.519"
#tool "nuget:?package=ReportGenerator&version=4.0.10"

// ---------------- Includes ----------------

#load "Common.cake"
#load "ImportantPaths.cake"
#load "MSBuild.cake"
#load "Templatize.cake"
#load "UnitTest.cake"
