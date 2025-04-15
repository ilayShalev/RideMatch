using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Scheduler.Service
{
    public class ScheduleTask
    {
        // Executes the scheduling algorithm
        public async Task<bool> ExecuteAsync();

        // Generates optimal routes
        private async Task<Solution> GenerateRoutesAsync(IEnumerable<Vehicle> vehicles, IEnumerable<Passenger> passengers);

        // Calculates pickup times based on target arrival
        private async Task CalculatePickupTimesAsync(Solution solution, DateTime targetTime);

        // Saves the generated solution
        private async Task<int> SaveSolutionAsync(Solution solution, string date);

        // Logs the execution results
        private async Task LogExecutionAsync(bool success, int vehiclesUsed, int passengersAssigned, string message = null);
    }

}
