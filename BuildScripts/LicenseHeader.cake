Task( "update_licenses" )
.Does(
    () =>
    {
        const string currentLicense =
@"//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

";
        const string oldLicenseRegex1 =
@"//
//\s+Copyright\s+Seth\s+Hendrick\s+\d+-?\d*\.?
//\s+Distributed\s+under\s+the\s+Boost\s+Software\s+License,\s+Version\s+1\.0\.?
//\s+\(See\s+accompanying\s+file\s+[./]*LICENSE_1_0\.txt\s+or\s+copy\s+at
//\s+http://www.boost.org/LICENSE_1_0\.txt\)
//[\n\r\s]*";

        const string oldLicenseRegex2 =
@"//\s+Copyright\s+Seth\s+Hendrick\s+\d+-?\d*\.?
//\s+Distributed\s+under\s+the\s+Boost\s+Software\s+License,\s+Version\s+1\.0\.?
//\s+\(See\s+accompanying\s+file\s+[./]*LICENSE_1_0\.txt\s+or\s+copy\s+at
//\s+http://www.boost.org/LICENSE_1_0\.txt\)[\n\r\s]*";

        const string oldLicenseRegex3 =
@"//
//\s+Copyright\s+Seth\s+Hendrick\s+[^\n\r]+\.?
//\s+Distributed\s+under\s+the\s+Boost\s+Software\s+License,\s+Version\s+1\.0\.?
//\s+\(See\s+accompanying\s+file\s+[./]*LICENSE_1_0\.txt\s+or\s+copy\s+at
//\s+http://www.boost.org/LICENSE_1_0\.txt\)
//[\n\r\s]*";

        CakeLicenseHeaderUpdaterSettings settings = new CakeLicenseHeaderUpdaterSettings
        {
            LicenseString = currentLicense,
            Threads = 0,
        };

        settings.OldHeaderRegexPatterns.Add( oldLicenseRegex1 );
        settings.OldHeaderRegexPatterns.Add( oldLicenseRegex2 );
        settings.OldHeaderRegexPatterns.Add( oldLicenseRegex3 );

        settings.FileFilter = delegate( FilePath path )
        {
            if( Regex.IsMatch( path.ToString(), @"[/\\]obj[/\\]" ) )
            {
                return false;
            }
            if( Regex.IsMatch( path.ToString(), @"[/\\]bin[/\\]" ) )
            {
                return false;
            }
            else
            {
                return true;
            }
        };

        List<FilePath> files = new List<FilePath>();

        files.Add( File( "./Chaskis/RegressionTests/TestFixtures/TestCore/ChaskisConstants.cs.template" ) );

        SolutionParserResult slnResult = ParseSolution( paths.SolutionPath );
        foreach( SolutionProject proj in slnResult.Projects )
        {
            if( proj.Path.ToString().EndsWith( ".csproj" ) )
            {
                string glob = proj.Path.GetDirectory() + "/**/*.cs";
                files.AddRange( GetFiles( glob ) );
            }
        }

        UpdateLicenseHeaders( files, settings );
    }
).Description( "Updates all the license headers in all .cs projects" );
