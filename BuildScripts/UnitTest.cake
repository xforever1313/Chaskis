public class UnitTestRunner : TestRunner
{
    // ---------------- Constructor ----------------

    public UnitTestRunner( ICakeContext context, ImportantPaths paths ) :
        base(
            context,
            paths,
            "UnitTests",
            paths.UnitTestFolder.CombineWithFilePath( new FilePath( "Chaskis.UnitTests.csproj" ) )
        )
    {
    }
}
