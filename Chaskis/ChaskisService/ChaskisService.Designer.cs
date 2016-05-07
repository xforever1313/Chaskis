namespace ChaskisService
{
    partial class ChaskisService
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
            this.ChaskisEventLog = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.ChaskisEventLog)).BeginInit();
            // 
            // ChaskisEventLog
            // 
            this.ChaskisEventLog.Log = "ChaskisLog";
            this.ChaskisEventLog.Source = "ChaskisSource";
            // 
            // ChaskisService
            // 
            this.CanShutdown = true;
            this.ServiceName = "Chaskis";
            ((System.ComponentModel.ISupportInitialize)(this.ChaskisEventLog)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog ChaskisEventLog;
    }
}
