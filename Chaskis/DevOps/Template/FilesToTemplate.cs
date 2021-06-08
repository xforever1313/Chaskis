//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using Cake.Core.IO;

namespace DevOps.Template
{
    public class FilesToTemplate
    {
        // ---------------- Fields ----------------

        private readonly ImportantPaths paths;

        // ---------------- Constructor ----------------

        public FilesToTemplate( ImportantPaths paths )
        {
            this.paths = paths;

            List<FileToTemplate> files = new List<FileToTemplate>
        {
            // Windows WIX MSI Config.
            new FileToTemplate(
                this.paths.WixConfigFolder.CombineWithFilePath( new FilePath( "Product.wxs.template" ) ),
                this.paths.WixConfigFolder.CombineWithFilePath( new FilePath( "Product.wxs" ) ),
                new List<string>(){ "WINDOWS" }
            ),
            
            // Linux WIX MSI Config.
            new FileToTemplate(
                this.paths.WixConfigFolder.CombineWithFilePath( new FilePath( "Product.wxs.template" ) ),
                this.paths.WixConfigFolder.CombineWithFilePath( new FilePath( "Product.wxs.linux" ) ),
                new List<string>(){ "LINUX" }
            ),
            // Debian Control File
            new FileToTemplate(
                this.paths.DebianLinuxInstallConfigFolder.CombineWithFilePath( new FilePath( "control.template" ) ),
                this.paths.DebianLinuxInstallConfigFolder.CombineWithFilePath( new FilePath( "control" ) )
            ),

            // Fedora Spec File
            new FileToTemplate(
                this.paths.FedoraLinuxInstallConfigFolder.CombineWithFilePath( new FilePath( "chaskis.spec.template" ) ),
                this.paths.FedoraLinuxInstallConfigFolder.CombineWithFilePath( new FilePath( "chaskis.spec" ) )
            ),

            // Root README
            new FileToTemplate(
                this.paths.ProjectRoot.CombineWithFilePath( new FilePath( "README.md.template" ) ),
                this.paths.ProjectRoot.CombineWithFilePath( new FilePath( "README.md" ) )
            ),

            // New Version Notifier
            new FileToTemplate(
                this.paths.RegressionTestFolder.CombineWithFilePath( new FilePath( "Environments/NewVersionNotifierNoChangeEnvironment/Plugins/NewVersionNotifier/.lastversion.txt.template" ) ),
                this.paths.RegressionTestFolder.CombineWithFilePath( new FilePath( "Environments/NewVersionNotifierNoChangeEnvironment/Plugins/NewVersionNotifier/.lastversion.txt" ) )
            ),
            
            // Chaskis Constants for Regression Tests
            new FileToTemplate(
                this.paths.RegressionTestFolder.CombineWithFilePath( new FilePath( "TestFixtures/TestCore/ChaskisConstants.cs.template" ) ),
                this.paths.RegressionTestFolder.CombineWithFilePath( new FilePath( "TestFixtures/TestCore/ChaskisConstants.cs" ) )
            ),
        };

            this.TemplateFiles = files.AsReadOnly();
        }

        // ---------------- Properties ----------------

        public IReadOnlyList<FileToTemplate> TemplateFiles { get; private set; }
    }
}
