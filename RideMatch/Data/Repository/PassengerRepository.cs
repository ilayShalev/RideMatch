using RideMatch.Core.Models;
using RideMatch.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    /// <summary>
    /// Repository for passenger-related data access
    /// </summary>
    public class PassengerRepository
    {
        private readonly RideMatchDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the PassengerRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public PassengerRepository(RideMatchDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Adds a new passenger
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="name">The passenger's name</param>
        /// <param name="latitude">The latitude</param>
        /// <param name="longitude">The longitude</param>
        /// <param name="address">The address</param>
        /// <returns>The new passenger ID</returns>
        public async Task<int> AddPassengerAsync(int userId, string name, double latitude, double longitude, string address)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        // Check if a passenger already exists for this user
                        command.CommandText = "SELECT COUNT(*) FROM Passengers WHERE UserId = @UserId";
                        command.Parameters.AddWithValue("@UserId", userId);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        if (count > 0)
                            throw new Exception("Passenger already exists for this user");

                        // Clear parameters for the next query
                        command.Parameters.Clear();

                        // Insert passenger
                        command.CommandText = @"
                            INSERT INTO Passengers (UserId, Name, Latitude, Longitude, Address, IsAvailable, CreatedAt, UpdatedAt)
                            VALUES (@UserId, @Name, @Latitude, @Longitude, @Address, 0, @CreatedAt, @UpdatedAt);
                            SELECT last_insert_rowid();";

                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Latitude", latitude);
                        command.Parameters.AddWithValue("@Longitude", longitude);
                        command.Parameters.AddWithValue("@Address", address ?? "");
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        // Execute and get the new passenger ID
                        object result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error adding passenger: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Updates a passenger
        /// </summary>
        /// <param name="passengerId">The passenger ID</param>
        /// <param name="name">The passenger's name</param>
        /// <param name="latitude">The latitude</param>
        /// <param name="longitude">The longitude</param>
        /// <param name="address">The address</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdatePassengerAsync(int passengerId, string name, double latitude, double longitude, string address)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        command.CommandText = @"
                            UPDATE Passengers 
                            SET Name = @Name, Latitude = @Latitude, Longitude = @Longitude, Address = @Address, UpdatedAt = @UpdatedAt
                            WHERE Id = @PassengerId";

                        command.Parameters.AddWithValue("@PassengerId", passengerId);
                        command.Parameters.AddWithValue("@Name", name);
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
                        Console.WriteLine($"Error updating passenger: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Updates a passenger's availability
        /// </summary>
        /// <param name="passengerId">The passenger ID</param>
        /// <param name="isAvailable">The availability status</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdatePassengerAvailabilityAsync(int passengerId, bool isAvailable)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        command.CommandText = @"
                            UPDATE Passengers 
                            SET IsAvailable = @IsAvailable, UpdatedAt = @UpdatedAt
                            WHERE Id = @PassengerId";

                        command.Parameters.AddWithValue("@PassengerId", passengerId);
                        command.Parameters.AddWithValue("@IsAvailable", isAvailable ? 1 : 0);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error updating passenger availability: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Gets available passengers
        /// </summary>
        /// <returns>List of available passengers</returns>
        public async Task<List<Passenger>> GetAvailablePassengersAsync()
        {
            return await Task.Run(() =>
            {
                var passengers = new List<Passenger>();

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT Id, UserId, Name, Latitude, Longitude, Address, IsAvailable
                        FROM Passengers
                        WHERE IsAvailable = 1
                        ORDER BY Name";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            passengers.Add(new Passenger
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Name = reader["Name"].ToString(),
                                Latitude = Convert.ToDouble(reader["Latitude"]),
                                Longitude = Convert.ToDouble(reader["Longitude"]),
                                Address = reader["Address"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                            });
                        }
                    }
                }

                return passengers;
            });
        }

        /// <summary>
        /// Gets all passengers
        /// </summary>
        /// <returns>List of all passengers</returns>
        public async Task<List<Passenger>> GetAllPassengersAsync()
        {
            return await Task.Run(() =>
            {
                var passengers = new List<Passenger>();

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT Id, UserId, Name, Latitude, Longitude, Address, IsAvailable
                        FROM Passengers
                        ORDER BY Name";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            passengers.Add(new Passenger
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Name = reader["Name"].ToString(),
                                Latitude = Convert.ToDouble(reader["Latitude"]),
                                Longitude = Convert.ToDouble(reader["Longitude"]),
                                Address = reader["Address"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                            });
                        }
                    }
                }

                return passengers;
            });
        }

        /// <summary>
        /// Gets a passenger by user ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The passenger, or null if not found</returns>
        public async Task<Passenger> GetPassengerByUserIdAsync(int userId)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT Id, UserId, Name, Latitude, Longitude, Address, IsAvailable
                        FROM Passengers
                        WHERE UserId = @UserId";

                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Passenger
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Name = reader["Name"].ToString(),
                                Latitude = Convert.ToDouble(reader["Latitude"]),
                                Longitude = Convert.ToDouble(reader["Longitude"]),
                                Address = reader["Address"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                            };
                        }
                    }
                }

                return null;
            });
        }

        /// <summary>
        /// Gets a passenger by ID
        /// </summary>
        /// <param name="passengerId">The passenger ID</param>
        /// <returns>The passenger, or null if not found</returns>
        public async Task<Passenger> GetPassengerByIdAsync(int passengerId)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT Id, UserId, Name, Latitude, Longitude, Address, IsAvailable
                        FROM Passengers
                        WHERE Id = @PassengerId";

                    command.Parameters.AddWithValue("@PassengerId", passengerId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Passenger
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Name = reader["Name"].ToString(),
                                Latitude = Convert.ToDouble(reader["Latitude"]),
                                Longitude = Convert.ToDouble(reader["Longitude"]),
                                Address = reader["Address"].ToString(),
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                            };
                        }
                    }
                }

                return null;
            });
        }

        /// <summary>
        /// Deletes a passenger
        /// </summary>
        /// <param name="passengerId">The passenger ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DeletePassengerAsync(int passengerId)
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

                            // Delete route passenger assignments
                            command.CommandText = "DELETE FROM RoutePassengers WHERE PassengerId = @PassengerId";
                            command.Parameters.AddWithValue("@PassengerId", passengerId);
                            command.ExecuteNonQuery();

                            // Delete passenger
                            command.CommandText = "DELETE FROM Passengers WHERE Id = @PassengerId";
                            int rowsAffected = command.ExecuteNonQuery();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error deleting passenger: {ex.Message}");
                        transaction.Rollback();
                        return false;
                    }
                }
            });
        }
    }
}