using System.ComponentModel;

namespace ChaskisService
{
    [RunInstaller( true )]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}