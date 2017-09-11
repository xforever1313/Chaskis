//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using NetRunner.ExternalLibrary;
using SethCS.Basic;

namespace Chaskis.RegressionTests
{
    /// <summary>
    /// This manages Chaskis Test Environments.
    /// 
    /// Each time we run a test, we want to make sure our Environment is prestine.
    /// So we do our work in a Test Environment and delete it on teardown.
    /// </summary>
    public class EnvironmentManager : BaseTestContainer
    {
        // ---------------- Fields ----------------

        public static readonly string ChaskisRoot;

        public static readonly string RegressionTestDir;

        private string environmentDir;

        private string testEnvironmentDir;

        private const string defaultEnvironmentName = "DefaultEnvironment";

        private string defaultEnvironmentDir;

        private GenericLogger consoleOut;

        // ---------------- Constructor ----------------

        public EnvironmentManager()
        {
            this.environmentDir = Path.Combine( RegressionTestDir, "Environments" );
            this.testEnvironmentDir = Path.Combine( this.environmentDir, "TestEnvironment" );
            this.defaultEnvironmentDir = Path.Combine( this.environmentDir, defaultEnvironmentName );

            this.consoleOut = Logger.GetConsoleOutLog();

            this.consoleOut.WriteLine( "Working Directory: " + Directory.GetCurrentDirectory() );
            this.consoleOut.WriteLine( "Chaskis Root: " + ChaskisRoot );
            this.consoleOut.WriteLine( "Regression Test Directory: " + RegressionTestDir );
        }

        static EnvironmentManager()
        {
            // Our working directory is in the bin/Debug folder... need to account for that one...
            ChaskisRoot = Path.GetFullPath(
                Path.Combine(
                    "..", // bin
                    "..", // TestFixtures
                    "..", // RegressionTests
                    ".."  // Chaskis Root
                )
            );

            RegressionTestDir = Path.Combine( ChaskisRoot, "RegressionTests" );
        }

        // ---------------- Functions ----------------

        public bool SetupDefaultEnvironment()
        {
            return this.SetupEnvironment( defaultEnvironmentName );
        }

        /// <summary>
        /// Sets up the given Environment to use by moving it
        /// to the TestEnvironment Directory.
        /// </summary>
        public bool SetupEnvironment( string envName )
        {
            this.TeardownEnvironment();

            string envPath = Path.Combine( this.environmentDir, envName );
            if( Directory.Exists( envPath ) == false )
            {
                throw new DirectoryNotFoundException( envPath + " does not exist!" );
            }

            this.consoleOut.WriteLine( "Copying Environment " + envPath + " to " + this.testEnvironmentDir + "..." );
            DirectoryCopy( envPath, this.testEnvironmentDir, true );
            this.consoleOut.WriteLine( "Copying Environment " + envPath + " to " + this.testEnvironmentDir + "...Done!" );

            return true;
        }

        /// <summary>
        /// Removes the test Environment, if it exists.
        /// </summary>
        public bool TeardownEnvironment()
        {
            if( Directory.Exists( this.testEnvironmentDir ) )
            {
                this.consoleOut.WriteLine( "Deleting Test Environment " + this.testEnvironmentDir + "..." );
                Directory.Delete( this.testEnvironmentDir, true );
                this.consoleOut.WriteLine( "Deleting Test Environment " + this.testEnvironmentDir + "...Done!" );
            }

            return true;
        }

        /// <summary>
        /// Copies the entire directory.
        /// 
        /// Taken from: https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        /// </summary>
        private static void DirectoryCopy( string sourceDirName, string destDirName, bool copySubDirs )
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
            if( !Directory.Exists( destDirName ) )
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
                    fileContents = fileContents.Replace( "{%chaskispath%}", ChaskisRoot );
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
