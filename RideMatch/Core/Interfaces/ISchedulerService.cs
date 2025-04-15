using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Interfaces
{
    public interface ISchedulerService
    {
        // Gets scheduling settings
        Task<SchedulingSetting> GetSchedulingSettingsAsync();

        // Saves scheduling settings
        Task<bool> SaveSchedulingSettingsAsync(bool isEnabled, DateTime scheduledTime);

        // Runs the scheduling algorithm
        Task<bool> RunSchedulerAsync();

        // Gets the scheduling log
        Task<IEnumerable<SchedulingLogEntry>> GetSchedulingLogAsync();

        // Logs a scheduling run
        Task<bool> LogSchedulingRunAsync(DateTime runTime, string status, int routesGenerated, int passengersAssigned, string message = null);
    }
}
