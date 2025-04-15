using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Scheduler.Installer
{
    public class ServiceInstaller : Installer
    {
        // Initializes the installer
        private void InitializeComponent();

        // Called after the installer is committed
        private void ProjectInstaller_Committed(object sender, InstallEventArgs e);

        // Sets up recovery actions for the service
        private void SetupRecoveryActions(string serviceName);

        // Logs installation details
        private void LogInstallation(string message);
    }
}
