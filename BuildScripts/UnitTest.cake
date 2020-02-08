public class UnitTestRunner
{
    // ---------------- Fields ----------------

    private readonly ICakeContext cakeContext;
    private readonly ImportantPaths paths;
    private readonly DirectoryPath unitTestDir;

    private readonly FilePath testProject;

    // ---------------- Constructor ----------------

    public UnitTestRunner( ICakeContext context, ImportantPaths paths )
    {
        this.cakeContext = context;
        this.paths = paths;
        this.unitTestDir = this.paths.UnitTestFolder;

        this.testProject = unitTestDir.CombineWithFilePath( new FilePath( "Chaskis.UnitTests.csproj" ) );
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

        FilePath resultFile = new FilePath(
            outputPath.CombineWithFilePath( this.testProject.GetFilenameWithoutExtension() ).ToString() + ".xml"
        );

        DotNetCoreTestSettings settings = new DotNetCoreTestSettings
        {
            NoBuild = true,
            NoRestore = true,
            Configuration = "Debug",
            ResultsDirectory = outputPath,
            VSTestReportPath = resultFile
        };

        context.DotNetCoreTest( this.testProject.ToString(), settings );
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
            ReturnTargetCodeOffset = 0,
            OldStyle = true // This is needed or MissingMethodExceptions get thrown everywhere for some reason.
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