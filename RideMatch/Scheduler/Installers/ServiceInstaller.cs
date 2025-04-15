using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;


namespace RideMatch.Scheduler.Installers
{
    [RunInstaller(true)]
    public class ServiceInstaller : Installer
    {
        private System.ServiceProcess.ServiceInstaller serviceInstaller;
        private System.ServiceProcess.ServiceProcessInstaller processInstaller;
        private string _serviceName = "RideMatchScheduler";
        private string _serviceDisplayName = "RideMatch Ride Scheduling Service";
        private string _serviceDescription = "Scheduler service for automated ride matching and route optimization";

        public ServiceInstaller()
        {
            InitializeComponent();

            // Event handler for after installation
            this.Committed += ProjectInstaller_Committed;
        }

        /// <summary>
        /// Initializes the installer components
        /// </summary>
        private void InitializeComponent()
        {
            // Process installer settings
            processInstaller = new System.ServiceProcess.ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem,
                Username = null,
                Password = null
            };

            // Service installer settings
            serviceInstaller = new System.ServiceProcess.ServiceInstaller
            {
                ServiceName = _serviceName,
                DisplayName = _serviceDisplayName,
                Description = _serviceDescription,
                StartType = ServiceStartMode.Automatic
            };

            // Add installers to collection
            Installers.AddRange(new Installer[] {
                processInstaller,
                serviceInstaller
            });
        }

        /// <summary>
        /// Called after the installer is committed
        /// </summary>
        private void ProjectInstaller_Committed(object sender, InstallEventArgs e)
        {
            try
            {
                // Set up recovery actions for the service
                SetupRecoveryActions(_serviceName);

                // Log installation details
                LogInstallation("Service installation completed successfully.");

                // Auto-start the service after installation
                using (ServiceController sc = new ServiceController(_serviceName))
                {
                    if (sc.Status != ServiceControllerStatus.Running)
                    {
                        sc.Start();
                        LogInstallation("Service started successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogInstallation($"Error during post-installation: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up recovery actions for the service
        /// </summary>
        private void SetupRecoveryActions(string serviceName)
        {
            try
            {
                // Set up recovery actions using sc.exe command-line tool
                // First action: Restart after 1 minute
                // Second action: Restart after 5 minutes
                // Third action: Run a command after 10 minutes
                // Reset failure count after 1 day
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"failure \"{serviceName}\" reset= 86400 actions= restart/60000/restart/300000/run/600000",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    LogInstallation($"Recovery settings applied: {output}");
                }
            }
            catch (Exception ex)
            {
                LogInstallation($"Error setting up recovery actions: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs installation details to a file
        /// </summary>
        private void LogInstallation(string message)
        {
            try
            {
                // Create log directory if it doesn't exist
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "RideMatch");

                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                string logFilePath = Path.Combine(appDataPath, "installer.log");

                // Append to log file
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch
            {
                // Cannot log the error if logging itself fails
            }
        }

        /// <summary>
        /// Entry point for installing the service
        /// </summary>
        public static void InstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] {
                    System.Reflection.Assembly.GetExecutingAssembly().Location
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing service: {ex.Message}");
            }
        }

        /// <summary>
        /// Entry point for uninstalling the service
        /// </summary>
        public static void UninstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] {
                    "/u", System.Reflection.Assembly.GetExecutingAssembly().Location
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uninstalling service: {ex.Message}");
            }
        }
    }
}