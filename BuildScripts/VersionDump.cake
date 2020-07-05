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

    private readonly ICakeContext context;

    private readonly VersionDumpSettings settings;

    private readonly TemplateConstants templateConstants;

    // ---------------- Constructor ----------------

    public VersionDumpRunner( ICakeContext context, ImportantPaths paths ) :
        this( context, paths, ArgumentBinder.FromArguments<VersionDumpSettings>( context ) )
    {
    }

    public VersionDumpRunner( ICakeContext context, ImportantPaths paths, VersionDumpSettings settings )
    {
        this.context = context;
        this.settings = settings;

        this.templateConstants = new TemplateConstants(
            this.context,
            paths,
            frameworkTarget
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
