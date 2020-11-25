//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Linq;
using Chaskis.Plugins.IrcLogger;
using NUnit.Framework;
using SethCS.Basic;

namespace Chaskis.UnitTests.PluginTests.Plugins.IrcLogger
{
    [TestFixture]
    public class LogManagerTest
    {
        // -------- Fields --------

        private GenericLogger log;

        /// <summary>
        /// Unit Under Test.
        /// </summary>
        private LogManager uut;

        /// <summary>
        /// The config to use during this test.
        /// </summary>
        private IrcLoggerConfig testConfig;

        private static readonly string testLogDirectory = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestLogs"
        );

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.log = new GenericLogger();
            this.log.OnErrorWriteLine += delegate ( string s ) { Console.Error.WriteLine( s ); };

            this.testConfig = new IrcLoggerConfig();
            this.testConfig.LogName = "TestLog";
            this.testConfig.LogFileLocation = testLogDirectory;
        }

        [TearDown]
        public void TestTeardown()
        {
            this.uut?.Dispose();

            if( Directory.Exists( testLogDirectory ) )
            {
                string[] files = Directory.GetFiles( testLogDirectory );
                foreach( string file in files )
                {
                    File.Delete( file );
                }
                Directory.Delete( testLogDirectory );
            }
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures a NullArgumentException is thrown if we give a null argument during construction.
        /// </summary>
        [Test]
        public void NullArgumentTest()
        {
            Assert.Throws<ArgumentNullException>( () => new LogManager( null, log ) );
        }

        /// <summary>
        /// Ensures setting the file count to 0 results in no file changes.
        /// </summary>
        [Test]
        public void SingleFileTest()
        {
            this.testConfig.MaxNumberMessagesPerLog = 0;
            this.uut = new LogManager( this.testConfig, log );

            // Ensure no file is currently open.
            Assert.AreEqual( string.Empty, this.uut.CurrentFileName );
            Assert.AreEqual( string.Empty, this.uut.LastFileName );

            this.uut.SyncLogToFile( "Hello World!" );

            // Ensure directory is created.
            Assert.IsTrue( Directory.Exists( testLogDirectory ) );

            // Ensure there is one file created, and named correctly.
            string[] files = Directory.GetFiles( testLogDirectory );
            Assert.AreEqual( 1, files.Length );
            Assert.IsTrue( files[0].Contains( this.testConfig.LogName ) );
            Assert.AreEqual( this.uut.CurrentFileName, Path.GetFileName( files[0] ) );

            // Log 2000 more times.  There should still be only 1 file at the end.
            for( int i = 0; i < 2000; ++i )
            {
                this.uut.SyncLogToFile( "Test " + i );
            }

            // Ensure there is one file created, and named correctly.
            files = Directory.GetFiles( testLogDirectory );
            Assert.AreEqual( 1, files.Length );
            Assert.IsTrue( files[0].Contains( this.testConfig.LogName ) );
            Assert.AreEqual( this.uut.CurrentFileName, Path.GetFileName( files[0] ) );
            Assert.IsEmpty( this.uut.LastFileName ); // No new file name, should still be empty.
        }

        /// <summary>
        /// Ensures that once we reach the count, a new file is created.
        /// </summary>
        [Test]
        public void MultipleFileTest()
        {
            const int maxMessages = 10;
            this.testConfig.MaxNumberMessagesPerLog = maxMessages;
            this.uut = new LogManager( this.testConfig, this.log );

            // Ensure no file is currently open.
            Assert.AreEqual( string.Empty, this.uut.CurrentFileName );
            Assert.AreEqual( string.Empty, this.uut.LastFileName );

            this.uut.SyncLogToFile( "Hello World!" );

            // Ensure directory is created.
            Assert.IsTrue( Directory.Exists( testLogDirectory ) );

            // Ensure there is one file created, and named correctly.
            string[] files = Directory.GetFiles( testLogDirectory );
            Assert.AreEqual( 1, files.Length );
            Assert.IsTrue( files[0].Contains( this.testConfig.LogName ) );
            Assert.AreEqual( this.uut.CurrentFileName, Path.GetFileName( files[0] ) );

            string currentFileName = uut.CurrentFileName;

            // Log more times more times.  There should still be only 1 file at the end.
            for( int i = 0; i < maxMessages; ++i )
            {
                this.uut.SyncLogToFile( "Test " + i );
            }

            // Ensure there a new file created, and named correctly.
            files = Directory.GetFiles( testLogDirectory );
            Assert.AreEqual( 2, files.Length );
            Assert.IsTrue( files[0].Contains( this.testConfig.LogName ) );
            Assert.IsTrue( files[1].Contains( this.testConfig.LogName ) );
            Assert.IsNotNull( files.First( f => Path.GetFileName( f ) == this.uut.CurrentFileName ) );
            Assert.IsNotNull( files.First( f => Path.GetFileName( f ) == this.uut.LastFileName ) );

            // Current file name should become last.
            Assert.AreEqual( currentFileName, this.uut.LastFileName );

            // Do it again, should have three files.
            currentFileName = uut.CurrentFileName;

            // Log more times more times.  There should still be only 1 file at the end.
            for( int i = 0; i < maxMessages; ++i )
            {
                this.uut.SyncLogToFile( "Test " + i );
            }

            // Ensure there a new file created, and named correctly.
            files = Directory.GetFiles( testLogDirectory );
            Assert.AreEqual( 3, files.Length );
            Assert.IsTrue( files[0].Contains( this.testConfig.LogName ) );
            Assert.IsTrue( files[1].Contains( this.testConfig.LogName ) );
            Assert.IsTrue( files[2].Contains( this.testConfig.LogName ) );
            Assert.IsNotNull( files.First( f => Path.GetFileName( f ) == this.uut.CurrentFileName ) );
            Assert.IsNotNull( files.First( f => Path.GetFileName( f ) == this.uut.LastFileName ) );

            // Current file name should become last.
            Assert.AreEqual( currentFileName, this.uut.LastFileName );
        }
    }
}