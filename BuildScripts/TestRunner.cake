public class TestRunner
{
    // ---------------- Fields ----------------

    private readonly ICakeContext cakeContext;
    private readonly ImportantPaths paths;
    
    private readonly DirectoryPath resultsDir;
    private readonly FilePath testProject;

    // ---------------- Constructor ----------------

    public TestRunner( ICakeContext context, ImportantPaths paths, string testContext, FilePath testCsProjectPath )
    {
        this.cakeContext = context;
        this.paths = paths;

        this.resultsDir = this.paths.TestResultFolder.Combine( new DirectoryPath( testContext ) );
        this.testProject = testCsProjectPath;
    }

    // ---------------- Functions ----------------

    public void RunTests()
    {
        CreateResultsDir();
        this.RunTestsInternal( this.cakeContext );
    }

    private void RunTestsInternal( ICakeContext context )
    {
        FilePath resultFile = new FilePath(
            this.resultsDir.CombineWithFilePath( this.testProject.GetFilenameWithoutExtension() ).ToString() + ".xml"
        );

        DotNetCoreTestSettings settings = new DotNetCoreTestSettings
        {
            NoBuild = true,
            Configuration = "Debug",
            ResultsDirectory = this.resultsDir,
            VSTestReportPath = resultFile,
            Verbosity = DotNetCoreVerbosity.Normal
        };

        // Need to restore to download the TestHost, which is a NuGet package.
        settings.NoRestore = false;

        context.Information( "Running Tests..." );
        context.DotNetCoreTest( this.testProject.ToString(), settings );
        context.Information( "Running Tests...Done!" );
    }

    public void RunCodeCoverage()
    {
        CreateResultsDir();
        DirectoryPath codeCoveragePath = this.resultsDir.Combine( new DirectoryPath( "CodeCoverage" ) );
        FilePath outputPath = codeCoveragePath.CombineWithFilePath( new FilePath( "coverage.xml" ) );
        this.cakeContext.EnsureDirectoryExists( codeCoveragePath );
        this.cakeContext.CleanDirectory( codeCoveragePath );

        OpenCoverSettings settings = new OpenCoverSettings
        {
            Register = "user",
            ReturnTargetCodeOffset = 0,
            OldStyle = true // This is needed or MissingMethodExceptions get thrown everywhere for some reason.
        }
        .WithFilter( "+[*]Chaskis*" );

        this.cakeContext.OpenCover(
            context => this.RunTestsInternal( context ),
            outputPath,
            settings
        );

        this.cakeContext.ReportGenerator( outputPath, codeCoveragePath );
    }

    private void CreateResultsDir()
    {
        DirectoryPath outputPath = this.resultsDir;
        this.cakeContext.EnsureDirectoryExists( outputPath );
        this.cakeContext.CleanDirectory( outputPath );
    }
}