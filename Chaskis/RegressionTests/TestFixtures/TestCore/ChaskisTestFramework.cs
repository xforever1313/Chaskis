//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using NUnit.Framework;

namespace Chaskis.RegressionTests.TestCore
{
    public class ChaskisTestFramework
    {
        // ---------------- Fields ----------------

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
        }

        // ---------------- Properties ----------------

        public ChaskisProcess ProcessRunner { get; private set; }

        public HttpServer HttpServer { get; private set; }

        public TestHttpClient HttpClient { get; private set; }

        public IrcServer IrcServer { get; private set; }

        // ---------------- Functions ----------------

        public void PerformFixtureSetup( ChaskisFixtureConfig config = null )
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

            // ---------------- Start Process ----------------

            this.ProcessRunner = new ChaskisProcess( this.envManager.ChaskisDistDir, this.envManager.TestEnvironmentDir );
        }

        public void PerformFixtureTeardown()
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
        }

        public void PerformTestTeardown()
        {
        }
    }
}
