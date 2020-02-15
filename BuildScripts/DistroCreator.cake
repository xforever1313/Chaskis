#load "Includes.cake"

public class DistroCreatorConfig
{
    // ---------------- Constructor ----------------

    // ---------------- Properties ----------------

    public string OutputLocation { get; set; }

    public string Target { get; set; } = "Debug";
}

public class DistroCreator
{
    // ---------------- Fields ----------------

    private readonly ICakeContext context;
    private readonly ImportantPaths paths;
    private readonly DistroCreatorConfig config;

    private readonly DirectoryPath output;
    private readonly FilePath wixFile;

    // ---------------- Constructor ----------------

    public DistroCreator(
        ICakeContext context,
        ImportantPaths paths,
        DistroCreatorConfig config
    )
    {
        this.context = context;
        this.paths = paths;
        this.config = config;

        this.output = new DirectoryPath( this.config.OutputLocation );
        if( this.context.IsRunningOnWindows() )
        {
            this.wixFile = this.paths.WixConfigFolder.CombineWithFilePath(
                new FilePath( "Product.wxs" )
            );
        }
        else
        {
            this.wixFile = this.paths.WixConfigFolder.CombineWithFilePath(
                new FilePath( "Product.wxs.linux" )
            );
        }
    }

    // ---------------- Functions ----------------

    public void CreateDistro()
    {
        string target = this.config.Target;
        this.context.EnsureDirectoryExists( this.output );
        this.context.CleanDirectory( this.output );
        if( this.context.IsRunningOnWindows() == false )
        {
            this.context.SetDirectoryPermission( this.output, "755" );
        }

        FilePath cliInstallerPath = this.paths.CliInstallerProjectFolder.CombineWithFilePath(
            $"bin/{target}/{frameworkTarget}/Chaskis.CliInstaller"
        );

        string arguments = 
            $"\"{this.paths.SourceFolder}\" \"{this.output.ToString()}\" \"{this.wixFile.ToString()}\" \"{target}\" \"{frameworkTarget}\" \"{pluginTarget}\"";

        string exeName;

        if( this.context.IsRunningOnWindows() )
        {
            exeName = cliInstallerPath.ToString() + ".exe";
        }
        else
        {
            exeName = cliInstallerPath.ToString();
        }

        this.context.Information( $"Starting CLI Installer at '{exeName}' with arguments: '{arguments}'" );

        ProcessArgumentBuilder argumentsBuilder = ProcessArgumentBuilder.FromString( arguments );
        ProcessSettings settings = new ProcessSettings
        {
            Arguments = argumentsBuilder
        };
        int exitCode = this.context.StartProcess( exeName, settings );
        if( exitCode != 0 )
        {
            throw new ApplicationException(
                "Error when creating distro.  Got error: " + exitCode
            );
        }
    }
}