//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using SethCS.Basic;

namespace Chaskis.Cli
{
    /// <summary>
    /// This class bootstraps a directory with default configurations.
    /// Useful when Chaskis is installed to a new user.
    /// </summary>
    public class BootStrapper
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// The main chaskis root.
        /// </summary>
        private readonly string chaskisRoot;

        /// <summary>
        /// The location where we are going to put the default configurations.
        /// </summary>
        private readonly string bootStrapLocation;

        /// <summary>
        /// The sample config directory.
        /// </summary>
        private readonly string sampleConfigDir;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="directory">The directory to move the default configurations to.</param>
        public BootStrapper( string directory )
        {
            string root = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, ".." );
            this.chaskisRoot = Path.GetFullPath( root );
            this.sampleConfigDir = Path.Combine( this.chaskisRoot, "SampleConfig" );

            this.bootStrapLocation = directory;
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Does the bootstrap.
        /// </summary>
        public void DoBootStrap()
        {
            StaticLogger.Log.WriteLine( "Using Chaskis root '{0}'", this.chaskisRoot );
            StaticLogger.Log.WriteLine();

            StaticLogger.Log.WriteLine( "Bootstrapping at location '{0}'", this.bootStrapLocation );
            StaticLogger.Log.WriteLine();

            StaticLogger.Log.WriteLine( "Sample Directory at '{0}'", this.sampleConfigDir );
            StaticLogger.Log.WriteLine();

            CreateDir( this.bootStrapLocation );

            StaticLogger.Log.WriteLine( "Copying main config..." );
            this.CopyAllSamples( this.sampleConfigDir );
            StaticLogger.Log.WriteLine( "Copying main config...Done!" );

            StaticLogger.Log.WriteLine( "Copying Plugin Config..." );
            string pluginDir = Path.Combine( this.sampleConfigDir, "Plugins" );
            string[] pluginDirs = Directory.GetDirectories( pluginDir );
            foreach( string plugin in pluginDirs )
            {
                this.CopyAllSamples( plugin );
            }
            StaticLogger.Log.WriteLine( "Copying Plugin Config...Done!" );

            StaticLogger.Log.WriteLine( "Bootstrapping completed!" );
        }

        private void CopyAllSamples( string path )
        {
            string[] files = Directory.GetFiles( path );
            foreach( string file in files )
            {
                string filePath = Path.Combine( path, file );

                // If our path is /usr/lib/Chaskis/SampleConfig/SampleConfig.xml and we
                // want to copy to /home/user/.config/Chaskis/Config.xml, we need to
                // strip everything before and including the SampleConfig/ from the file
                // path, and replace it with our destination directory.
                string relPath = filePath.Substring( this.sampleConfigDir.Length );
                relPath = relPath.TrimStart( Path.DirectorySeparatorChar , Path.AltDirectorySeparatorChar );

                string destinationPath = Path.Combine( this.bootStrapLocation, relPath );
                destinationPath = Path.GetDirectoryName( destinationPath );
                destinationPath = Path.GetFullPath( destinationPath );
                this.CopySampleFile( filePath, destinationPath );
            }
        }

        private void CopySampleFile( string filePath, string destinationDir )
        {
            string fileName = Path.GetFileName( filePath );

            // Only copy sample files, and strip the sample from their name.
            if( fileName.ToLower().StartsWith( "sample" ) )
            {
                this.CreateDir( destinationDir, false );

                string destinationFile = Path.Combine(
                    destinationDir,
                    fileName.Substring( 6 ) // Remove sample.
                );

                if( File.Exists( destinationFile ) )
                {
                    StaticLogger.Log.WriteLine( "'{0}' already exists, skipping.", destinationFile );
                }
                else
                {
                    StaticLogger.Log.WriteLine( "Copy '{0}' to '{1}'", filePath, destinationFile );
                    File.Copy( filePath, destinationFile );
                }
            }
        }

        private void CreateDir( string directory, bool printExistsMsg = true )
        {
            if( Directory.Exists( directory ) )
            {
                if( printExistsMsg )
                {
                    StaticLogger.Log.WriteLine( "Directory '{0}' exists, skipping creation.", directory );
                }
            }
            else
            {
                StaticLogger.Log.WriteLine( "Creating directory '{0}'...", directory );
                Directory.CreateDirectory( directory );
            }
        }
    }
}
