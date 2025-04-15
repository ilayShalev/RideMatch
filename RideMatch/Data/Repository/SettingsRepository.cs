using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    public class SettingsRepository
    {
        // Gets the destination information
        public Task<(int Id, string Name, double Latitude, double Longitude, string Address, string TargetTime)> GetDestinationAsync();

       // Updates the destination information
        public Task<bool> UpdateDestinationAsync(string name, double latitude, double longitude, string targetTime, string address);

        // Saves scheduling settings
        public Task SaveSchedulingSettingsAsync(bool isEnabled, DateTime scheduledTime);

        // Gets scheduling settings
        public Task<(bool IsEnabled, DateTime ScheduledTime)> GetSchedulingSettingsAsync();

        // Gets the scheduling log entries
        public Task<List<(DateTime RunTime, string Status, int RoutesGenerated, int PassengersAssigned)>> GetSchedulingLogAsync();

        // Logs a scheduling run
        public Task LogSchedulingRunAsync(DateTime runTime, string status, int routesGenerated, int passengersAssigned, string errorMessage = null);

        // Saves a general setting
        public Task<bool> SaveSettingAsync(string settingName, string settingValue);

        // Gets a general setting
        public Task<string> GetSettingAsync(string settingName, string defaultValue = "");
    }
}
