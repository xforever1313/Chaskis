//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using Cake.Common.Solution;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.LicenseHeaderUpdater;
using Seth.CakeLib;

namespace DevOps.Tasks
{
    [TaskName( "update_licenses" )]
    public class UpdateLicenseHeaderTask : DefaultTask
    {
        // ---------------- Fields ----------------

        const string currentLicense =
@"//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

";
        const string oldLicenseRegex1 =
@"^//
//\s+Copyright\s+Seth\s+Hendrick\s+\d+-?\d*\.?
//\s+Distributed\s+under\s+the\s+Boost\s+Software\s+License,\s+Version\s+1\.0\.?
//\s+\(See\s+accompanying\s+file\s+[./]*LICENSE_1_0\.txt\s+or\s+copy\s+at
//\s+http://www.boost.org/LICENSE_1_0\.txt\)
//[\n\r\s]*";

        const string oldLicenseRegex2 =
@"^//
//\s+Copyright\s+Seth\s+Hendrick\s+[^\n\r]+\.?
//\s+Distributed\s+under\s+the\s+Boost\s+Software\s+License,\s+Version\s+1\.0\.?
//\s+\(See\s+accompanying\s+file\s+[./]*LICENSE_1_0\.txt\s+or\s+copy\s+at
//\s+http://www.boost.org/LICENSE_1_0\.txt\)
//[\n\r\s]*";

        // ---------------- Functions ----------------

        public override void Run( ChaskisContext context )
        {
            CakeLicenseHeaderUpdaterSettings settings = new CakeLicenseHeaderUpdaterSettings
            {
                LicenseString = currentLicense,
                Threads = 0,
            };

            settings.OldHeaderRegexPatterns.Add( oldLicenseRegex1 );
            settings.OldHeaderRegexPatterns.Add( oldLicenseRegex2 );

            settings.FileFilter = SolutionProjectHelpers.DefaultCsFileFilter;

            List<FilePath> files = new List<FilePath>();
            files.Add(
                context.Paths.ProjectRoot.CombineWithFilePath(
                    new FilePath( "./Chaskis/RegressionTests/TestFixtures/TestCore/ChaskisConstants.cs.template" )
                )
            );

            context.PerformActionOnSolutionCsFiles(
                context.Paths.SolutionPath,
                ( path ) => files.Add( path ),
                null,
                delegate( SolutionProject slnProject )
                {
                    if( slnProject.Path.ToString().Contains( "SethCS", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return false;
                    }

                    return true;
                }
            );

            context.UpdateLicenseHeaders( files, settings );
        }
    }
}
