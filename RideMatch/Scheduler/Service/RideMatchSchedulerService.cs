using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RideMatch.Scheduler.Service
{
    public class RideMatchSchedulerService : ServiceBase
    {
        // Initializes components
        private void InitializeComponent();

        // Called when service starts
        protected override void OnStart(string[] args);

        // Called when service stops
        protected override void OnStop();

        // Called when service is paused
        protected override void OnPause();

        // Called when service is resumed
        protected override void OnContinue();

        // Called when system is shutting down
        protected override void OnShutdown();

        // Checks if it's time to run the scheduler
        private async void CheckScheduleTime(object sender, ElapsedEventArgs e);

        // Runs the algorithm
        private async Task RunAlgorithmAsync();

        // Calculates pickup times based on target arrival time
        private async Task CalculatePickupTimesBasedOnTargetArrival(Solution solution, string targetTimeString, RoutingService routingService);

        // Converts target time to minutes
        private int GetTargetTimeInMinutes(string targetTime);

        // Logs a message to the service log file
        private void Log(string message);
    }
}
