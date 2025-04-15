using RideMatch.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    /// <summary>
    /// Repository for settings-related data access
    /// </summary>
    public class SettingsRepository
    {
        private readonly RideMatchDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the SettingsRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public SettingsRepository(RideMatchDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Gets the destination information
        /// </summary>
        /// <returns>The destination information</returns>
        public async Task<(int Id, string Name, double Latitude, double Longitude, string Address, string TargetTime)> GetDestinationAsync()
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT Id, Name, Latitude, Longitude, Address, TargetTime
                        FROM Destinations
                        ORDER BY Id
                        LIMIT 1";

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (
                                Convert.ToInt32(reader["Id"]),
                                reader["Name"].ToString(),
                                Convert.ToDouble(reader["Latitude"]),
                                Convert.ToDouble(reader["Longitude"]),
                                reader["Address"].ToString(),
                                reader["TargetTime"].ToString()
                            );
                        }
                    }
                }

                // Return default destination if none exists
                return (0, "Default Destination", 32.0853, 34.7818, "30 Rothschild Blvd, Tel Aviv", "09:00");
            });
        }

        /// <summary>
        /// Updates the destination information
        /// </summary>
        /// <param name="name">The destination name</param>
        /// <param name="latitude">The destination latitude</param>
        /// <param name="longitude">The destination longitude</param>
        /// <param name="targetTime">The target arrival time</param>
        /// <param name="address">The destination address</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateDestinationAsync(string name, double latitude, double longitude, string targetTime, string address)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        // Check if destination exists
                        command.CommandText = "SELECT COUNT(*) FROM Destinations";
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        if (count > 0)
                        {
                            // Update existing destination
                            command.CommandText = @"
                                UPDATE Destinations 
                                SET Name = @Name, Latitude = @Latitude, Longitude = @Longitude, 
                                    Address = @Address, TargetTime = @TargetTime, UpdatedAt = @UpdatedAt
                                WHERE Id = (SELECT Id FROM Destinations ORDER BY Id LIMIT 1)";
                        }
                        else
                        {
                            // Insert new destination
                            command.CommandText = @"
                                INSERT INTO Destinations (Name, Latitude, Longitude, Address, TargetTime, CreatedAt, UpdatedAt)
                                VALUES (@Name, @Latitude, @Longitude, @Address, @TargetTime, @CreatedAt, @UpdatedAt)";
                        }

                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Latitude", latitude);
                        command.Parameters.AddWithValue("@Longitude", longitude);
                        command.Parameters.AddWithValue("@Address", address ?? "");
                        command.Parameters.AddWithValue("@TargetTime", targetTime ?? "09:00");
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error updating destination: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Saves scheduling settings
        /// </summary>
        /// <param name="isEnabled">Whether scheduling is enabled</param>
        /// <param name="scheduledTime">The scheduled time</param>
        /// <returns>True if successful</returns>
        public async Task<bool> SaveSchedulingSettingsAsync(bool isEnabled, DateTime scheduledTime)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        // Check if settings exist
                        command.CommandText = "SELECT COUNT(*) FROM SchedulingSettings";
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        if (count > 0)
                        {
                            // Update existing settings
                            command.CommandText = @"
                                UPDATE SchedulingSettings 
                                SET IsEnabled = @IsEnabled, ScheduledTime = @ScheduledTime, UpdatedAt = @UpdatedAt
                                WHERE Id = (SELECT Id FROM SchedulingSettings ORDER BY Id LIMIT 1)";
                        }
                        else
                        {
                            // Insert new settings
                            command.CommandText = @"
                                INSERT INTO SchedulingSettings (IsEnabled, ScheduledTime, UpdatedAt)
                                VALUES (@IsEnabled, @ScheduledTime, @UpdatedAt)";
                        }

                        command.Parameters.AddWithValue("@IsEnabled", isEnabled ? 1 : 0);
                        command.Parameters.AddWithValue("@ScheduledTime", scheduledTime.ToString("HH:mm"));
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error saving scheduling settings: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Gets scheduling settings
        /// </summary>
        /// <returns>The scheduling settings</returns>
        public async Task<(bool IsEnabled, DateTime ScheduledTime)> GetSchedulingSettingsAsync()
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT IsEnabled, ScheduledTime
                        FROM SchedulingSettings
                        ORDER BY Id
                        LIMIT 1";

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isEnabled = Convert.ToBoolean(reader["IsEnabled"]);
                            string scheduledTimeStr = reader["ScheduledTime"].ToString();

                            // Parse scheduled time
                            if (TimeSpan.TryParse(scheduledTimeStr, out TimeSpan scheduledTime))
                            {
                                DateTime scheduledDateTime = DateTime.Today.Add(scheduledTime);
                                return (isEnabled, scheduledDateTime);
                            }
                        }
                    }
                }

                // Return default settings if none exist
                return (false, DateTime.Today.AddHours(7)); // Default: 7:00 AM
            });
        }

        /// <summary>
        /// Gets the scheduling log entries
        /// </summary>
        /// <returns>The scheduling log entries</returns>
        public async Task<List<(DateTime RunTime, string Status, int RoutesGenerated, int PassengersAssigned)>> GetSchedulingLogAsync()
        {
            return await Task.Run(() =>
            {
                var logEntries = new List<(DateTime RunTime, string Status, int RoutesGenerated, int PassengersAssigned)>();

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT RunTime, Status, RoutesGenerated, PassengersAssigned
                        FROM SchedulingLog
                        ORDER BY RunTime DESC
                        LIMIT 50";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime runTime = DateTime.Parse(reader["RunTime"].ToString());
                            string status = reader["Status"].ToString();
                            int routesGenerated = Convert.ToInt32(reader["RoutesGenerated"]);
                            int passengersAssigned = Convert.ToInt32(reader["PassengersAssigned"]);

                            logEntries.Add((runTime, status, routesGenerated, passengersAssigned));
                        }
                    }
                }

                return logEntries;
            });
        }

        /// <summary>
        /// Logs a scheduling run
        /// </summary>
        /// <param name="runTime">The run time</param>
        /// <param name="status">The status</param>
        /// <param name="routesGenerated">The number of routes generated</param>
        /// <param name="passengersAssigned">The number of passengers assigned</param>
        /// <param name="errorMessage">The error message (if any)</param>
        /// <returns>True if successful</returns>
        public async Task<bool> LogSchedulingRunAsync(DateTime runTime, string status, int routesGenerated, int passengersAssigned, string errorMessage = null)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        command.CommandText = @"
                            INSERT INTO SchedulingLog (RunTime, Status, RoutesGenerated, PassengersAssigned, ErrorMessage)
                            VALUES (@RunTime, @Status, @RoutesGenerated, @PassengersAssigned, @ErrorMessage)";

                        command.Parameters.AddWithValue("@RunTime", runTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@RoutesGenerated", routesGenerated);
                        command.Parameters.AddWithValue("@PassengersAssigned", passengersAssigned);
                        command.Parameters.AddWithValue("@ErrorMessage", errorMessage ?? "");

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error logging scheduling run: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Saves a general setting
        /// </summary>
        /// <param name="settingName">The setting name</param>
        /// <param name="settingValue">The setting value</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> SaveSettingAsync(string settingName, string settingValue)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        // Check if setting exists
                        command.CommandText = "SELECT COUNT(*) FROM Settings WHERE Key = @Key";
                        command.Parameters.AddWithValue("@Key", settingName);
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        command.Parameters.Clear();

                        if (count > 0)
                        {
                            // Update existing setting
                            command.CommandText = @"
                                UPDATE Settings 
                                SET Value = @Value, UpdatedAt = @UpdatedAt
                                WHERE Key = @Key";
                        }
                        else
                        {
                            // Insert new setting
                            command.CommandText = @"
                                INSERT INTO Settings (Key, Value, UpdatedAt)
                                VALUES (@Key, @Value, @UpdatedAt)";
                        }

                        command.Parameters.AddWithValue("@Key", settingName);
                        command.Parameters.AddWithValue("@Value", settingValue);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error saving setting: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Gets a general setting
        /// </summary>
        /// <param name="settingName">The setting name</param>
        /// <param name="defaultValue">The default value to return if the setting doesn't exist</param>
        /// <returns>The setting value</returns>
        public async Task<string> GetSettingAsync(string settingName, string defaultValue = "")
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT Value FROM Settings WHERE Key = @Key";
                    command.Parameters.AddWithValue("@Key", settingName);

                    object result = command.ExecuteScalar();
                    return result != null ? result.ToString() : defaultValue;
                }
            });
        }
    }
}