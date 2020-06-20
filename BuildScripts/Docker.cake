var buildWindowsRuntimeTask = Task( "build_windows_docker" )
.Does(
    ( context ) =>
    {
        DirectoryPath output = paths.OutputPackages.Combine( new DirectoryPath( "windows" ) );
        output = output.Combine( new DirectoryPath( "docker" ) );

        DistroCreatorConfig config = new DistroCreatorConfig
        {
            OutputLocation = output.ToString(),
            Target = "Release"
        };

        DistroCreator creator = new DistroCreator( context, paths, config );
        creator.CreateDistro();

        string arguments = "build -t chaskis.windows -f .\\Docker\\WindowsRuntime.Dockerfile .";
        ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );
        ProcessSettings settings = new ProcessSettings
        {
            Arguments = argumentsBuilder
        };
        int exitCode = StartProcess( "docker", settings );
        if( exitCode != 0 )
        {
            throw new ApplicationException(
                "Error when running docker.  Got error: " + exitCode
            );
        }
    }
)
.Description( "Builds the Windows Runtime Docker Container" );
if( isJenkins == false )
{
    buildWindowsRuntimeTask.IsDependentOn( "Release" );
}

var buildUbuntuRuntimeTask = Task( "build_ubuntu_docker" )
.Does(
    () =>
    {
        string arguments = "build -t chaskis.ubuntu -f .\\Docker\\UbuntuRuntime.Dockerfile .";
        ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );
        ProcessSettings settings = new ProcessSettings
        {
            Arguments = argumentsBuilder
        };
        int exitCode = StartProcess( "docker", settings );
        if( exitCode != 0 )
        {
            throw new ApplicationException(
                "Error when running docker.  Got error: " + exitCode
            );
        }
    }
).Description( "Builds the Ubuntu Runtime Docker Container" );

if( isJenkins == false )
{
    // DO NOT COMMIT THIS COMMENTED OUT
    //buildUbuntuRuntimeTask.IsDependentOn( "debian_pack" );
}
