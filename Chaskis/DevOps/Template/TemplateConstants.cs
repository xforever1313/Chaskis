//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;
using Cake.FileHelpers;

namespace DevOps.Template
{
    /// <summary>
    /// This class contains contants that are used
    /// to Template files.
    /// </summary>
    public class TemplateConstants
    {
        // ---------------- Fields ----------------

        private readonly string chaskisCoreProjectContents;

        // ---------------- Constructor ----------------

        public TemplateConstants( ChaskisContext context )
        {
            this.CakeContext = context;
            this.ImportantPaths = context.Paths;

            this.chaskisCoreProjectContents = this.CakeContext.FileReadText( this.ImportantPaths.ChaskisCoreVersionFile );

            this.FullName = "Chaskis IRC Bot";
            this.ChaskisVersion = this.GetChaskisVersion();
            this.ChaskisCoreVersion = this.GetChaskisCoreVersion();
            this.License = this.GetLicense();
            this.RegressionTestPluginVersion = this.GetRegressionTestPluginVersion();
            this.RegressionTestPluginName = this.GetRegressionTestPluginName();
            this.DefaultPluginName = this.GetDefaultPluginName();

            string tags = this.GetTags();
            this.CoreTags = tags + " core";
            this.CliTags = tags + " service full installer admin";
            this.Author = this.GetAuthor();
            this.AuthorEmail = "seth@shendrick.net";
            this.ProjectUrl = this.GetProjectUrl();
            this.LicenseUrl = this.GetLicenseUrl();
            this.WikiUrl = "https://github.com/xforever1313/Chaskis/wiki";
            this.IssueTrackerUrl = "https://github.com/xforever1313/Chaskis/issues";
            this.CopyRight = this.GetCopyRight();
            this.Description = this.GetDescription();
            this.ReleaseNotes = this.GetReleaseNotes();
            this.Summary = "A generic framework written in C# for making IRC Bots.";
            this.IconUrl = this.GetIconUrl();
            this.RunTime = context.ApplicationTarget;
            this.DebChecksum = this.GetDebChecksum();
            this.MsiChecksum = this.GetMsiChecksum();
        }

        // ---------------- Properties ----------------

        public ChaskisContext CakeContext { get; private set; }

        public ImportantPaths ImportantPaths { get; private set; }

        public string FullName { get; private set; }

        public string ChaskisVersion { get; private set; }

        public string ChaskisCoreVersion { get; private set; }

        public string License { get; private set; }

        public string RegressionTestPluginVersion { get; private set; }

        public string RegressionTestPluginName { get; private set; }

        public string DefaultPluginName { get; private set; }

        public string CoreTags { get; private set; }

        public string CliTags { get; private set; }

        public string Author { get; private set; }

        public string AuthorEmail { get; private set; }

        public string ProjectUrl { get; private set; }

        public string LicenseUrl { get; private set; }

        public string WikiUrl { get; private set; }

        public string IssueTrackerUrl { get; private set; }

        public string CopyRight { get; private set; }

        public string Description { get; private set; }

        public string ReleaseNotes { get; private set; }

        public string Summary { get; private set; }

        public string IconUrl { get; private set; }

        public string RunTime { get; private set; }

        public string DebChecksum { get; private set; }

        public string MsiChecksum { get; private set; }

        // ---------------- Functions ----------------

        private string GetChaskisVersion()
        {
            string fileContents = this.CakeContext.FileReadText( this.ImportantPaths.ChaskisVersionFile );
            Match match = Regex.Match(
                fileContents,
                @"public\s+const\s+string\s+VersionStr\s+=\s+""(?<version>\d+\.\d+\.\d+)"";"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Chaskis Version Regex" );
            }

            return match.Groups["version"].Value;
        }

        private string GetChaskisCoreVersion()
        {
            string fileContents = this.chaskisCoreProjectContents;
            Match match = Regex.Match(
                fileContents,
                @"\<Version\>(?<version>\d+\.\d+\.\d+)\</Version\>"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Chaskis Core Version Regex" );
            }

            return match.Groups["version"].Value;
        }

        private string GetLicense()
        {
            return this.CakeContext.FileReadText( this.ImportantPaths.LicenseFile );
        }

        private string GetRegressionTestPluginVersion()
        {
            string fileContents = this.CakeContext.FileReadText( this.ImportantPaths.RegressionTestPluginFile );
            Match match = Regex.Match(
                fileContents,
                @"public\s+const\s+string\s+VersionStr\s+=\s+""(?<version>\d+\.\d+\.\d+)"";"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Regression Test Version Regex" );
            }

            return match.Groups["version"].Value;
        }

        private string GetRegressionTestPluginName()
        {
            string fileContents = this.CakeContext.FileReadText( this.ImportantPaths.RegressionTestPluginFile );
            Match match = Regex.Match(
                fileContents,
                @"\[ChaskisPlugin\( ""(?<name>\w+)"" \)\]"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Chaskis Plugin Name Regex" );
            }

            return match.Groups["name"].Value;
        }

        private string GetDefaultPluginName()
        {
            string fileContents = this.CakeContext.FileReadText( this.ImportantPaths.DefaultPluginFile );
            Match match = Regex.Match(
                fileContents,
                @"internal\s+const\s+string\s+DefaultPluginName\s*=\s*""(?<name>\w+)"""
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Default Plugin Name Regex" );
            }

            return match.Groups["name"].Value;
        }

        private string GetTags()
        {
            string fileContents = this.chaskisCoreProjectContents;
            Match match = Regex.Match(
                fileContents,
                @"\<PackageTags\>(?<tags>.+)\</PackageTags\>"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Package Tags Regex" );
            }

            return match.Groups["tags"].Value;
        }

        private string GetAuthor()
        {
            string fileContents = this.chaskisCoreProjectContents;
            Match match = Regex.Match(
                fileContents,
                @"\<Authors\>(?<author>.+)\</Authors\>"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Author Regex" );
            }

            return match.Groups["author"].Value;
        }

        private string GetProjectUrl()
        {
            string fileContents = this.chaskisCoreProjectContents;
            Match match = Regex.Match(
                fileContents,
                @"\<PackageProjectUrl\>(?<PackageProjectUrl>.+)\</PackageProjectUrl\>"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Project URL Regex" );
            }

            return match.Groups["PackageProjectUrl"].Value;
        }

        private string GetLicenseUrl()
        {
            string fileContents = this.chaskisCoreProjectContents;
            Match match = Regex.Match(
                fileContents,
                @"\<PackageLicenseUrl\>(?<PackageLicenseUrl>.+)\</PackageLicenseUrl\>"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match License URL Regex" );
            }

            return match.Groups["PackageLicenseUrl"].Value;
        }

        private string GetCopyRight()
        {
            string fileContents = this.chaskisCoreProjectContents;
            Match match = Regex.Match(
                fileContents,
                @"\<Copyright\>(?<copyright>.+)\</Copyright\>"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Copyright Regex" );
            }

            return match.Groups["copyright"].Value;
        }

        private string GetDescription()
        {
            string fileContents = this.chaskisCoreProjectContents;
            Match match = Regex.Match(
                fileContents,
                @"(\<Description\>(?<description>.+)\</Description\>)",
                RegexOptions.Singleline
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Description Regex" );
            }

            return match.Groups["description"].Value;
        }

        private string GetReleaseNotes()
        {
            string fileContents = this.chaskisCoreProjectContents;
            Match match = Regex.Match(
                fileContents,
                @"\<PackageReleaseNotes\>(?<PackageReleaseNotes>.+)\</PackageReleaseNotes\>"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Release Notes Regex" );
            }

            return match.Groups["PackageReleaseNotes"].Value;
        }

        private string GetIconUrl()
        {
            string fileContents = this.chaskisCoreProjectContents;
            Match match = Regex.Match(
                fileContents,
                @"\<PackageIconUrl\>(?<PackageIconUrl>.+)\</PackageIconUrl\>"
            );
            if( match.Success == false )
            {
                throw new ApplicationException( "Could not match Icon Url Regex" );
            }

            return match.Groups["PackageIconUrl"].Value;
        }

        private string GetDebChecksum()
        {
            string[] lines = this.CakeContext.FileReadLines( this.ImportantPaths.DebianChecksumFile );
            return lines[0];
        }

        private string GetMsiChecksum()
        {
            string[] lines = this.CakeContext.FileReadLines( this.ImportantPaths.SavedMsiChecksumFile );
            return lines[0];
        }
    }

}
