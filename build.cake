// ---------------- Arguments ----------------

const string defaultTarget = "default";
string target = Argument( "target", defaultTarget );

// ---------------- Globals ----------------

DirectoryPath projectRoot = MakeAbsolute( new DirectoryPath( "." ) );
DirectoryPath sourceRoot = projectRoot.Combine( new DirectoryPath( "Chaskis" ) );
FilePath solution = sourceRoot.CombineWithFilePath( new FilePath( "Chaskis.sln" ) );

// ---------------- Includes ----------------

#load "BuildScripts/MSBuild.cake"

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

Task( defaultTarget )
.IsDependentOn( "debug" )
.Description( "The default target; alias for 'debug'." );

RunTarget( target );