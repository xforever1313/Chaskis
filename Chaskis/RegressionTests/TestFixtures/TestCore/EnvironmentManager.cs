//
//          Copyright Seth Hendrick 2017-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using SethCS.Basic;

namespace Chaskis.RegressionTests.TestCore
{
    /// <summary>
    /// This manages Chaskis Test Environments.
    /// 
    /// Each time we run a test, we want to make sure our Environment is prestine.
    /// So we do our work in a Test Environment and delete it on teardown.
    /// </summary>
    public class EnvironmentManager : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly string environmentDir;

        private const string defaultEnvironmentName = "DefaultEnvironment";

        private ushort port;

        private readonly GenericLogger consoleOut;

        // ---------------- Constructor ----------------

        public EnvironmentManager( string testDllFolder )
        {
            // Our working directory is in the bin/Debug/netcoreapp folder... need to account for that one...
            this.ChaskisProjectRoot = Path.GetFullPath(
                Path.Combine(
                    testDllFolder, // netcoreapp.
                    "..", // Debug
                    "..", // bin
                    "..", // TestFixtures
                    "..", // RegressionTests
                    ".."  // Chaskis Root
                )
            );

            this.RegressionTestDir = Path.Combine( this.ChaskisProjectRoot, "RegressionTests" );

            this.ChaskisDistDir = Path.Combine( this.RegressionTestDir, "dist", "Chaskis" );

            this.environmentDir = Path.Combine( this.RegressionTestDir, "Environments" );
            this.TestEnvironmentDir = Path.Combine( this.environmentDir, "TestEnvironment" );

            this.consoleOut = Logger.GetConsoleOutLog();

            this.consoleOut.WriteLine( "Working Directory: " + Directory.GetCurrentDirectory() );
            this.consoleOut.WriteLine( "Chaskis Root: " + this.ChaskisProjectRoot );
            this.consoleOut.WriteLine( "Regression Test Directory: " + this.RegressionTestDir );
            this.consoleOut.WriteLine( "System Under Test Directory: " + this.ChaskisDistDir );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Path to the folder that contains the SLN file.
        /// </summary>
        public string ChaskisProjectRoot { get; private set; }

        /// <summary>
        /// Path to the folder that contains regression tests.
        /// </summary>
        public string RegressionTestDir { get; private set; }

        /// <summary>
        /// Path to the folder that contains the test environments.
        /// </summary>
        public string TestEnvironmentDir { get; private set; }

        /// <summary>
        /// The system under test's directory
        /// </summary>
        public string ChaskisDistDir { get; private set; }

        // ---------------- Functions ----------------

        public void SetupDefaultEnvironment( ushort port )
        {
            this.SetupEnvironment( defaultEnvironmentName, port );
        }

        /// <summary>
        /// Sets up the given Environment to use by moving it
        /// to the TestEnvironment Directory.
        /// </summary>
        public void SetupEnvironment( string envName, ushort port )
        {
            this.port = port;
            this.TeardownEnvironment();

            string envPath = Path.Combine( environmentDir, envName );
            if( Directory.Exists( envPath ) == false )
            {
                throw new DirectoryNotFoundException( envPath + " does not exist!" );
            }

            this.consoleOut.WriteLine( "Copying Environment " + envPath + " to " + TestEnvironmentDir + "..." );
            DirectoryCopy( envPath, TestEnvironmentDir, true );
            this.consoleOut.WriteLine( "Copying Environment " + envPath + " to " + TestEnvironmentDir + "...Done!" );
        }

        /// <summary>
        /// Removes the test Environment, if it exists.
        /// </summary>
        public void TeardownEnvironment()
        {
            if( Directory.Exists( TestEnvironmentDir ) )
            {
                this.consoleOut.WriteLine( "Deleting Test Environment " + TestEnvironmentDir + "..." );
                Directory.Delete( TestEnvironmentDir, true );
                this.consoleOut.WriteLine( "Deleting Test Environment " + TestEnvironmentDir + "...Done!" );
            }
        }

        public void Dispose()
        {
            this.TeardownEnvironment();
        }

        /// <summary>
        /// Copies the entire directory.
        /// 
        /// Taken from: https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        /// </summary>
        private void DirectoryCopy( string sourceDirName, string destDirName, bool copySubDirs )
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo( sourceDirName );

            if( !dir.Exists )
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName );
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if( Directory.Exists( destDirName ) == false )
            {
                Directory.CreateDirectory( destDirName );
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach( FileInfo file in files )
            {
                string temppath = Path.Combine( destDirName, file.Name );
                if( file.Extension.Equals( ".xml", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    string fileContents = File.ReadAllText( Path.Combine( file.DirectoryName, file.FullName ) );
                    fileContents = fileContents.Replace( "{%chaskispath%}", ChaskisDistDir );
                    fileContents = fileContents.Replace( "{%regressiontestpath%}", RegressionTestDir );
                    fileContents = fileContents.Replace( "{%port%}", this.port.ToString() );
                    fileContents = fileContents.Replace( "{%testenvpath%}", TestEnvironmentDir );
                    File.WriteAllText( temppath, fileContents );
                }
                else
                {
                    file.CopyTo( temppath, true );
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if( copySubDirs )
            {
                foreach( DirectoryInfo subdir in dirs )
                {
                    string temppath = Path.Combine( destDirName, subdir.Name );
                    DirectoryCopy( subdir.FullName, temppath, copySubDirs );
                }
            }
        }
    }
}
