/// <summary>
/// Calls MSBuild to compile Chaskis
/// </summary>
/// <param name="configuration">The configuration to use (e.g. Debug, Release, etc.).</param>
/// <param name="target">The platform target to compile. Defaulted to MSIL (ANY CPU).</param>
void DoMsBuild( string configuration, PlatformTarget target = PlatformTarget.MSIL )
{
    MSBuildSettings settings = new MSBuildSettings
    {
        Configuration = configuration,
        MaxCpuCount = 0,
        PlatformTarget = target,
        Restore = true,
        WorkingDirectory = paths.SourceFolder
    };

    MSBuild( paths.SolutionPath, settings );
}