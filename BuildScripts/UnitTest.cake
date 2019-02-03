#tool "nuget:?package=NUnit.ConsoleRunner&version=3.9.0"
#tool "nuget:?package=OpenCover&version=4.6.519"
#tool "nuget:?package=ReportGenerator&version=4.0.10"

public class UnitTestRunner
{
    // ---------------- Fields ----------------

    private readonly ICakeContext cakeContext;
    private readonly DirectoryPath projectRoot;
    private readonly DirectoryPath unitTestDir;

    private readonly List<FilePath> testAssemblies;

    // ---------------- Constructor ----------------

    public UnitTestRunner( ICakeContext context, DirectoryPath projectRoot )
    {
        this.cakeContext = context;
        this.projectRoot = projectRoot;
        this.unitTestDir = this.projectRoot.Combine( new DirectoryPath( "Chaskis/UnitTests" ) );

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
        FilePath unitTestResultsOutput = projectRoot.CombineWithFilePath( new FilePath( "TestResult/TestResult.xml" ) );
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
        DirectoryPath codeCoveragePath = projectRoot.Combine( new DirectoryPath( "CodeCoverage" ) );
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