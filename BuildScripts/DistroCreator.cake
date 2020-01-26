#load "Includes.cake"

public class DistroCreatorConfig
{
    // ---------------- Constructor ----------------

    // ---------------- Properties ----------------

    public bool IsWindows { get; set; }

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
        if( this.config.IsWindows )
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

        FilePath cliInstallerPath = this.paths.CliInstallerProjectFolder.CombineWithFilePath(
            $"bin/{target}/{frameworkTarget}/Chaskis.CliInstaller.exe"
        );

        string arguments = 
            $"\"{this.paths.SourceFolder}\" \"{this.output.ToString()}\" \"{this.wixFile.ToString()}\" \"{target}\" \"{frameworkTarget}\" \"{pluginTarget}\"";

        string exeName;

        if( this.config.IsWindows )
        {
            exeName = cliInstallerPath.ToString();
        }
        else
        {
            exeName = "mono";
            arguments = $"\"{cliInstallerPath.ToString()}\" " + arguments;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            Arguments = arguments,
            CreateNoWindow = true,
            FileName = exeName,
            UseShellExecute = false
        };

        using( Process process = Process.Start( startInfo ) )
        {
            process.WaitForExit();

            if( process.ExitCode != 0 )
            {
                throw new ApplicationException(
                    "Error when creating distro.  Got error: " + process.ExitCode
                );
            }
        }
    }
}