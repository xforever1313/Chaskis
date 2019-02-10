// ---------------- Arguments ----------------

const string defaultTarget = "default";
const string frameworkTarget = "net471";
string target = Argument( "target", defaultTarget );

// ---------------- Globals ----------------

#load "BuildScripts/ImportantPaths.cake"

ImportantPaths paths = new ImportantPaths( MakeAbsolute( new DirectoryPath( "." ) ) );

bool isWindows = ( Environment.OSVersion.Platform == PlatformID.Win32NT );

// ---------------- Includes ----------------

#addin "Cake.FileHelpers&version=3.1.0"

#load "BuildScripts/Common.cake"
#load "BuildScripts/MSBuild.cake"
#load "BuildScripts/UnitTest.cake"

// ---------------- Targets ----------------

Task( "debug" )
.Does(
    () =>
    {
        DoMsBuild( "Debug" );
    }
)
.Description( "Builds Chaskis with Debug turned on." );

Task( "release" )
.Does(
    () =>
    {
        DoMsBuild( "Release" );
    }
)
.Description( "Builds Chaskis with Release turned on." );

Task( "unit_test" )
.Does(
    ( context ) =>
    {
        UnitTestRunner runner = new UnitTestRunner( context, paths );
        runner.RunUnitTests();
    }
)
.Description( "Runs all the unit tests (does not run code coverage)." )
.IsDependentOn( "debug" );

Task( "msi" )
.Does(
    () =>
    {
        DirectoryPath msiLocation = paths.InstallConfigFolder.Combine( "windows/bin/x64/Release" );

        DoMsBuild( "Install", PlatformTarget.x64 );
        GenerateSha256(
            msiLocation.CombineWithFilePath( "ChaskisInstaller.msi" ),
            msiLocation.CombineWithFilePath( "ChaskisInstaller.msi.sha256" )
        );
    }
)
.Description( "Builds the MSI on Windows." )
.WithCriteria( isWindows )
.IsDependentOn( "unit_test" );

Task( "code_coverage" )
.Does(
    ( context ) =>
    {
        UnitTestRunner runner = new UnitTestRunner( context, paths );
        runner.RunCodeCoverage();
    }
)
.Description( "Runs code coverage, Windows only." )
.WithCriteria( isWindows )
.IsDependentOn( "debug" );

Task( defaultTarget )
.IsDependentOn( "debug" )
.Description( "The default target; alias for 'debug'." );

RunTarget( target );