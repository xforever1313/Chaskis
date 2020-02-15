//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using NUnit.Framework;
using SethCS.Basic;

namespace Chaskis.RegressionTests.TestCore
{
    public class ChaskisTestFramework
    {
        // ---------------- Fields ----------------

        private readonly GenericLogger testStateLog;

        private readonly EnvironmentManager envManager;

        // ---------------- Constructor ----------------

        public ChaskisTestFramework() :
            this( TestContext.CurrentContext.TestDirectory )
        {
        }

        public ChaskisTestFramework( string dllFolderPath )
        {
            this.envManager = new EnvironmentManager( dllFolderPath );

            this.HttpServer = new HttpServer();
            this.HttpClient = new TestHttpClient();
            this.IrcServer = new IrcServer();
            this.testStateLog = Logger.GetLogFromContext( "TestState" );
        }

        // ---------------- Properties ----------------

        public ChaskisProcess ProcessRunner { get; private set; }

        public HttpServer HttpServer { get; private set; }

        public TestHttpClient HttpClient { get; private set; }

        public IrcServer IrcServer { get; private set; }

        // ---------------- Functions ----------------

        public void PerformFixtureSetup( ChaskisFixtureConfig config = null )
        {
            Step.Run(
                this.testStateLog,
                "Running Fixture Setup",
                () => PerformFixtureSetupInternal( config )
            );
        }

        private void PerformFixtureSetupInternal( ChaskisFixtureConfig config  )
        {
            if( config == null )
            {
                config = new ChaskisFixtureConfig();
            }

            // ---------------- Setup Environment ----------------

            if( config.Environment == null )
            {
                this.envManager.SetupDefaultEnvironment( config.Port );
            }
            else
            {
                this.envManager.SetupEnvironment( config.Environment, config.Port );
            }

            // ---------------- Start Server ----------------

            Step.Run(
                "Starting server",
                () => this.IrcServer.StartServer( config.Port )
            );

            // ---------------- Start Process ----------------

            Step.Run(
                "Starting Client",
                () =>
                {
                    this.ProcessRunner = new ChaskisProcess( this.envManager.ChaskisDistDir, this.envManager.TestEnvironmentDir );
                    this.ProcessRunner.StartProcess();
                }
            );

            // ---------------- Wait to join ----------------

            Step.Run(
                "Waiting for client to connect to server",
                () => this.IrcServer.WaitForConnection().FailIfFalse( "Server never got IRC connection" )
            );

            Step.Run(
                "Wait for client to join channels",
                () =>
                {
                    this.ProcessRunner.WaitForStringFromChaskis(
                        @"<chaskis_event source_type=""CORE""\s+source_plugin=""IRC""\s+dest_plugin=""""><args><event_id>FINISHED\s+JOINING\s+CHANNELS</event_id><server>(?<server>\S+)</server><nick>(?<nick>\S+)</nick></args><passthrough_args\s+/></chaskis_event>"
                    );
                }
            );
        }

        public void PerformFixtureTeardown()
        {
            Step.Run(
                this.testStateLog,
                "Running Fixture Teardown",
                () => PerformFixtureTeardowInternal()
            );
        }
        private void PerformFixtureTeardowInternal()
        {
            try
            {
                this.ProcessRunner?.Dispose();
                this.HttpServer?.Dispose();
                this.IrcServer?.Dispose();
            }
            finally
            {
                this.envManager.Dispose();
            }
        }

        public void PerformTestSetup()
        {
            Step.Run(
                this.testStateLog,
                "Running Test Setup",
                () => PerformTestSetupInternal()
            );
        }

        private void PerformTestSetupInternal()
        {
        }

        public void PerformTestTeardown()
        {
            Step.Run(
                this.testStateLog,
                "Running Test Teardown",
                () => PerformTestTeardownInternal()
            );
        }

        public void PerformTestTeardownInternal()
        {
        }
    }
}
