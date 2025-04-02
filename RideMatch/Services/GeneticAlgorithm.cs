using System;
using System.Collections.Generic;
using System.Linq;
using RideMatch.Models;

namespace RideMatch.Services
{
    public class GeneticAlgorithm
    {
        private readonly DatabaseService _databaseService;
        private const int PopulationSize = 100;
        private const int MaxGenerations = 1000;
        private const double MutationRate = 0.01;
        private const double ElitePercentage = 0.2;

        public GeneticAlgorithm(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public void RunAlgorithm()
        {
            var drivers = _databaseService.GetAllDrivers().Where(d => d.IsAvailable).ToList();
            var passengers = _databaseService.GetAllPassengers().Where(p => p.IsAvailable).ToList();
            var vehicles = _databaseService.GetAllVehicles().Where(v => v.IsAvailable).ToList();

            if (!drivers.Any() || !passengers.Any() || !vehicles.Any())
            {
                throw new InvalidOperationException("No available drivers, passengers, or vehicles found.");
            }

            var population = InitializePopulation(drivers, passengers, vehicles);
            var bestSolution = null as Dictionary<int, List<int>>;
            var bestFitness = double.MaxValue;
            var generationsWithoutImprovement = 0;

            for (int generation = 0; generation < MaxGenerations; generation++)
            {
                // Evaluate fitness for each solution
                var solutions = population.Select(s => new
                {
                    Solution = s,
                    Fitness = CalculateFitness(s, drivers, passengers, vehicles)
                }).OrderBy(x => x.Fitness).ToList();

                // Update best solution if found
                if (solutions[0].Fitness < bestFitness)
                {
                    bestSolution = solutions[0].Solution;
                    bestFitness = solutions[0].Fitness;
                    generationsWithoutImprovement = 0;
                }
                else
                {
                    generationsWithoutImprovement++;
                }

                // Check stopping conditions
                if (generationsWithoutImprovement >= 20 || bestFitness < 0.1)
                {
                    break;
                }

                // Create new population
                population = CreateNewPopulation(solutions.Select(s => s.Solution).ToList());
            }

            // Apply the best solution found
            if (bestSolution != null)
            {
                ApplySolution(bestSolution, passengers);
            }
        }

        private List<Dictionary<int, List<int>>> InitializePopulation(List<Driver> drivers, List<Passenger> passengers, List<Vehicle> vehicles)
        {
            var population = new List<Dictionary<int, List<int>>>();
            var random = new Random();

            for (int i = 0; i < PopulationSize; i++)
            {
                var solution = new Dictionary<int, List<int>>();
                var availableDrivers = new List<Driver>(drivers);
                var availablePassengers = new List<Passenger>(passengers);

                while (availablePassengers.Any())
                {
                    if (!availableDrivers.Any())
                    {
                        break;
                    }

                    var driver = availableDrivers[random.Next(availableDrivers.Count)];
                    var vehicle = vehicles.FirstOrDefault(v => v.DriverId == driver.Id);
                    if (vehicle == null)
                    {
                        availableDrivers.Remove(driver);
                        continue;
                    }

                    var passengerCount = random.Next(1, Math.Min(vehicle.Capacity + 1, availablePassengers.Count + 1));
                    var selectedPassengers = availablePassengers.Take(passengerCount).Select(p => p.Id).ToList();
                    solution[driver.Id] = selectedPassengers;

                    availableDrivers.Remove(driver);
                    availablePassengers.RemoveRange(0, passengerCount);
                }

                population.Add(solution);
            }

            return population;
        }

        private double CalculateFitness(Dictionary<int, List<int>> solution, List<Driver> drivers, List<Passenger> passengers, List<Vehicle> vehicles)
        {
            double fitness = 0;
            var random = new Random();

            foreach (var driverAssignment in solution)
            {
                var driver = drivers.First(d => d.Id == driverAssignment.Key);
                var vehicle = vehicles.First(v => v.DriverId == driver.Id);
                var assignedPassengers = driverAssignment.Value.Select(id => passengers.First(p => p.Id == id)).ToList();

                // Check vehicle capacity
                if (assignedPassengers.Count > vehicle.Capacity)
                {
                    fitness += 1000; // Penalty for exceeding capacity
                }

                // Calculate total distance (simplified)
                foreach (var passenger in assignedPassengers)
                {
                    // In a real implementation, you would use actual distance calculation
                    fitness += random.Next(1, 10);
                }
            }

            // Add penalty for unassigned passengers
            var assignedPassengerIds = solution.Values.SelectMany(v => v).ToList();
            var unassignedPassengers = passengers.Count - assignedPassengerIds.Count;
            fitness += unassignedPassengers * 100;

            return fitness;
        }

        private List<Dictionary<int, List<int>>> CreateNewPopulation(List<Dictionary<int, List<int>>> currentPopulation)
        {
            var newPopulation = new List<Dictionary<int, List<int>>>();
            var random = new Random();

            // Keep elite solutions
            var eliteCount = (int)(PopulationSize * ElitePercentage);
            newPopulation.AddRange(currentPopulation.Take(eliteCount));

            // Create new solutions through crossover and mutation
            while (newPopulation.Count < PopulationSize)
            {
                var parent1 = currentPopulation[random.Next(currentPopulation.Count)];
                var parent2 = currentPopulation[random.Next(currentPopulation.Count)];
                var child = Crossover(parent1, parent2);

                if (random.NextDouble() < MutationRate)
                {
                    child = Mutate(child);
                }

                newPopulation.Add(child);
            }

            return newPopulation;
        }

        private Dictionary<int, List<int>> Crossover(Dictionary<int, List<int>> parent1, Dictionary<int, List<int>> parent2)
        {
            var child = new Dictionary<int, List<int>>();
            var random = new Random();

            // Take half of the assignments from each parent
            var parent1Drivers = parent1.Keys.ToList();
            var parent2Drivers = parent2.Keys.ToList();

            var selectedDrivers = parent1Drivers.Take(parent1Drivers.Count / 2).ToList();
            selectedDrivers.AddRange(parent2Drivers.Where(d => !selectedDrivers.Contains(d)));

            foreach (var driverId in selectedDrivers)
            {
                if (parent1.ContainsKey(driverId))
                {
                    child[driverId] = new List<int>(parent1[driverId]);
                }
                else if (parent2.ContainsKey(driverId))
                {
                    child[driverId] = new List<int>(parent2[driverId]);
                }
            }

            return child;
        }

        private Dictionary<int, List<int>> Mutate(Dictionary<int, List<int>> solution)
        {
            var random = new Random();
            var mutatedSolution = new Dictionary<int, List<int>>(solution);

            // Randomly swap two passengers between drivers
            if (mutatedSolution.Count >= 2)
            {
                var drivers = mutatedSolution.Keys.ToList();
                var driver1 = drivers[random.Next(drivers.Count)];
                var driver2 = drivers[random.Next(drivers.Count)];

                if (mutatedSolution[driver1].Any() && mutatedSolution[driver2].Any())
                {
                    var passenger1Index = random.Next(mutatedSolution[driver1].Count);
                    var passenger2Index = random.Next(mutatedSolution[driver2].Count);

                    var temp = mutatedSolution[driver1][passenger1Index];
                    mutatedSolution[driver1][passenger1Index] = mutatedSolution[driver2][passenger2Index];
                    mutatedSolution[driver2][passenger2Index] = temp;
                }
            }

            return mutatedSolution;
        }

        private void ApplySolution(Dictionary<int, List<int>> solution, List<Passenger> passengers)
        {
            foreach (var driverAssignment in solution)
            {
                var driverId = driverAssignment.Key;
                var passengerIds = driverAssignment.Value;

                // Calculate pickup times (simplified)
                var baseTime = DateTime.Now.Date.AddHours(7); // Assuming school starts at 7:00
                var timePerPickup = TimeSpan.FromMinutes(5);

                for (int i = 0; i < passengerIds.Count; i++)
                {
                    var pickupTime = baseTime.Add(timePerPickup * i);
                    _databaseService.UpdatePassengerAssignment(passengerIds[i], driverId, pickupTime);
                }
            }
        }
    }
} 