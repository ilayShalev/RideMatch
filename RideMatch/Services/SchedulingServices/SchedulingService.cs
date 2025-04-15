using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Services.SchedulingServices
{
    public class SchedulingService : ISchedulerService
    {
        // Gets scheduling settings
        public Task<SchedulingSetting> GetSchedulingSettingsAsync();

        // Saves scheduling settings
        public Task<bool> SaveSchedulingSettingsAsync(bool isEnabled, DateTime scheduledTime);

        // Runs the scheduling algorithm
        public Task<bool> RunSchedulerAsync();

        // Gets the scheduling log
        public Task<IEnumerable<SchedulingLogEntry>> GetSchedulingLogAsync();

        // Logs a scheduling run
        public Task<bool> LogSchedulingRunAsync(DateTime runTime, string status, int routesGenerated, int passengersAssigned, string message = null);

        // Calculates pickup times based on target arrival time
        private Task CalculatePickupTimesBasedOnTargetArrival(Solution solution, string targetTimeString, IRoutingService routingService);

        // Logs a message to the service log file
        private void Log(string message);
    }
}
