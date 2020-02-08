/// <summary>
/// Calls MSBuild to compile Chaskis
/// </summary>
/// <param name="configuration">The configuration to use (e.g. Debug, Release, etc.).</param>
void DoMsBuild( string configuration )
{
    DotNetCoreMSBuildSettings msBuildSettings = new DotNetCoreMSBuildSettings
    {
        WorkingDirectory = paths.SourceFolder
    }
    .SetMaxCpuCount( System.Environment.ProcessorCount )
    .SetConfiguration( configuration );

    DotNetCoreBuildSettings settings = new DotNetCoreBuildSettings
    {
        MSBuildSettings = msBuildSettings
    };

    DotNetCoreBuild( paths.SolutionPath.ToString(), settings );
}