//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.ArgumentBinder;
using Cake.Core.IO;
using Cake.Frosting;
using Seth.CakeLib.TestRunner;

namespace DevOps.Tasks
{
    [TaskName( "unit_test" )]
    [TaskDescription( "Runs all the unit tests.  Pass in --code_coverage=true to run with coverage." )]
    public class RunUnitTestTask : DefaultTask
    {
        // ----------------- Functions -----------------

        public override void Run( ChaskisContext context )
        {
            TestConfig testConfig = new TestConfig
            {
                ResultsFolder = context.Paths.TestResultFolder,
                TestCsProject = context.Paths.UnitTestFolder.CombineWithFilePath( new FilePath( "Chaskis.UnitTests.csproj" ) )
            };
            UnitTestRunner runner = new UnitTestRunner( context, testConfig );

            TestArguments args = context.CreateFromArguments<TestArguments>();
            if( args.RunWithCodeCoverage )
            {
                runner.RunCodeCoverage( TestArguments.CoverageFilter );
            }
            else
            {
                runner.RunTests();
            }
        }
    }
}
