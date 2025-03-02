using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseBackupService
{
    public partial class MyDatabaseBackupService : ServiceBase
    {
        private Timer backupTimer;
        private string connectionString;
        private string backupFolder;
        private string logFolder;
        private int backupIntervalMinutes;
        public MyDatabaseBackupService()
        {
            InitializeComponent();

            connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            backupFolder = ConfigurationManager.AppSettings["BackupFolder"];
            logFolder = ConfigurationManager.AppSettings["LogFolder"];
            
            if (int.TryParse(ConfigurationManager.AppSettings["BackupIntervalMinutes"], out int intervalMinutes))
            {
                backupIntervalMinutes = intervalMinutes;
            }
            else
            {
                backupIntervalMinutes = 60;
                Log($"The Backup Interval Minutes is missing in App.congig: Using defult: {backupIntervalMinutes}");
            }

            if(string.IsNullOrWhiteSpace(connectionString))
            {

                connectionString = "Server=.;Database=HotelDatabase;Integrated Security=True;";
                Log($"Connection String is missing in App.congig: Using defult: {connectionString}");
            }

            if(string.IsNullOrWhiteSpace(backupFolder))
            {
                backupFolder = @"C:\DatabaseBackups";
                Log($"Backup Folder Path is missing in App.congig: Using defult: {backupFolder}");
            }

            if(string.IsNullOrWhiteSpace(logFolder))
            {
                logFolder = @"C:\DatabaseBackups\Logs";
                Log($"Log Folder Path is missing in App.congig: Using defult: {logFolder}");
            }

            Directory.CreateDirectory( backupFolder );
            Directory.CreateDirectory( logFolder );
        }

        protected override void OnStart(string[] args)
        {
            Log("Service Started.");

            backupTimer =  new Timer(
                callback: PerformBackup,
                state: null,
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromMinutes(backupIntervalMinutes)
            );

            Log($"Backup schedule initiated: every {backupIntervalMinutes} minute(s).");
        }

        protected override void OnStop()
        {
            backupTimer.Dispose();
            Log("Service Stopped.");
        }

        private void PerformBackup(object state)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFilePath = $@"{backupFolder}\Backup_{timestamp}.bak";

            if (DatabaseBackup(backupFilePath))
            {
                Log($"Database backup successful: {backupFilePath}");
            }

        }

        private bool DatabaseBackup(string backupFilePath)
        {
            bool isSuccess = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using(SqlCommand command = new SqlCommand("SP_DatabaseBackup", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@backupPath", backupFilePath);
                        command.ExecuteNonQuery();

                        isSuccess = true;
                    }
                }
            }
            catch( Exception ex )
            {
                isSuccess = false;
                Log($"Error during backup: {ex.Message}");
            }

            return isSuccess;
        }

        private void Log(string message)
        {
            string logFile = Path.Combine(logFolder, "BackupLogs.txt");
            string logMessage = $"[{DateTime.Now:yyyy:MM:dd HH:mm:ss}] {message}";

            File.AppendAllText(logFile, logMessage + Environment.NewLine);

            if (Environment.UserInteractive)
            {
                Console.WriteLine(logMessage);
            }
        }

        public void StartInConsole()
        {
            OnStart(null);
            Console.WriteLine("Press Enter to stop the service...");
            Console.ReadLine();
            OnStop();
            Console.ReadKey();
        }
    }
}
