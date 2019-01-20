// ---------------- Arguments ----------------

const string defaultTarget = "default";
const string frameworkTarget = "net471";
string target = Argument( "target", defaultTarget );

// ---------------- Globals ----------------

DirectoryPath projectRoot = MakeAbsolute( new DirectoryPath( "." ) );
DirectoryPath sourceRoot = projectRoot.Combine( new DirectoryPath( "Chaskis" ) );
FilePath solution = sourceRoot.CombineWithFilePath( new FilePath( "Chaskis.sln" ) );
DirectoryPath unitTestDir = sourceRoot.Combine( new DirectoryPath( "UnitTests" ) );

bool isWindows = ( Environment.OSVersion.Platform == PlatformID.Win32NT );

// ---------------- Includes ----------------

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
    () =>
    {
        RunUnitTests();
    }
)
.Description( "Runs all the unit tests (does not run code coverage)." )
.IsDependentOn( "debug" );

Task( "code_coverage" )
.Does(
    () =>
    {
        RunCodeCoverage();
    }
)
.Description( "Runs code coverage, Windows only." )
.WithCriteria( isWindows )
.IsDependentOn( "debug" );

Task( defaultTarget )
.IsDependentOn( "debug" )
.Description( "The default target; alias for 'debug'." );

RunTarget( target );