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
    [TaskName( "bootstrap_regression_tests" )]
    [TaskDescription( "Creates the distro for the regression tests." )]
    public class BootstrapRegressionTest : DefaultTask
    {
        // ---------------- Functions ----------------

        public override void Run( ChaskisContext context )
        {
            DistroCreatorConfig distroConfig = new DistroCreatorConfig
            {
                OutputLocation = context.Paths.RegressionDistroFolder.ToString()
            };

            DistroCreator distroCreator = new DistroCreator(
                context,
                distroConfig
            );
            distroCreator.CreateDistro();
        }
    }
    
    [TaskName( "regression_test" )]
    [TaskDescription( "Runs all regression tests." )]
    public class RunRegressionTestTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override void Run( ChaskisContext context )
        {
            TestConfig testConfig = new TestConfig
            {
                ResultsFolder = context.Paths.TestResultFolder,
                TestCsProject = context.Paths.RegressionTestProjectFolder.CombineWithFilePath( new FilePath( "RegressionTests.csproj" ) )
            };

            BaseTestRunner runner = new BaseTestRunner(
                context,
                testConfig,
                "RegressionTests"
            );

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
