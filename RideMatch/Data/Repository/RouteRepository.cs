using RideMatch.Core.Models;
using RideMatch.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    /// <summary>
    /// Repository for route-related data access
    /// </summary>
    public class RouteRepository
    {
        private readonly RideMatchDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the RouteRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public RouteRepository(RideMatchDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Saves a solution to the database
        /// </summary>
        /// <param name="solution">The solution to save</param>
        /// <param name="date">The date for the routes</param>
        /// <returns>The route ID</returns>
        public async Task<int> SaveSolutionAsync(Solution solution, string date)
        {
            return await Task.Run(() =>
            {
                int routeId = 0;

                using (var connection = _dbContext.GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SQLiteCommand(connection))
                        {
                            command.Transaction = transaction;

                            // Insert route
                            command.CommandText = @"
                                INSERT INTO Routes (Date, GeneratedAt)
                                VALUES (@Date, @GeneratedAt);
                                SELECT last_insert_rowid();";

                            command.Parameters.AddWithValue("@Date", date);
                            command.Parameters.AddWithValue("@GeneratedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            // Get the route ID
                            routeId = Convert.ToInt32(command.ExecuteScalar());

                            // Save each vehicle route
                            foreach (var vehicle in solution.Vehicles)
                            {
                                // Skip vehicles with no passengers
                                if (vehicle.Passengers.Count == 0)
                                    continue;

                                // Insert route detail
                                command.Parameters.Clear();
                                command.CommandText = @"
                                    INSERT INTO RouteDetails (RouteId, VehicleId, TotalDistance, TotalTime, CreatedAt)
                                    VALUES (@RouteId, @VehicleId, @TotalDistance, @TotalTime, @CreatedAt);
                                    SELECT last_insert_rowid();";

                                command.Parameters.AddWithValue("@RouteId", routeId);
                                command.Parameters.AddWithValue("@VehicleId", vehicle.Id);
                                command.Parameters.AddWithValue("@TotalDistance", vehicle.TotalDistance);
                                command.Parameters.AddWithValue("@TotalTime", vehicle.TotalTime);
                                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                // Get the route detail ID
                                int routeDetailId = Convert.ToInt32(command.ExecuteScalar());

                                // Insert route passengers
                                for (int i = 0; i < vehicle.Passengers.Count; i++)
                                {
                                    var passenger = vehicle.Passengers[i];

                                    command.Parameters.Clear();
                                    command.CommandText = @"
                                        INSERT INTO RoutePassengers (RouteDetailId, PassengerId, StopOrder, PickupTime)
                                        VALUES (@RouteDetailId, @PassengerId, @StopOrder, @PickupTime);";

                                    command.Parameters.AddWithValue("@RouteDetailId", routeDetailId);
                                    command.Parameters.AddWithValue("@PassengerId", passenger.Id);
                                    command.Parameters.AddWithValue("@StopOrder", i + 1);
                                    command.Parameters.AddWithValue("@PickupTime", passenger.PickupTime?.ToString("HH:mm:ss") ?? null);

                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error saving solution: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }
                }

                return routeId;
            });
        }

        /// <summary>
        /// Gets driver's route and assigned passengers for a specific date
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="date">The date</param>
        /// <returns>The vehicle and assigned passengers</returns>
        public async Task<(Vehicle Vehicle, List<Passenger> Passengers, DateTime? PickupTime)> GetDriverRouteAsync(int userId, string date)
        {
            return await Task.Run(() =>
            {
                Vehicle vehicle = null;
                List<Passenger> passengers = new List<Passenger>();
                DateTime? pickupTime = null;

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    // First, get the vehicle
                    command.CommandText = @"
                        SELECT v.Id, v.UserId, u.Name AS DriverName, v.Capacity, v.Latitude, v.Longitude, v.Address, v.IsAvailable
                        FROM Vehicles v
                        JOIN Users u ON v.UserId = u.Id
                        WHERE v.UserId = @UserId";

                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            vehicle = new Vehicle
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                DriverName = reader["DriverName"].ToString(),
                                Capacity = Convert.ToInt32(reader["Capacity"]),
                                Latitude = Convert.ToDouble(reader["Latitude"]),
                                Longitude = Convert.ToDouble(reader["Longitude"]),
                                Address = reader["Address"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                                Passengers = new List<Passenger>()
                            };
                        }
                    }

                    // If no vehicle found, return early
                    if (vehicle == null)
                        return (null, null, null);

                    // Get the route detail and passengers
                    command.Parameters.Clear();
                    command.CommandText = @"
                        SELECT 
                            rd.Id AS RouteDetailId,
                            rd.TotalDistance,
                            rd.TotalTime,
                            p.Id AS PassengerId,
                            p.UserId,
                            p.Name,
                            p.Latitude,
                            p.Longitude,
                            p.Address,
                            p.IsAvailable,
                            rp.PickupTime,
                            rp.StopOrder
                        FROM Routes r
                        JOIN RouteDetails rd ON r.Id = rd.RouteId
                        JOIN RoutePassengers rp ON rd.Id = rp.RouteDetailId
                        JOIN Passengers p ON rp.PassengerId = p.Id
                        WHERE r.Date = @Date AND rd.VehicleId = @VehicleId
                        ORDER BY rp.StopOrder";

                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@VehicleId", vehicle.Id);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Update vehicle stats if first row
                            if (passengers.Count == 0)
                            {
                                vehicle.TotalDistance = Convert.ToDouble(reader["TotalDistance"]);
                                vehicle.TotalTime = Convert.ToDouble(reader["TotalTime"]);
                            }

                            // Get pickup time if available
                            string pickupTimeStr = reader["PickupTime"].ToString();
                            DateTime? passengerPickupTime = null;
                            if (!string.IsNullOrEmpty(pickupTimeStr))
                            {
                                // Parse pickup time
                                if (DateTime.TryParse(pickupTimeStr, out DateTime parsed))
                                {
                                    passengerPickupTime = DateTime.Today.Add(parsed.TimeOfDay);
                                }
                            }

                            // Create passenger
                            var passenger = new Passenger
                            {
                                Id = Convert.ToInt32(reader["PassengerId"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Name = reader["Name"].ToString(),
                                Latitude = Convert.ToDouble(reader["Latitude"]),
                                Longitude = Convert.ToDouble(reader["Longitude"]),
                                Address = reader["Address"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                                PickupTime = passengerPickupTime
                            };

                            passengers.Add(passenger);
                            vehicle.Passengers.Add(passenger);
                        }
                    }
                }

                return (vehicle, passengers, pickupTime);
            });
        }

        /// <summary>
        /// Gets the solution for a specific date
        /// </summary>
        /// <param name="date">The date</param>
        /// <returns>The solution</returns>
        public async Task<Solution> GetSolutionForDateAsync(string date)
        {
            return await Task.Run(() =>
            {
            var solution = new Solution();
            var vehiclesDict = new Dictionary<int, Vehicle>();

            using (var connection = _dbContext.GetConnection())
            {
                // First, get all vehicles used in the solution
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                            SELECT DISTINCT 
                                v.Id, v.UserId, u.Name AS DriverName, v.Capacity, v.Latitude, v.Longitude, v.Address, v.IsAvailable,
                                rd.TotalDistance, rd.TotalTime
                            FROM Routes r
                            JOIN RouteDetails rd ON r.Id = rd.RouteId
                            JOIN Vehicles v ON rd.VehicleId = v.Id
                            JOIN Users u ON v.UserId = u.Id
                            WHERE r.Date = @Date";

                    command.Parameters.AddWithValue("@Date", date);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var vehicle = new Vehicle
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                DriverName = reader["DriverName"].ToString(),
                                Capacity = Convert.ToInt32(reader["Capacity"]),
                                Latitude = Convert.ToDouble(reader["Latitude"]),
                                Longitude = Convert.ToDouble(reader["Longitude"]),
                                Address = reader["Address"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                                Passengers = new List<Passenger>(),
                                TotalDistance = Convert.ToDouble(reader["TotalDistance"]),
                                TotalTime = Convert.ToDouble(reader["TotalTime"])
                            };

                                vehiclesDict[vehicle.Id] = vehicle;
                                solution.Vehicles.Add(vehicle);
                            }
                        }
                    }

                    // Now get all passengers assigned to these vehicles
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = @"
                            SELECT 
                                rd.VehicleId,
                                p.Id AS PassengerId,
                                p.UserId,
                                p.Name,
                                p.Latitude,
                                p.Longitude,
                                p.Address,
                                p.IsAvailable,
                                rp.PickupTime,
                                rp.StopOrder
                            FROM Routes r
                            JOIN RouteDetails rd ON r.Id = rd.RouteId
                            JOIN RoutePassengers rp ON rd.Id = rp.RouteDetailId
                            JOIN Passengers p ON rp.PassengerId = p.Id
                            WHERE r.Date = @Date
                            ORDER BY rd.VehicleId, rp.StopOrder";

                        command.Parameters.AddWithValue("@Date", date);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int vehicleId = Convert.ToInt32(reader["VehicleId"]);

                                // Skip if vehicle not found (shouldn't happen, but just in case)
                                if (!vehiclesDict.ContainsKey(vehicleId))
                                    continue;

                                // Parse pickup time if available
                                string pickupTimeStr = reader["PickupTime"].ToString();
                                DateTime? pickupTime = null;
                                if (!string.IsNullOrEmpty(pickupTimeStr))
                                {
                                    if (DateTime.TryParse(pickupTimeStr, out DateTime parsed))
                                    {
                                        pickupTime = DateTime.Today.Add(parsed.TimeOfDay);
                                    }
                                }

                                // Create passenger
                                var passenger = new Passenger
                                {
                                    Id = Convert.ToInt32(reader["PassengerId"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Name = reader["Name"].ToString(),
                                    Latitude = Convert.ToDouble(reader["Latitude"]),
                                    Longitude = Convert.ToDouble(reader["Longitude"]),
                                    Address = reader["Address"].ToString(),
                                    IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                                    PickupTime = pickupTime
                                };

                                // Add passenger to vehicle
                                vehiclesDict[vehicleId].Passengers.Add(passenger);
                            }
                        }
                    }
                }

                return solution;
            });
        }

        /// <summary>
        /// Gets passenger assignment and vehicle for a specific date
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="date">The date</param>
        /// <returns>The assigned vehicle and pickup time</returns>
        public async Task<(Vehicle AssignedVehicle, DateTime? PickupTime)> GetPassengerAssignmentAsync(int userId, string date)
        {
            return await Task.Run(() =>
            {
                Vehicle assignedVehicle = null;
                DateTime? pickupTime = null;

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT 
                            v.Id AS VehicleId, 
                            v.UserId AS DriverUserId, 
                            u.Name AS DriverName, 
                            v.Capacity, 
                            v.Latitude, 
                            v.Longitude, 
                            v.Address,
                            v.IsAvailable,
                            rp.PickupTime
                        FROM Routes r
                        JOIN RouteDetails rd ON r.Id = rd.RouteId
                        JOIN RoutePassengers rp ON rd.Id = rp.RouteDetailId
                        JOIN Passengers p ON rp.PassengerId = p.Id
                        JOIN Vehicles v ON rd.VehicleId = v.Id
                        JOIN Users u ON v.UserId = u.Id
                        WHERE r.Date = @Date AND p.UserId = @UserId";

                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Create vehicle
                            assignedVehicle = new Vehicle
                            {
                                Id = Convert.ToInt32(reader["VehicleId"]),
                                UserId = Convert.ToInt32(reader["DriverUserId"]),
                                DriverName = reader["DriverName"].ToString(),
                                Capacity = Convert.ToInt32(reader["Capacity"]),
                                Latitude = Convert.ToDouble(reader["Latitude"]),
                                Longitude = Convert.ToDouble(reader["Longitude"]),
                                Address = reader["Address"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                                Passengers = new List<Passenger>()
                            };

                            // Get pickup time if available
                            string pickupTimeStr = reader["PickupTime"].ToString();
                            if (!string.IsNullOrEmpty(pickupTimeStr))
                            {
                                if (DateTime.TryParse(pickupTimeStr, out DateTime parsed))
                                {
                                    pickupTime = DateTime.Today.Add(parsed.TimeOfDay);
                                }
                            }
                        }
                    }
                }

                return (assignedVehicle, pickupTime);
            });
        }

        /// <summary>
        /// Updates the estimated pickup times for a route
        /// </summary>
        /// <param name="routeDetailId">The route detail ID</param>
        /// <param name="passengerPickupTimes">The pickup times for each passenger</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdatePickupTimesAsync(int routeDetailId, Dictionary<int, string> passengerPickupTimes)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SQLiteCommand(connection))
                        {
                            command.Transaction = transaction;

                            foreach (var item in passengerPickupTimes)
                            {
                                int passengerId = item.Key;
                                string pickupTime = item.Value;

                                command.CommandText = @"
                                    UPDATE RoutePassengers 
                                    SET PickupTime = @PickupTime
                                    WHERE RouteDetailId = @RouteDetailId AND PassengerId = @PassengerId";

                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@RouteDetailId", routeDetailId);
                                command.Parameters.AddWithValue("@PassengerId", passengerId);
                                command.Parameters.AddWithValue("@PickupTime", pickupTime);

                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error updating pickup times: {ex.Message}");
                        transaction.Rollback();
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Resets all availability flags for a new day
        /// </summary>
        public async Task ResetAvailabilityAsync()
        {
            await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SQLiteCommand(connection))
                        {
                            command.Transaction = transaction;

                            // Reset vehicle availability
                            command.CommandText = "UPDATE Vehicles SET IsAvailable = 0, UpdatedAt = @UpdatedAt";
                            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            command.ExecuteNonQuery();

                            // Reset passenger availability
                            command.CommandText = "UPDATE Passengers SET IsAvailable = 0, UpdatedAt = @UpdatedAt";
                            command.ExecuteNonQuery();

                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error resetting availability: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Gets route history
        /// </summary>
        /// <returns>List of route history entries</returns>
        public async Task<List<(int RouteId, DateTime GeneratedTime, int VehicleCount, int PassengerCount)>> GetRouteHistoryAsync()
        {
            return await Task.Run(() =>
            {
                var history = new List<(int RouteId, DateTime GeneratedTime, int VehicleCount, int PassengerCount)>();

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT 
                            r.Id AS RouteId, 
                            r.GeneratedAt, 
                            COUNT(DISTINCT rd.VehicleId) AS VehicleCount,
                            COUNT(DISTINCT rp.PassengerId) AS PassengerCount
                        FROM Routes r
                        LEFT JOIN RouteDetails rd ON r.Id = rd.RouteId
                        LEFT JOIN RoutePassengers rp ON rd.Id = rp.RouteDetailId
                        GROUP BY r.Id
                        ORDER BY r.GeneratedAt DESC
                        LIMIT 50";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int routeId = Convert.ToInt32(reader["RouteId"]);
                            DateTime generatedTime = DateTime.Parse(reader["GeneratedAt"].ToString());
                            int vehicleCount = Convert.ToInt32(reader["VehicleCount"]);
                            int passengerCount = Convert.ToInt32(reader["PassengerCount"]);

                            history.Add((routeId, generatedTime, vehicleCount, passengerCount));
                        }
                    }
                }

                return history;
            });
        }
    }
}