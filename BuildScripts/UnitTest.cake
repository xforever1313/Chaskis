#tool "nuget:?package=NUnit.ConsoleRunner&version=3.9.0"

List<FilePath> testAssemblies = new List<FilePath>
{
    unitTestDir.CombineWithFilePath( new FilePath( "ChaskisTests/bin/Debug/" + frameworkTarget + "/ChaskisTests.dll" ) ),
    unitTestDir.CombineWithFilePath( new FilePath( "CoreTests/bin/Debug/" + frameworkTarget + "/CoreTests.dll" ) ),
    unitTestDir.CombineWithFilePath( new FilePath( "PluginTests/bin/Debug/" + frameworkTarget + "/PluginTests.dll" ) )
};

FilePath unitTestResultsOutput = projectRoot.CombineWithFilePath( new FilePath( "TestResult/TestResult.xml" ) );

void RunUnitTests()
{
    NUnit3Result result = new NUnit3Result
    {
        FileName = unitTestResultsOutput
    };

    NUnit3Settings settings = new NUnit3Settings
    {
        Results = new List<NUnit3Result>{ result }
    };

    NUnit3( testAssemblies, settings );
}

void RunCodeCoverage()
{

}