#tool "nuget:?package=NUnit.ConsoleRunner&version=3.9.0"
#tool "nuget:?package=OpenCover&version=4.6.519"
#tool "nuget:?package=ReportGenerator&version=4.0.10"

public class UnitTestRunner
{
    // ---------------- Fields ----------------

    private readonly ICakeContext cakeContext;
    private readonly ImportantPaths paths;
    private readonly DirectoryPath unitTestDir;

    private readonly List<FilePath> testAssemblies;

    // ---------------- Constructor ----------------

    public UnitTestRunner( ICakeContext context, ImportantPaths paths )
    {
        this.cakeContext = context;
        this.paths = paths;
        this.unitTestDir = this.paths.UnitTestFolder;

        testAssemblies = new List<FilePath>
        {
            unitTestDir.CombineWithFilePath( new FilePath( "ChaskisTests/bin/Debug/" + frameworkTarget + "/ChaskisTests.dll" ) ),
            unitTestDir.CombineWithFilePath( new FilePath( "CoreTests/bin/Debug/" + frameworkTarget + "/CoreTests.dll" ) ),
            unitTestDir.CombineWithFilePath( new FilePath( "PluginTests/bin/Debug/" + frameworkTarget + "/PluginTests.dll" ) )
        };
    }

    // ---------------- Functions ----------------

    public void RunUnitTests()
    {
        this.RunUnitTestInternal( this.cakeContext );
    }

    private void RunUnitTestInternal( ICakeContext context )
    {
        FilePath unitTestResultsOutput = this.paths.UnitTestResultFolder.CombineWithFilePath( new FilePath( "TestResult.xml" ) );
        NUnit3Result result = new NUnit3Result
        {
            FileName = unitTestResultsOutput
        };

        NUnit3Settings settings = new NUnit3Settings
        {
            Results = new List<NUnit3Result>{ result }
        };

        context.NUnit3( testAssemblies, settings );
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