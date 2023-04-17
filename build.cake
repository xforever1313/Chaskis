// ---------------- Includes ----------------

#load Chaskis/DevOps/MsBuildHelpers.cs

// ---------------- Constants ----------------

const string target = "run_devops";
const string buildTask = "build";
bool forceBuild = Argument<bool>( "force_build", false );
string targetArg = Argument( "target", string.Empty );

FilePath devopsExe = File( "./Chaskis/DevOps/bin/Debug/net6.0/DevOps.dll" );
FilePath sln = File( "./Chaskis/Chaskis.sln" );

// ---------------- Targets ----------------

Task( buildTask )
.Does(
    ( context ) =>
    {
        if( forceBuild == false )
        {
            Information( "DevOps.dll not found, compiling" );
        }

        MsBuildHelpers.DoMsBuild( context, sln, "Debug" );
    }
)
.Description( "Builds Chaskis with Debug turned on." );

var runTask = Task( target )
.Does(
    () =>
    {
        List<string> args = new List<string>( System.Environment.GetCommandLineArgs() );
        args.RemoveAt( 0 );
        args.Insert( 0, devopsExe.ToString() );

        ProcessSettings processSettings = new ProcessSettings
        {
            Arguments = ProcessArgumentBuilder.FromStrings( args )
        };

        int exitCode = StartProcess( "dotnet", processSettings );
        if( exitCode != 0 )
        {
            throw new Exception( $"DevOps.exe Exited with exit code: {exitCode}" );
        }
    }
);

if( forceBuild || ( FileExists( devopsExe ) == false ) )
{
    runTask.IsDependentOn( buildTask );
}

// ---------------- Run ----------------

if( targetArg == buildTask )
{
    RunTarget( targetArg );
}
else
{
    RunTarget( target );
}
