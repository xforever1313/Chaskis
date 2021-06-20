//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Mono.Unix;

namespace Chaskis.ChaskisCliInstaller
{
    /// <summary>
    /// The class that puts the files where they are supposed to go.
    /// </summary>
    public class Installer
    {
        // ---------------- Fields ----------------

        private readonly Dictionary<string, DirectoryInfo> dirInfo;

        private const string parentDir = "INSTALLFOLDER";

        private readonly string wixArgName;

        private readonly string exeString;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rootDir">
        /// The install root.  The Chaskis directory will be placed in here.
        /// </param>
        /// <param name="wixFile">The WIX Xml file we'll use to parse and figure out where to install things.</param>
        public Installer( string slnDir, string rootDir, string wixFile, string target, string exeRunTime, string pluginRuntime )
        {
            this.wixArgName = nameof( wixFile );

            if( Directory.Exists( slnDir ) == false )
            {
                throw new DirectoryNotFoundException( "Directory " + slnDir + " does not exist." );
            }
            else if( File.Exists( Path.Combine( slnDir, "Chaskis.sln" ) ) == false )
            {
                throw new DirectoryNotFoundException( "Directory " + slnDir + " does not contain Chaskis.sln." );
            }

            if( Directory.Exists( rootDir ) == false )
            {
                throw new DirectoryNotFoundException( "Directory " + rootDir + " does not exist." );
            }

            if( File.Exists( wixFile ) == false )
            {
                throw new FileNotFoundException( "File " + wixFile + " does not exist." );
            }

            if( Environment.OSVersion.Platform == PlatformID.Win32NT )
            {
                this.exeString = ".exe";
            }
            else
            {
                this.exeString = string.Empty;
            }

            this.SlnDir = Path.GetFullPath( slnDir );
            this.RootDir = Path.GetFullPath( rootDir );
            this.WixFile = Path.GetFullPath( wixFile );

            if( ( target.ToLower() == "debug" ) )
            {
                this.Target = "Debug";
            }
            else if( target.ToLower() == "release" )
            {
                this.Target = "Release";
            }
            else
            {
                throw new ArgumentException( "Target must be debug or release.  Got " + target, nameof( target ) );
            }

            this.ExeRunTime = exeRunTime.ToLower();
            this.PluginRunTime = pluginRuntime.ToLower();

            this.dirInfo = new Dictionary<string, DirectoryInfo>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The install root.  The Chaskis directory will be placed in here.
        /// </summary>
        public string RootDir { get; private set; }

        /// <summary>
        /// The WIX Xml file we'll use to parse and figure out where to install things.
        /// </summary>
        public string WixFile { get; private set; }

        /// <summary>
        /// Our target (Debug or Release).
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// Our solution directory.
        /// </summary>
        public string SlnDir { get; private set; }

        public string ExeRunTime { get; private set; }

        public string PluginRunTime { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Starts the install.
        /// </summary>
        public void Start()
        {
            this.Status(
                string.Format(
                    "Installing Chaskis to {0} using XML file {1} and target {2}",
                    this.RootDir,
                    this.WixFile,
                    this.Target
                )
            );

            this.ParseWixFile();
        }

        /// <summary>
        /// Parses the WIX file so we know which files to grab and where to put them.
        /// </summary>
        private void ParseWixFile()
        {
            XmlDocument wixDoc = new XmlDocument();
            wixDoc.Load( this.WixFile );

            XmlNode rootNode = wixDoc.DocumentElement;
            if( rootNode.Name != "Wix" )
            {
                throw new ArgumentException( "Passed in Wix XML file does not have a 'Wix' root element", this.wixArgName );
            }

            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                if( childNode.Name == "Fragment" )
                {
                    foreach( XmlNode dirNode in childNode.ChildNodes )
                    {
                        if( dirNode.Name == "Directory" )
                        {
                            this.ParseDirectories( dirNode, null );
                        }
                    }
                }
            }

            IList<Tuple<string, string>> files = null;
            foreach( XmlNode childNode in rootNode.ChildNodes )
            {
                if( childNode.Name == "Fragment" )
                {
                    foreach( XmlAttribute attr in childNode.Attributes )
                    {
                        if( ( attr.Name == "Id" ) && ( attr.Value == "Components" ) )
                        {
                            files = this.ParseComponents( childNode );
                            break;
                        }
                    }
                }
            }

            if( files == null )
            {
                throw new ArgumentException( "No ComponentGroup fragments found...", this.wixArgName );
            }



            foreach( Tuple<string, string> file in files )
            {
                string filePath = Path.Combine( this.RootDir, file.Item2 );
                string dirPath = Path.GetDirectoryName( filePath );

                // Create any missing parent directories first.
                System.IO.DirectoryInfo info = new System.IO.DirectoryInfo( dirPath );
                System.IO.DirectoryInfo parentInfo = info.Parent;
                while( parentInfo != null )
                {
                    CreateDirectoryHelper( parentInfo.FullName );
                    parentInfo = parentInfo.Parent;
                }

                // Now create the new directory.
                CreateDirectoryHelper( dirPath );

                this.Status( "Copy " + file.Item1 + "\tto\t" + filePath );
                File.Copy( file.Item1, filePath, true );
            }
        }

        private void CreateDirectoryHelper( string dirPath )
        {
            if( Directory.Exists( dirPath ) == false )
            {
                this.Status( "Create directory: " + dirPath );
                Directory.CreateDirectory( dirPath );

                if(
                    ( Environment.OSVersion.Platform == PlatformID.Unix ) ||
                    ( Environment.OSVersion.Platform == PlatformID.MacOSX )
                )
                {
                    UnixFileInfo info = new UnixFileInfo( dirPath );
                    info.FileAccessPermissions =
                        FileAccessPermissions.UserReadWriteExecute |
                        FileAccessPermissions.GroupRead | FileAccessPermissions.GroupExecute |
                        FileAccessPermissions.OtherRead | FileAccessPermissions.OtherExecute;
                }
            }
        }

        private void ParseDirectories( XmlNode installFolder, string parent )
        {
            DirectoryInfo parentInfo = new DirectoryInfo();
            parentInfo.Id = installFolder.Attributes["Id"].Value;
            if( installFolder.Attributes["Name"] != null )
            {
                parentInfo.Name = installFolder.Attributes["Name"].Value;
            }
            else
            {
                parentInfo.Name = parentInfo.Id;
            }
            parentInfo.Parent = parent;
            this.dirInfo.Add( parentInfo.Id, parentInfo );

            foreach( XmlNode dirNode in installFolder.ChildNodes )
            {
                if( dirNode.Name == "Directory" )
                {
                    this.ParseDirectories( dirNode, parentInfo.Id );
                }
            }
        }

        private IList<Tuple<string, string>> ParseComponents( XmlNode fragment )
        {
            List<Tuple<string, string>> files = new List<Tuple<string, string>>();

            foreach( XmlNode componentGroup in fragment.ChildNodes )
            {
                if( componentGroup.Name == "ComponentGroup" )
                {
                    foreach( XmlNode component in componentGroup.ChildNodes )
                    {
                        if( component.Name == "Component" )
                        {
                            XmlAttribute dirAttr = component.Attributes["Directory"];
                            if( dirAttr == null )
                            {
                                throw new ArgumentException(
                                    "Wix file has a component but no directory attached on component " + component.Name,
                                    this.wixArgName
                                );
                            }
                            string directory = dirAttr.Value;
                            if( this.dirInfo.ContainsKey( directory ) == false )
                            {
                                throw new ArgumentException(
                                    "Wix file has a missing directory reference. " + directory + " not defined.",
                                    this.wixArgName
                                );
                            }

                            foreach( XmlNode file in component.ChildNodes )
                            {
                                if( file.Name == "File" )
                                {
                                    XmlAttribute source = file.Attributes["Source"];
                                    if( source == null )
                                    {
                                        throw new ArgumentException(
                                            "File is missing a source. File: " + file.Name,
                                            this.wixArgName
                                        );
                                    }

                                    bool optional = false;
                                    XmlAttribute optionalNode = file.Attributes["Optional"];
                                    if( optionalNode != null )
                                    {
                                        optional = bool.Parse( optionalNode.Value );
                                    }

                                    string sourcePath = source.Value;
                                    if( Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX )
                                    {
                                        sourcePath = sourcePath.Replace( "\\", "/" );
                                    }

                                    string originalDir = this.GetComponentPath( sourcePath );

                                    string relPath;
                                    TryParseVar( sourcePath, out relPath );

                                    if( ( File.Exists( relPath ) == false ) && optional )
                                    {
                                        Console.WriteLine( $"Can not file file '{relPath}', but its optional skipping" );
                                    }

                                    string fileName = Path.GetFileName( relPath );
                                    if( ( Environment.OSVersion.Platform == PlatformID.Unix ) || ( Environment.OSVersion.Platform == PlatformID.MacOSX ) )
                                    {
                                        // For some reason, the sqlite.native package is broken, and when it gets copied over,
                                        // it becomes '%(Filename)%(Extension)'.  lolwut.
                                        // Unix systems usually have sqlite already installed, we're just going to need to make it a
                                        // dependency.  Therefore, skip this file.
                                        if( fileName == "sqlite3.dll" )
                                        {
                                            continue;
                                        }
                                    }

                                    string dest =
                                        Path.Combine(
                                            this.GetPath( this.dirInfo[directory] ),
                                            fileName
                                        );

                                    Tuple<string, string> f = new Tuple<string, string>(
                                        originalDir,
                                        dest
                                    );
                                    files.Add( f );
                                }
                            }
                        }
                    }
                }
            }

            return files;
        }

        /// <summary>
        /// Gets the component's (the thing we want to copy)'s full path.
        /// </summary>
        /// <param name="source">From the XML File.</param>
        /// <returns>The ABS path of the thing we wish to copy.</returns>
        private string GetComponentPath( string source )
        {
            source = this.GetComponentAbsPath( source );

            return Path.Combine(
                Path.GetDirectoryName( this.WixFile ),
                source
            );
        }

        private bool TryParseVar( string source, out string fileName )
        {
            Match match = Regex.Match( source, @"\$\(var\.(?<plugin>\S+)\.(?<target>TargetPath|TargetDir)\)(?<file>\S+)?" );
            if( match.Success )
            {
                string pluginName = match.Groups["plugin"].Value;

                // At the moment, only plugins and Core use TargetPath, .exes used to, but Wix confused those with the .dlls,
                // not the .exes, so we swapped .exe to use targetDir instead.
                if( match.Groups["target"].Value == "TargetPath" )
                {
                    if( pluginName == "Chaskis.Core" )
                    {
                        fileName = Path.Combine( this.SlnDir, "ChaskisCore", "bin", this.Target, this.PluginRunTime, "Chaskis.Core.dll" );
                    }
                    else
                    {
                        fileName = Path.Combine( this.SlnDir, "Plugins", pluginName, "bin", this.Target, this.PluginRunTime, pluginName + ".dll" );
                    }
                }
                else
                {
                    string f = match.Groups["file"].Value;

                    if( pluginName == "Chaskis" )
                    {
                        fileName = Path.Combine( this.SlnDir, "Chaskis", "bin", this.Target, this.ExeRunTime, f );
                    }
                    else if( pluginName == "Chaskis.Service" )
                    {
                        fileName = Path.Combine( this.SlnDir, "Chaskis.Service", "bin", this.Target, this.ExeRunTime, f );
                    }
                    else
                    {
                        fileName = Path.Combine( this.SlnDir, "Plugins", pluginName, "bin", this.Target, this.PluginRunTime, f );
                    }
                }
            }
            else
            {
                fileName = source;
            }

            return match.Success;
        }

        /// <summary>
        /// Gets the path of the thing we want to copy.
        /// </summary>
        /// <param name="source">The path from the XML file.</param>
        private string GetComponentAbsPath( string source )
        {
            if( TryParseVar( source, out source ) == false )
            {
                source = Path.Combine(
                    // Paths are relative to the wix file.
                    Path.GetDirectoryName( this.WixFile ),
                    source
                );
            }

            return source;
        }

        /// <summary>
        /// Climbs the tree of directory info and gets the relative path.
        /// </summary>
        private string GetPath( DirectoryInfo info )
        {
            if( info.Id == parentDir )
            {
                return Path.Combine( info.Name );
            }

            List<string> parents = new List<string>();
            do
            {
                parents.Add( info.Name );
                info = this.dirInfo[info.Parent ?? parentDir];
            }
            while( info.Id != parentDir );
            parents.Add( info.Name );

            parents.Reverse();
            return Path.Combine( parents.ToArray() );
        }

        /// <summary>
        /// Prints status to console out.
        /// </summary>
        /// <param name="str"></param>
        private void Status( string str )
        {
            Console.WriteLine( str );
        }

        // ---------------- Helper Classes ----------------

        private class DirectoryInfo
        {
            public DirectoryInfo()
            {
            }

            // ---------------- Properties ----------------

            public string Name { get; set; }

            public string Id { get; set; }

            public string Parent { get; set; }
        }
    }
}
