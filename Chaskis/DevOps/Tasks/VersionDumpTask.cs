//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Cake.ArgumentBinder;
using Cake.Common.Diagnostics;
using Cake.Frosting;
using DevOps.Template;

namespace DevOps.Tasks
{
    [TaskName( "dump_version" )]
    [TaskDescription( "Dumps the version of Chaskis to a file." )]
    public class VersionDumpTask : DefaultTask
    {
        // ---------------- Functions ----------------

        public override void Run( ChaskisContext context )
        {
            VersionDumpRunner runner = new VersionDumpRunner( context );
            runner.DumpVersion();
        }
    }

    public class VersionDumpSettings
    {
        [StringArgument(
            "output",
            Description = "File to output the version to",
            DefaultValue = "version.txt"
        )]
        public string OutputLocation { get; set; }
    }

    public class VersionDumpRunner
    {
        // ---------------- Fields ----------------

        private readonly ChaskisContext context;

        private readonly VersionDumpSettings settings;

        private readonly TemplateConstants templateConstants;

        // ---------------- Constructor ----------------

        public VersionDumpRunner( ChaskisContext context ) :
            this( context, ArgumentBinder.FromArguments<VersionDumpSettings>( context ) )
        {
        }

        public VersionDumpRunner( ChaskisContext context, VersionDumpSettings settings )
        {
            this.context = context;
            this.settings = settings;

            this.templateConstants = new TemplateConstants(
                this.context
            );
        }

        // ---------------- Functions ----------------

        public void DumpVersion()
        {
            string version = this.templateConstants.ChaskisVersion;

            this.context.Information( $"Writing version '{version}' to '{this.settings.OutputLocation}'" );
            System.IO.File.WriteAllText( this.settings.OutputLocation, version );
        }
    }
}
