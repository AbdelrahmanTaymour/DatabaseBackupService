using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace DatabaseBackupService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();

            serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            serviceInstaller = new ServiceInstaller
            {
                ServiceName = "MyDatabaseBackupService",
                DisplayName = "My Database Backup Service",
                Description = "This is my Database Backup Service",
                StartType = ServiceStartMode.Automatic,
                ServicesDependedOn = new string[] { "MSSQLSERVER", "RpcSs", "EventLog" }
            };

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
