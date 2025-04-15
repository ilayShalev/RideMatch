using RideMatch.Core.Models;
using RideMatch.Utilities.Geo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RideMatch.Services.RouteServices
{
    public class RideSharingAlgorithm
    {
        private List<Vehicle> _vehicles;
        private List<Passenger> _passengers;
        private List<Solution> _population;
        private Random _random;
        private int _populationSize = 50;
        private double _mutationRate = 0.2;
        private double _crossoverRate = 0.8;
        private double _destinationLat;
        private double _destinationLng;

        public RideSharingAlgorithm(List<Vehicle> vehicles, List<Passenger> passengers, double destinationLat, double destinationLng)
        {
            _vehicles = vehicles ?? new List<Vehicle>();
            _passengers = passengers ?? new List<Passenger>();
            _random = new Random();
            _population = new List<Solution>();
            _destinationLat = destinationLat;
            _destinationLng = destinationLng;
        }

        // Solves the ride sharing problem using a genetic algorithm
        public Solution Solve(int generations, List<Solution> initialPopulation = null)
        {
            // Use initial population if provided, otherwise generate one
            if (initialPopulation != null && initialPopulation.Count > 0)
            {
                _population = initialPopulation;
            }
            else
            {
                _population = GenerateInitialPopulation();
            }

            // Evolve the population for the specified number of generations
            for (int i = 0; i < generations; i++)
            {
                EvolveSingleGeneration();
            }

            // Return the best solution
            return GetBestSolution();
        }

        // Evolves the population by one generation
        private void EvolveSingleGeneration()
        {
            List<Solution> newPopulation = new List<Solution>();

            // Elitism: Keep the best solution
            Solution bestSolution = GetBestSolution();
            newPopulation.Add(bestSolution.Clone());

            // Generate the rest of the new population
            while (newPopulation.Count < _populationSize)
            {
                // Select parents using tournament selection
                Solution parent1 = TournamentSelection();
                Solution parent2 = TournamentSelection();

                // Apply crossover with probability _crossoverRate
                Solution offspring;
                if (_random.NextDouble() < _crossoverRate)
                {
                    offspring = Crossover(parent1, parent2);
                }
                else
                {
                    // No crossover, just clone one of the parents
                    offspring = _random.Next(2) == 0 ? parent1.Clone() : parent2.Clone();
                }

                // Apply mutation with probability _mutationRate
                if (_random.NextDouble() < _mutationRate)
                {
                    Mutate(offspring);
                }

                newPopulation.Add(offspring);
            }

            // Replace the old population with the new one
            _population = newPopulation;
        }

        // Gets the latest population of solutions
        public List<Solution> GetLatestPopulation()
        {
            return _population;
        }

        // Generates initial population of solutions
        private List<Solution> GenerateInitialPopulation()
        {
            List<Solution> initialPopulation = new List<Solution>();

            // Add a greedy solution
            initialPopulation.Add(CreateGreedySolution());

            // Add an even distribution solution
            initialPopulation.Add(CreateEvenDistributionSolution());

            // Generate random solutions for the rest of the population
            while (initialPopulation.Count < _populationSize)
            {
                // Create a random solution
                Solution randomSolution = new Solution();
                randomSolution.Vehicles = DeepCopyVehicles();

                // Randomly assign passengers to vehicles
                foreach (var passenger in _passengers)
                {
                    // Only assign available passengers
                    if (!passenger.IsAvailable)
                        continue;

                    // Find available vehicles (with capacity)
                    var availableVehicles = randomSolution.Vehicles
                        .Where(v => v.IsAvailable && v.HasCapacity(1))
                        .ToList();

                    if (availableVehicles.Count > 0)
                    {
                        // Assign to a random available vehicle
                        var randomVehicle = availableVehicles[_random.Next(availableVehicles.Count)];
                        randomVehicle.Passengers.Add(passenger.Clone());
                    }
                }

                // Evaluate the solution
                randomSolution.Score = Evaluate(randomSolution);
                initialPopulation.Add(randomSolution);
            }

            return initialPopulation;
        }

        // Creates a greedy solution that assigns passengers to closest vehicles first
        private Solution CreateGreedySolution()
        {
            Solution solution = new Solution();
            solution.Vehicles = DeepCopyVehicles();

            // Sort passengers by their distance to the destination (furthest first)
            var sortedPassengers = _passengers
                .Where(p => p.IsAvailable)
                .OrderByDescending(p => p.DistanceTo(_destinationLat, _destinationLng))
                .ToList();

            foreach (var passenger in sortedPassengers)
            {
                // Find the closest vehicle with available capacity
                var availableVehicles = solution.Vehicles
                    .Where(v => v.IsAvailable && v.HasCapacity(1))
                    .ToList();

                if (availableVehicles.Count > 0)
                {
                    var closestVehicle = availableVehicles
                        .OrderBy(v => v.DistanceTo(passenger.Latitude, passenger.Longitude))
                        .First();

                    closestVehicle.Passengers.Add(passenger.Clone());
                }
            }

            solution.Score = Evaluate(solution);
            return solution;
        }

        // Creates a solution that distributes passengers evenly among vehicles
        private Solution CreateEvenDistributionSolution()
        {
            Solution solution = new Solution();
            solution.Vehicles = DeepCopyVehicles();

            // Get available vehicles and passengers
            var availableVehicles = solution.Vehicles.Where(v => v.IsAvailable).ToList();
            var availablePassengers = _passengers.Where(p => p.IsAvailable).ToList();

            if (availableVehicles.Count == 0 || availablePassengers.Count == 0)
            {
                solution.Score = Evaluate(solution);
                return solution;
            }

            // Calculate target number of passengers per vehicle
            int totalCapacity = availableVehicles.Sum(v => v.Capacity);
            int passengerCount = Math.Min(availablePassengers.Count, totalCapacity);

            // Simple round-robin assignment
            int vehicleIndex = 0;
            foreach (var passenger in availablePassengers)
            {
                // Find the next vehicle with capacity
                while (!availableVehicles[vehicleIndex].HasCapacity(1))
                {
                    vehicleIndex = (vehicleIndex + 1) % availableVehicles.Count;
                }

                availableVehicles[vehicleIndex].Passengers.Add(passenger.Clone());
                vehicleIndex = (vehicleIndex + 1) % availableVehicles.Count;
            }

            solution.Score = Evaluate(solution);
            return solution;
        }

        // Creates a deep copy of vehicles without passengers
        private List<Vehicle> DeepCopyVehicles()
        {
            return _vehicles
                .Where(v => v.IsAvailable)
                .Select(v => v.Clone())
                .ToList();
        }

        // Evaluates a solution and assigns a score (higher is better)
        private double Evaluate(Solution solution)
        {
            // Initialize base score
            double score = 0;

            // Get the number of assigned passengers
            int assignedPassengers = solution.GetAssignedPassengerCount();

            // The primary objective is to maximize the number of assigned passengers
            score += assignedPassengers * 1000;

            // Secondary objective is to minimize the total distance
            double totalDistance = solution.GetTotalDistance();
            if (totalDistance > 0)
            {
                // Penalize long distances
                score -= totalDistance * 0.01;
            }

            // Tertiary objective is to minimize the number of vehicles used
            int usedVehicles = solution.GetUsedVehicleCount();
            score -= usedVehicles * 50;

            // Penalty for unbalanced vehicle loads
            double loadBalancePenalty = 0;
            var vehiclesWithPassengers = solution.Vehicles.Where(v => v.Passengers.Count > 0).ToList();
            if (vehiclesWithPassengers.Count > 1)
            {
                double avgPassengersPerVehicle = (double)assignedPassengers / vehiclesWithPassengers.Count;
                loadBalancePenalty = vehiclesWithPassengers.Sum(v =>
                    Math.Pow(v.Passengers.Count - avgPassengersPerVehicle, 2));
                score -= loadBalancePenalty * 10;
            }

            return score;
        }

        // Calculates route metrics for a vehicle
        private (double TotalDistance, double TotalTime) CalculateRouteMetrics(Vehicle vehicle)
        {
            if (vehicle.Passengers.Count == 0)
                return (0, 0);

            double totalDistance = 0;
            double totalTime = 0; // Time in minutes

            // Starting point is the vehicle's location
            double currentLat = vehicle.Latitude;
            double currentLng = vehicle.Longitude;

            // Calculate distance to pick up each passenger
            foreach (var passenger in vehicle.Passengers)
            {
                // Distance from current location to passenger
                double distance = DistanceCalculator.CalculateDistance(
                    currentLat, currentLng, passenger.Latitude, passenger.Longitude);

                totalDistance += distance;

                // Estimate time: assume average speed of 30 km/h in urban areas
                // Convert distance (km) to time (minutes)
                totalTime += (distance / 30) * 60;

                // Update current location to passenger's location
                currentLat = passenger.Latitude;
                currentLng = passenger.Longitude;
            }

            // Add distance from last passenger to destination
            double distanceToDestination = DistanceCalculator.CalculateDistance(
                currentLat, currentLng, _destinationLat, _destinationLng);

            totalDistance += distanceToDestination;
            totalTime += (distanceToDestination / 30) * 60;

            return (totalDistance, totalTime);
        }

        // Calculates the additional distance if a passenger is added to a vehicle
        private double CalculateAdditionalDistance(Vehicle vehicle, Passenger passenger)
        {
            // If the vehicle has no passengers, calculate direct route
            if (vehicle.Passengers.Count == 0)
            {
                // Distance from vehicle to passenger
                double distToPassenger = DistanceCalculator.CalculateDistance(
                    vehicle.Latitude, vehicle.Longitude, passenger.Latitude, passenger.Longitude);

                // Distance from passenger to destination
                double distToDestination = DistanceCalculator.CalculateDistance(
                    passenger.Latitude, passenger.Longitude, _destinationLat, _destinationLng);

                return distToPassenger + distToDestination;
            }

            // Otherwise, calculate the difference in route length
            double originalDistance = 0;
            double newDistance = 0;

            // Get last passenger in the route
            var lastPassenger = vehicle.Passengers.Last();

            // Original: distance from last passenger to destination
            originalDistance = DistanceCalculator.CalculateDistance(
                lastPassenger.Latitude, lastPassenger.Longitude, _destinationLat, _destinationLng);

            // New: distance from last passenger to new passenger, plus new passenger to destination
            double distToNewPassenger = DistanceCalculator.CalculateDistance(
                lastPassenger.Latitude, lastPassenger.Longitude, passenger.Latitude, passenger.Longitude);

            double newPassengerToDestination = DistanceCalculator.CalculateDistance(
                passenger.Latitude, passenger.Longitude, _destinationLat, _destinationLng);

            newDistance = distToNewPassenger + newPassengerToDestination;

            // Return the additional distance
            return newDistance - originalDistance;
        }

        // Selects a solution using tournament selection
        private Solution TournamentSelection()
        {
            // Tournament size is 3 (can be adjusted)
            int tournamentSize = 3;
            List<Solution> tournament = new List<Solution>();

            // Randomly select solutions for the tournament
            for (int i = 0; i < tournamentSize; i++)
            {
                int randomIndex = _random.Next(_population.Count);
                tournament.Add(_population[randomIndex]);
            }

            // Return the solution with the highest score
            return tournament.OrderByDescending(s => s.Score).First();
        }

        // Performs crossover between two parent solutions
        private Solution Crossover(Solution parent1, Solution parent2)
        {
            // Create a new solution with empty vehicles
            Solution offspring = new Solution();
            offspring.Vehicles = DeepCopyVehicles();

            // Get all assigned passengers from both parents
            HashSet<int> assignedPassengerIds = new HashSet<int>();

            // Dictionary to map vehicle IDs to their index in the offspring's vehicle list
            Dictionary<int, int> vehicleIdToIndex = new Dictionary<int, int>();
            for (int i = 0; i < offspring.Vehicles.Count; i++)
            {
                vehicleIdToIndex[offspring.Vehicles[i].Id] = i;
            }

            // Randomly decide which parent to prioritize
            Solution primaryParent = _random.Next(2) == 0 ? parent1 : parent2;
            Solution secondaryParent = primaryParent == parent1 ? parent2 : parent1;

            // First, copy passenger assignments from the primary parent
            foreach (var vehicle in primaryParent.Vehicles)
            {
                if (!vehicleIdToIndex.ContainsKey(vehicle.Id))
                    continue;

                int offspringVehicleIndex = vehicleIdToIndex[vehicle.Id];

                // Copy passengers from this vehicle
                foreach (var passenger in vehicle.Passengers)
                {
                    if (!assignedPassengerIds.Contains(passenger.Id) &&
                        offspring.Vehicles[offspringVehicleIndex].HasCapacity(1))
                    {
                        offspring.Vehicles[offspringVehicleIndex].Passengers.Add(passenger.Clone());
                        assignedPassengerIds.Add(passenger.Id);
                    }
                }
            }

            // Then, copy remaining passenger assignments from the secondary parent
            foreach (var vehicle in secondaryParent.Vehicles)
            {
                if (!vehicleIdToIndex.ContainsKey(vehicle.Id))
                    continue;

                int offspringVehicleIndex = vehicleIdToIndex[vehicle.Id];

                // Copy passengers from this vehicle
                foreach (var passenger in vehicle.Passengers)
                {
                    if (!assignedPassengerIds.Contains(passenger.Id) &&
                        offspring.Vehicles[offspringVehicleIndex].HasCapacity(1))
                    {
                        offspring.Vehicles[offspringVehicleIndex].Passengers.Add(passenger.Clone());
                        assignedPassengerIds.Add(passenger.Id);
                    }
                }
            }

            // Evaluate the offspring
            offspring.Score = Evaluate(offspring);
            return offspring;
        }

        // Mutates a solution by applying one of several mutation strategies
        private void Mutate(Solution solution)
        {
            // Choose a random mutation operation
            int mutationType = _random.Next(4);

            switch (mutationType)
            {
                case 0:
                    SwapPassengers(solution);
                    break;
                case 1:
                    ReorderPassengers(solution);
                    break;
                case 2:
                    MovePassenger(solution);
                    break;
                case 3:
                    OptimizeRoutes(solution);
                    break;
            }

            // Re-evaluate the solution after mutation
            solution.Score = Evaluate(solution);
        }

        // Swaps passengers between two vehicles
        private void SwapPassengers(Solution solution)
        {
            // Get vehicles with passengers
            var vehiclesWithPassengers = solution.Vehicles
                .Where(v => v.Passengers.Count > 0)
                .ToList();

            if (vehiclesWithPassengers.Count < 2)
                return;

            // Select two random vehicles
            int firstIndex = _random.Next(vehiclesWithPassengers.Count);
            int secondIndex;
            do
            {
                secondIndex = _random.Next(vehiclesWithPassengers.Count);
            } while (secondIndex == firstIndex);

            Vehicle vehicle1 = vehiclesWithPassengers[firstIndex];
            Vehicle vehicle2 = vehiclesWithPassengers[secondIndex];

            if (vehicle1.Passengers.Count == 0 || vehicle2.Passengers.Count == 0)
                return;

            // Select random passengers to swap
            int passenger1Index = _random.Next(vehicle1.Passengers.Count);
            int passenger2Index = _random.Next(vehicle2.Passengers.Count);

            // Swap the passengers
            Passenger temp = vehicle1.Passengers[passenger1Index];
            vehicle1.Passengers[passenger1Index] = vehicle2.Passengers[passenger2Index];
            vehicle2.Passengers[passenger2Index] = temp;
        }

        // Reorders passengers within a vehicle using a simple local optimization
        private void ReorderPassengers(Solution solution)
        {
            // Get vehicles with multiple passengers
            var vehiclesWithMultiplePassengers = solution.Vehicles
                .Where(v => v.Passengers.Count > 1)
                .ToList();

            if (vehiclesWithMultiplePassengers.Count == 0)
                return;

            // Select a random vehicle
            Vehicle vehicle = vehiclesWithMultiplePassengers[_random.Next(vehiclesWithMultiplePassengers.Count)];

            // Simple 2-opt local search
            for (int i = 0; i < vehicle.Passengers.Count - 1; i++)
            {
                for (int j = i + 1; j < vehicle.Passengers.Count; j++)
                {
                    // Try swapping passengers i and j
                    Passenger temp = vehicle.Passengers[i];
                    vehicle.Passengers[i] = vehicle.Passengers[j];
                    vehicle.Passengers[j] = temp;

                    // Calculate new metrics
                    var newMetrics = CalculateRouteMetrics(vehicle);
                    double newDistance = newMetrics.TotalDistance;

                    // Swap back if the distance increased
                    temp = vehicle.Passengers[i];
                    vehicle.Passengers[i] = vehicle.Passengers[j];
                    vehicle.Passengers[j] = temp;

                    // Calculate original metrics
                    var origMetrics = CalculateRouteMetrics(vehicle);
                    double origDistance = origMetrics.TotalDistance;

                    // If the new distance is shorter, keep the swap
                    if (newDistance < origDistance)
                    {
                        temp = vehicle.Passengers[i];
                        vehicle.Passengers[i] = vehicle.Passengers[j];
                        vehicle.Passengers[j] = temp;
                    }
                }
            }
        }

        // Moves a passenger from one vehicle to another
        private void MovePassenger(Solution solution)
        {
            // Get vehicles with passengers
            var vehiclesWithPassengers = solution.Vehicles
                .Where(v => v.Passengers.Count > 0)
                .ToList();

            if (vehiclesWithPassengers.Count == 0)
                return;

            // Get vehicles with capacity
            var vehiclesWithCapacity = solution.Vehicles
                .Where(v => v.IsAvailable && v.Passengers.Count < v.Capacity)
                .ToList();

            if (vehiclesWithCapacity.Count == 0)
                return;

            // Select a random vehicle to take a passenger from
            Vehicle sourceVehicle = vehiclesWithPassengers[_random.Next(vehiclesWithPassengers.Count)];

            // Select a random passenger to move
            int passengerIndex = _random.Next(sourceVehicle.Passengers.Count);
            Passenger passenger = sourceVehicle.Passengers[passengerIndex];

            // Select a random vehicle to move to
            Vehicle targetVehicle = vehiclesWithCapacity[_random.Next(vehiclesWithCapacity.Count)];

            // Move the passenger
            sourceVehicle.Passengers.RemoveAt(passengerIndex);
            targetVehicle.Passengers.Add(passenger);
        }

        // Optimizes routes using a 2-opt local search on a random vehicle
        private void OptimizeRoutes(Solution solution)
        {
            // Get vehicles with enough passengers to optimize
            var vehiclesToOptimize = solution.Vehicles
                .Where(v => v.Passengers.Count >= 3)
                .ToList();

            if (vehiclesToOptimize.Count == 0)
                return;

            // Select a random vehicle
            Vehicle vehicle = vehiclesToOptimize[_random.Next(vehiclesToOptimize.Count)];

            // 2-opt algorithm for TSP
            bool improved = true;
            while (improved)
            {
                improved = false;
                for (int i = 0; i < vehicle.Passengers.Count - 1; i++)
                {
                    for (int j = i + 1; j < vehicle.Passengers.Count; j++)
                    {
                        // Calculate current distance
                        var currentMetrics = CalculateRouteMetrics(vehicle);
                        double currentDistance = currentMetrics.TotalDistance;

                        // Reverse the segment between i and j
                        vehicle.Passengers.Reverse(i, j - i + 1);

                        // Calculate new distance
                        var newMetrics = CalculateRouteMetrics(vehicle);
                        double newDistance = newMetrics.TotalDistance;

                        if (newDistance < currentDistance)
                        {
                            // Keep the change
                            improved = true;
                        }
                        else
                        {
                            // Revert the change
                            vehicle.Passengers.Reverse(i, j - i + 1);
                        }
                    }
                }
            }
        }

        // Gets the best solution from the current population
        private Solution GetBestSolution()
        {
            if (_population.Count == 0)
                return new Solution();

            return _population.OrderByDescending(s => s.Score).First();
        }

        // Calculates exact metrics for a solution after optimization
        private void CalculateExactMetrics(Solution solution)
        {
            foreach (var vehicle in solution.Vehicles)
            {
                if (vehicle.Passengers.Count > 0)
                {
                    var metrics = CalculateRouteMetrics(vehicle);
                    vehicle.TotalDistance = metrics.TotalDistance;
                    vehicle.TotalTime = metrics.TotalTime;
                }
                else
                {
                    vehicle.TotalDistance = 0;
                    vehicle.TotalTime = 0;
                }
            }
        }
    }
}