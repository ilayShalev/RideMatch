using RideMatch.Core.Models;
using RideMatch.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    /// <summary>
    /// Repository for vehicle-related data access
    /// </summary>
    public class VehicleRepository
    {
        private readonly RideMatchDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the VehicleRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public VehicleRepository(RideMatchDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Adds a new vehicle
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="capacity">The vehicle capacity</param>
        /// <param name="startLatitude">The starting latitude</param>
        /// <param name="startLongitude">The starting longitude</param>
        /// <param name="startAddress">The starting address</param>
        /// <returns>The new vehicle ID</returns>
        public async Task<int> AddVehicleAsync(int userId, int capacity, double startLatitude, double startLongitude, string startAddress)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        // Check if a vehicle already exists for this user
                        command.CommandText = "SELECT COUNT(*) FROM Vehicles WHERE UserId = @UserId";
                        command.Parameters.AddWithValue("@UserId", userId);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        if (count > 0)
                            throw new Exception("Vehicle already exists for this user");

                        // Clear parameters for the next query
                        command.Parameters.Clear();

                        // Insert vehicle
                        command.CommandText = @"
                            INSERT INTO Vehicles (UserId, Capacity, Latitude, Longitude, Address, IsAvailable, CreatedAt, UpdatedAt)
                            VALUES (@UserId, @Capacity, @Latitude, @Longitude, @Address, 0, @CreatedAt, @UpdatedAt);
                            SELECT last_insert_rowid();";

                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@Capacity", capacity);
                        command.Parameters.AddWithValue("@Latitude", startLatitude);
                        command.Parameters.AddWithValue("@Longitude", startLongitude);
                        command.Parameters.AddWithValue("@Address", startAddress ?? "");
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        // Execute and get the new vehicle ID
                        object result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error adding vehicle: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Updates a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="capacity">The vehicle capacity</param>
        /// <param name="startLatitude">The starting latitude</param>
        /// <param name="startLongitude">The starting longitude</param>
        /// <param name="startAddress">The starting address</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateVehicleAsync(int vehicleId, int capacity, double startLatitude, double startLongitude, string startAddress)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        command.CommandText = @"
                            UPDATE Vehicles 
                            SET Capacity = @Capacity, Latitude = @Latitude, Longitude = @Longitude, Address = @Address, UpdatedAt = @UpdatedAt
                            WHERE Id = @VehicleId";

                        command.Parameters.AddWithValue("@VehicleId", vehicleId);
                        command.Parameters.AddWithValue("@Capacity", capacity);
                        command.Parameters.AddWithValue("@Latitude", startLatitude);
                        command.Parameters.AddWithValue("@Longitude", startLongitude);
                        command.Parameters.AddWithValue("@Address", startAddress ?? "");
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error updating vehicle: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Updates a vehicle's availability
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="isAvailable">The availability status</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateVehicleAvailabilityAsync(int vehicleId, bool isAvailable)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        command.CommandText = @"
                            UPDATE Vehicles 
                            SET IsAvailable = @IsAvailable, UpdatedAt = @UpdatedAt
                            WHERE Id = @VehicleId";

                        command.Parameters.AddWithValue("@VehicleId", vehicleId);
                        command.Parameters.AddWithValue("@IsAvailable", isAvailable ? 1 : 0);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error updating vehicle availability: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Gets all vehicles
        /// </summary>
        /// <returns>List of all vehicles</returns>
        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            return await Task.Run(() =>
            {
                var vehicles = new List<Vehicle>();

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT v.Id, v.UserId, u.Name AS DriverName, v.Capacity, v.Latitude, v.Longitude, v.Address, v.IsAvailable
                        FROM Vehicles v
                        JOIN Users u ON v.UserId = u.Id
                        ORDER BY v.Id";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vehicles.Add(new Vehicle
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
                            });
                        }
                    }
                }

                return vehicles;
            });
        }

        /// <summary>
        /// Gets available vehicles
        /// </summary>
        /// <returns>List of available vehicles</returns>
        public async Task<List<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await Task.Run(() =>
            {
                var vehicles = new List<Vehicle>();

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT v.Id, v.UserId, u.Name AS DriverName, v.Capacity, v.Latitude, v.Longitude, v.Address, v.IsAvailable
                        FROM Vehicles v
                        JOIN Users u ON v.UserId = u.Id
                        WHERE v.IsAvailable = 1
                        ORDER BY v.Id";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vehicles.Add(new Vehicle
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
                            });
                        }
                    }
                }

                return vehicles;
            });
        }

        /// <summary>
        /// Gets a vehicle by user ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The vehicle, or null if not found</returns>
        public async Task<Vehicle> GetVehicleByUserIdAsync(int userId)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
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
                            return new Vehicle
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
                }

                return null;
            });
        }

        /// <summary>
        /// Gets a vehicle by ID
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The vehicle, or null if not found</returns>
        public async Task<Vehicle> GetVehicleByIdAsync(int vehicleId)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT v.Id, v.UserId, u.Name AS DriverName, v.Capacity, v.Latitude, v.Longitude, v.Address, v.IsAvailable
                        FROM Vehicles v
                        JOIN Users u ON v.UserId = u.Id
                        WHERE v.Id = @VehicleId";

                    command.Parameters.AddWithValue("@VehicleId", vehicleId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Vehicle
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
                }

                return null;
            });
        }

        /// <summary>
        /// Saves or updates a driver's vehicle
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="capacity">The vehicle capacity</param>
        /// <param name="startLatitude">The starting latitude</param>
        /// <param name="startLongitude">The starting longitude</param>
        /// <param name="startAddress">The starting address</param>
        /// <returns>The vehicle ID</returns>
        public async Task<int> SaveDriverVehicleAsync(int userId, int capacity, double startLatitude, double startLongitude, string startAddress)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        // Check if vehicle exists for this user
                        command.CommandText = "SELECT Id FROM Vehicles WHERE UserId = @UserId";
                        command.Parameters.AddWithValue("@UserId", userId);
                        object existingId = command.ExecuteScalar();

                        if (existingId != null)
                        {
                            // Update existing vehicle
                            int vehicleId = Convert.ToInt32(existingId);
                            command.Parameters.Clear();

                            command.CommandText = @"
                                UPDATE Vehicles 
                                SET Capacity = @Capacity, Latitude = @Latitude, Longitude = @Longitude, Address = @Address, UpdatedAt = @UpdatedAt
                                WHERE Id = @VehicleId";

                            command.Parameters.AddWithValue("@VehicleId", vehicleId);
                            command.Parameters.AddWithValue("@Capacity", capacity);
                            command.Parameters.AddWithValue("@Latitude", startLatitude);
                            command.Parameters.AddWithValue("@Longitude", startLongitude);
                            command.Parameters.AddWithValue("@Address", startAddress ?? "");
                            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            command.ExecuteNonQuery();
                            return vehicleId;
                        }
                        else
                        {
                            // Create new vehicle
                            command.Parameters.Clear();

                            command.CommandText = @"
                                INSERT INTO Vehicles (UserId, Capacity, Latitude, Longitude, Address, IsAvailable, CreatedAt, UpdatedAt)
                                VALUES (@UserId, @Capacity, @Latitude, @Longitude, @Address, 0, @CreatedAt, @UpdatedAt);
                                SELECT last_insert_rowid();";

                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@Capacity", capacity);
                            command.Parameters.AddWithValue("@Latitude", startLatitude);
                            command.Parameters.AddWithValue("@Longitude", startLongitude);
                            command.Parameters.AddWithValue("@Address", startAddress ?? "");
                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            // Execute and get the new vehicle ID
                            object result = command.ExecuteScalar();
                            return Convert.ToInt32(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error saving driver vehicle: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Updates a vehicle's capacity
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="capacity">The new capacity</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateVehicleCapacityAsync(int vehicleId, int capacity)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        command.CommandText = @"
                            UPDATE Vehicles 
                            SET Capacity = @Capacity, UpdatedAt = @UpdatedAt
                            WHERE Id = @VehicleId";

                        command.Parameters.AddWithValue("@VehicleId", vehicleId);
                        command.Parameters.AddWithValue("@Capacity", capacity);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error updating vehicle capacity: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Updates a vehicle's location
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="latitude">The new latitude</param>
        /// <param name="longitude">The new longitude</param>
        /// <param name="address">The new address</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateVehicleLocationAsync(int vehicleId, double latitude, double longitude, string address)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        command.CommandText = @"
                            UPDATE Vehicles 
                            SET Latitude = @Latitude, Longitude = @Longitude, Address = @Address, UpdatedAt = @UpdatedAt
                            WHERE Id = @VehicleId";

                        command.Parameters.AddWithValue("@VehicleId", vehicleId);
                        command.Parameters.AddWithValue("@Latitude", latitude);
                        command.Parameters.AddWithValue("@Longitude", longitude);
                        command.Parameters.AddWithValue("@Address", address ?? "");
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error updating vehicle location: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Deletes a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DeleteVehicleAsync(int vehicleId)
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

                            // Delete related route details
                            command.CommandText = @"
                                DELETE FROM RoutePassengers
                                WHERE RouteDetailId IN (SELECT Id FROM RouteDetails WHERE VehicleId = @VehicleId)";
                            command.Parameters.AddWithValue("@VehicleId", vehicleId);
                            command.ExecuteNonQuery();

                            // Delete route details
                            command.CommandText = "DELETE FROM RouteDetails WHERE VehicleId = @VehicleId";
                            command.ExecuteNonQuery();

                            // Delete vehicle
                            command.CommandText = "DELETE FROM Vehicles WHERE Id = @VehicleId";
                            int rowsAffected = command.ExecuteNonQuery();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error deleting vehicle: {ex.Message}");
                        transaction.Rollback();
                        return false;
                    }
                }
            });
        }
    }
}