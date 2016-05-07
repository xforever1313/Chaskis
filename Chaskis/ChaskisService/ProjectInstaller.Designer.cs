namespace ChaskisService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ChaskisProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ChaskisInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ChaskisProcessInstaller
            // 
            this.ChaskisProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.ChaskisProcessInstaller.Password = null;
            this.ChaskisProcessInstaller.Username = null;
            // 
            // ChaskisInstaller
            // 
            this.ChaskisInstaller.Description = "A .NET IRC Bot";
            this.ChaskisInstaller.DisplayName = "Chaskis IRC Bot";
            this.ChaskisInstaller.ServiceName = "Chaskis";
            this.ChaskisInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ChaskisProcessInstaller,
            this.ChaskisInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ChaskisProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ChaskisInstaller;
    }
}