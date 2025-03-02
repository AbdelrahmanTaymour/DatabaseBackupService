using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBackupService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                MyDatabaseBackupService service = new MyDatabaseBackupService();
                service.StartInConsole();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new MyDatabaseBackupService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
