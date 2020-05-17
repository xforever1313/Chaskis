﻿//
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

        // ---------------- Constructor ----------------

        public ChaskisTestFramework() :
            this( TestContext.CurrentContext.TestDirectory )
        {
        }

        public ChaskisTestFramework( string dllFolderPath )
        {
            this.EnvironmentManager = new EnvironmentManager( dllFolderPath );

            this.HttpServer = new HttpServer();
            this.HttpClient = new TestHttpClient();
            this.IrcServer = new IrcServer();
            this.testStateLog = Logger.GetLogFromContext( "TestState" );
        }

        // ---------------- Properties ----------------

        public EnvironmentManager EnvironmentManager { get; private set; }

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
                this.EnvironmentManager.SetupDefaultEnvironment( config.Port );
            }
            else
            {
                this.EnvironmentManager.SetupEnvironment( config.Environment, config.Port );
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
                    this.ProcessRunner = new ChaskisProcess( this.EnvironmentManager.ChaskisDistDir, this.EnvironmentManager.TestEnvironmentDir );
                    this.ProcessRunner.StartProcess();
                }
            );

            // ---------------- Wait to join ----------------

            Step.Run(
                "Waiting for client to connect to server",
                () => this.IrcServer.WaitForConnection().FailIfFalse( "Server never got IRC connection" )
            );

            Step.Run(
                "Wait for client to finish connecting.",
                () =>
                {
                    if( config.ConnectionWaitMode == ConnectionWaitMode.WaitForConnected )
                    {
                        this.ProcessRunner.WaitForClientToConnect();
                    }
                    else if( config.ConnectionWaitMode == ConnectionWaitMode.WaitForFinishJoiningChannels )
                    {
                        this.ProcessRunner.WaitToFinishJoiningChannels();
                    }

                    // Else, do nothing.
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
                this.ProcessRunner?.StopOrKillProcess();
                this.ProcessRunner?.Dispose();
                this.HttpServer?.Dispose();
                this.IrcServer?.Dispose();
            }
            finally
            {
                this.EnvironmentManager.Dispose();
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