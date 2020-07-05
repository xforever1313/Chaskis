public class RegressionTestRunner : TestRunner
{
    // ---------------- Constructor ----------------

    public RegressionTestRunner( ICakeContext context, ImportantPaths paths ) :
        base(
            context,
            paths,
            "RegressionTests",
            paths.RegressionTestProjectFolder.CombineWithFilePath( new FilePath( "RegressionTests.csproj" ) )
        )
    {
    }
}