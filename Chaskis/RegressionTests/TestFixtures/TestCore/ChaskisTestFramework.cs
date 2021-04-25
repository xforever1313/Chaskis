//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using NUnit.Framework;
using SethCS.Basic;

namespace Chaskis.RegressionTests.TestCore
{
    public class ChaskisTestFramework
    {
        // ---------------- Fields ----------------

        private readonly GenericLogger testStateLog;

        private FileStream logFile;
        private StreamWriter logWriter;

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

        // -------- Setup / Teardown --------

        public void PerformFixtureSetup( ChaskisFixtureConfig config = null )
        {
            Step.Run(
                this.testStateLog,
                "Running Fixture Setup",
                () => PerformFixtureSetupInternal( config )
            );
        }

        private void PerformFixtureSetupInternal( ChaskisFixtureConfig config )
        {
            if( config == null )
            {
                config = new ChaskisFixtureConfig();
            }

            if( this.logFile == null )
            {
                SetupLog();
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

            // If we don't expect a connection, no sense in starting the server.
            if( config.ConnectionWaitMode != ConnectionWaitMode.ExpectNoConnection )
            {
                Step.Run(
                    "Starting server",
                    () => this.IrcServer.StartServer( config.Port )
                );
            }

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

            if( config.ConnectionWaitMode != ConnectionWaitMode.ExpectNoConnection )
            {
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
            else
            {
                Console.WriteLine( "Expecting no connecting, skipping wait" );
            }
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
                Logger.OnWriteLine -= this.Logger_OnWriteLine;
                this.logWriter?.Dispose();
                this.logFile?.Dispose();
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

        private void PerformTestTeardownInternal()
        {
        }

        // -------- Helper Functions --------

        public void SetupLog()
        {
            if( this.logFile != null )
            {
                throw new InvalidOperationException(
                    "Logfile already created"
                );
            }

            string logPath = Path.Combine(
                this.EnvironmentManager.ChaskisProjectRoot,
                "..",
                "TestResults",
                "RegressionTests",
                "Logs"
            );

            if( Directory.Exists( logPath ) == false )
            {
                Directory.CreateDirectory( logPath );
            }

            string fileName = Path.Combine(
                logPath,
                TestContext.CurrentContext.Test.Name + ".log"
            );

            this.logFile = new FileStream( fileName, FileMode.Create, FileAccess.ReadWrite );
            this.logWriter = new StreamWriter( this.logFile );

            Logger.OnWriteLine += this.Logger_OnWriteLine;
        }

        private void Logger_OnWriteLine( string obj )
        {
            this.logWriter.Write( obj );
            this.logWriter.Flush();
        }
    }
}
