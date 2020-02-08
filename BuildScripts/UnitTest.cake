public class UnitTestRunner
{
    // ---------------- Fields ----------------

    private readonly ICakeContext cakeContext;
    private readonly ImportantPaths paths;
    private readonly DirectoryPath unitTestDir;

    private readonly List<FilePath> testProjects;

    // ---------------- Constructor ----------------

    public UnitTestRunner( ICakeContext context, ImportantPaths paths )
    {
        this.cakeContext = context;
        this.paths = paths;
        this.unitTestDir = this.paths.UnitTestFolder;

        this.testProjects = new List<FilePath>
        {
            unitTestDir.CombineWithFilePath( new FilePath( "ChaskisTests/ChaskisTests.csproj" ) ),
            unitTestDir.CombineWithFilePath( new FilePath( "CoreTests/CoreTests.csproj" ) ),
            unitTestDir.CombineWithFilePath( new FilePath( "PluginTests/PluginTests.csproj" ) )
        };
    }

    // ---------------- Functions ----------------

    public void RunUnitTests()
    {
        this.RunUnitTestInternal( this.cakeContext );
    }

    private void RunUnitTestInternal( ICakeContext context )
    {
        DirectoryPath outputPath = this.paths.UnitTestResultFolder;
        context.EnsureDirectoryExists( outputPath );
        context.CleanDirectory( outputPath );

        List<Exception> failures = new List<Exception>();
        foreach( FilePath project in this.testProjects )
        {
            FilePath resultFile = new FilePath(
                outputPath.CombineWithFilePath( project.GetFilenameWithoutExtension() ).ToString() + ".xml"
            );

            DotNetCoreTestSettings settings = new DotNetCoreTestSettings
            {
                NoBuild = true,
                NoRestore = true,
                Configuration = "Debug",
                ResultsDirectory = outputPath,
                VSTestReportPath = resultFile
            };

            context.Information( $"Running unit tests for '{project.ToString()}'..." );
            try
            {
                context.DotNetCoreTest( project.ToString(), settings );
            }
            catch ( Exception e )
            {
                failures.Add( e );
            }
            context.Information( $"Running unit tests for '{project.ToString()}'...Done!" );
        }

        if( failures.Count != 0 )
        {
            throw new AggregateException( "Unit tests failed!", failures );
        }
    }

    public void RunCodeCoverage()
    {
        DirectoryPath codeCoveragePath = this.paths.CodeCoverageFolder;
        FilePath outputPath = codeCoveragePath.CombineWithFilePath( new FilePath( "coverage.xml" ) );
        this.cakeContext.EnsureDirectoryExists( codeCoveragePath );
        this.cakeContext.CleanDirectory( codeCoveragePath );

        OpenCoverSettings settings = new OpenCoverSettings
        {
            Register = "user",
            ReturnTargetCodeOffset = 0
        }
        .WithFilter( "+[*]Chaskis*" );

        this.cakeContext.OpenCover(
            context => this.RunUnitTestInternal( context ),
            outputPath,
            settings
        );

        this.cakeContext.ReportGenerator( outputPath, codeCoveragePath );
    }
}