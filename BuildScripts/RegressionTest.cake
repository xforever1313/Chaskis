
public class FitnesseConfig
{
    // ---------------- Constructor ----------------

    public FitnesseConfig()
    {
    }

    // ---------------- Properties ----------------

    [IntegerArgument(
        "port",
        Description = "Port to run Fitnesse on",
        DefaultValue = 10013,
        Min = 1,
        Max = ushort.MaxValue
    )]
    public int Port { get; set; }

    [IntegerArgument(
        "days",
        Description = "The expiration date for page versions.  Deleted after this number of days",
        DefaultValue = 1,
        Min = 1
    )]
    public int Days { get; set; }

    [BooleanArgument(
        "output_log",
        Description = "Set to true to output the log to the test results directory.",
        DefaultValue = true
    )]
    public bool OutputLog { get; set; }

    /// <summary>
    /// The command to send, if any.
    /// </summary>
    public string Command { get; set; }
}

public class FitnesseRunner : IDisposable
{
    // ---------------- Fields ----------------

    /// <summary>
    /// Called when the process starts.
    /// </summary>
    public event Action OnStarted;

    private readonly ICakeContext context;
    private readonly FitnesseConfig config;
    private readonly ImportantPaths paths;

    private readonly ProcessStartInfo startInfo;
    private Process process;

    // ---------------- Constructor ----------------

    public FitnesseRunner( ICakeContext context, ImportantPaths paths, FitnesseConfig config )
    {
        this.context = context;
        this.paths = paths;
        this.config = config;

        this.startInfo = new ProcessStartInfo
        {
            Arguments = BuildArguments(),
            CreateNoWindow = true,
            FileName = "java",
            UseShellExecute = false
        };
    }

    // ---------------- Functions ----------------

    public void StartProcess()
    {
        this.context.Information( $"Invoking FitNesse with arguments: '{this.startInfo.Arguments}'" );
        this.process = Process.Start( this.startInfo );
        this.OnStarted?.Invoke();
    }

    public int Wait()
    {
        if( this.process == null )
        {
            throw new InvalidOperationException( "Process not started!" );
        }

        this.process.WaitForExit();
        int exitCode = this.process.ExitCode;
        this.process = null;

        return exitCode;
    }

    public void Dispose()
    {
        this.process?.Kill();
        this.process?.Dispose();
    }

    private string BuildArguments()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append( "-jar");
        builder.Append( " " );
        builder.Append(
            this.paths.RegressionTestFolder.CombineWithFilePath( "fitnesse-standalone.jar" ).ToString()
        );
        
        // Port
        builder.Append( " " );
        builder.Append( "-p" );
        builder.Append( " " );
        builder.Append( this.config.Port.ToString() );

        // Days
        builder.Append( " " );
        builder.Append( "-e" );
        builder.Append( " " );
        builder.Append( this.config.Days.ToString() );
        
        // Command
        if( string.IsNullOrWhiteSpace( this.config.Command ) == false )
        {
            builder.Append( " " );
            builder.Append( "-c" );
            builder.Append( " " );
            builder.Append( this.config.Command );
        }

        // Output
        if( this.config.OutputLog )
        {
            builder.Append( " " );
            builder.Append( "-b" );
            builder.Append( " " );
            builder.Append( this.paths.RegressionTestResultsFile.ToString() );
        }

        // Working Directory
        builder.Append( " " );
        builder.Append( "-r" );
        builder.Append( " " );
        builder.Append( this.context.Environment.WorkingDirectory.GetRelativePath( this.paths.FitNesseRoot ).ToString() );

        return builder.ToString();
    }
}

const string bootStrapTask = "bootstrap_fitnesse";

Task( bootStrapTask )
.Does(
    ( context ) =>
    {
        void CopyRunnerFile( string fileName )
        {
            FilePath path = paths.RegressionTestRunnerDirectory.CombineWithFilePath( File( fileName ) );
            if( FileExists( path ) == false )
            {
                CopyFile(
                    context.Tools.Resolve( fileName ),
                    path
                );
            }
        }

        EnsureDirectoryExists( paths.RegressionTestRunnerDirectory );

        CopyRunnerFile( "NetRunner.ExternalLibrary.dll" );
        CopyRunnerFile( "NetRunner.ExternalLibrary.XML" );
        CopyRunnerFile( "NetRunner.Executable.exe" );
    }
).Description( "Ensures our Environment is ready to run with FitNesse." );

Task( "launch_fitnesse" )
.Does(
    ( context ) =>
    {
        FitnesseConfig config = ArgumentBinder.FromArguments<FitnesseConfig>( context );
        config.OutputLog = false;

        using( FitnesseRunner runner = new FitnesseRunner( context, paths, config ) )
        {
            runner.OnStarted += () =>
            {
                using( Process.Start( "http://localhost:" + config.Port ) )
                {
                    Console.WriteLine( "Browser should have opened." );
                    Console.WriteLine( "Press any key to stop FitNesse" );
                }
                Console.ReadKey();
            };

            runner.StartProcess();
        }
    }
).Description(
    ArgumentBinder.GetDescription<FitnesseConfig>( "Launches FitNesse to be run interactively." )
).IsDependentOn( bootStrapTask );

Task( "run_regression" )
.Does(
    ( context ) =>
    {
        FitnesseConfig config = ArgumentBinder.FromArguments<FitnesseConfig>( context );
        config.Command = "ChaskisTests.AllTests?suite";

        int exitCode = -1;
        using( FitnesseRunner runner = new FitnesseRunner( context, paths, config ) )
        {
            runner.StartProcess();
            exitCode = runner.Wait();
        }

        if( exitCode != 0 )
        {
            throw new ApplicationException( "FitNesse did not exit cleanly, got exit code: " + exitCode );
        }
    }
).Description(
    ArgumentBinder.GetDescription<FitnesseConfig>( "Runs all regression tests." )
).IsDependentOn( bootStrapTask );
