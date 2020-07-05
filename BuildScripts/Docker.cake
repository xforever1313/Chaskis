const string windowsDockerImageName = "xforever1313/chaskis.windows";
const string ubuntuDockerImageName = "xforever1313/chaskis.ubuntu";

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

        Information( "Running Docker..." );
        BuildDockerImage( windowsDockerImageName, ".\\Docker\\WindowsRuntime.Dockerfile" );
        TagDocker( context, windowsDockerImageName );
        Information( "Running Docker... Done!" );
    }
)
.Description( "Builds the Windows Runtime Docker Container" );
if( isJenkins == false )
{
    buildWindowsRuntimeTask.IsDependentOn( "Release" );
}

var buildUbuntuRuntimeTask = Task( "build_ubuntu_docker" )
.Does(
    ( context ) =>
    {
        BuildDockerImage( ubuntuDockerImageName, "./Docker/UbuntuRuntime.Dockerfile" );
        TagDocker( context, ubuntuDockerImageName );
    }
).Description( "Builds the Ubuntu Runtime Docker Container" );

if( isJenkins == false )
{
    buildUbuntuRuntimeTask.IsDependentOn( "debian_pack" );
}

private void BuildDockerImage( string imageName, string dockerFile )
{
    string arguments = $"build -t {imageName} -f {dockerFile} .";
    ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );
    ProcessSettings settings = new ProcessSettings
    {
        Arguments = argumentsBuilder
    };
    int exitCode = StartProcess( "docker", settings );
    if( exitCode != 0 )
    {
        throw new ApplicationException(
            "Error when running docker to build.  Got error: " + exitCode
        );
    }
}

private void TagDocker( ICakeContext context, string imageName )
{
    TemplateConstants templateConstants = new TemplateConstants(
        context,
        paths,
        frameworkTarget
    );

    string arguments = $"tag {imageName}:latest {imageName}:{templateConstants.ChaskisVersion}";
    ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );
    ProcessSettings settings = new ProcessSettings
    {
        Arguments = argumentsBuilder
    };
    int exitCode = StartProcess( "docker", settings );
    if( exitCode != 0 )
    {
        throw new ApplicationException(
            "Error when running docker to tag.  Got error: " + exitCode
        );
    }
}