using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Data.DbContext
{
    /// <summary>
    /// Handles database initialization and sample data creation
    /// </summary>
    internal class DbInitializer
    {
        private readonly RideMatchDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the DbInitializer class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public DbInitializer(RideMatchDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Initializes the database with sample data if it's empty
        /// </summary>
        public void Initialize()
        {
            // Check if we need to seed the database with sample data
            if (IsDatabaseEmpty())
            {
                SeedDatabaseWithSampleData();
            }
        }

        /// <summary>
        /// Checks if the database is empty (no users)
        /// </summary>
        /// <returns>True if database is empty, otherwise false</returns>
        private bool IsDatabaseEmpty()
        {
            using (var connection = _dbContext.GetConnection())
            using (var command = new SQLiteCommand(connection))
            {
                // Check if any users exist
                command.CommandText = "SELECT COUNT(*) FROM Users";
                var result = command.ExecuteScalar();
                int userCount = Convert.ToInt32(result);

                return userCount == 0;
            }
        }

        /// <summary>
        /// Seeds the database with sample data
        /// </summary>
        private void SeedDatabaseWithSampleData()
        {
            using (var connection = _dbContext.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Add sample destination
                    AddSampleDestination(connection);

                    // Add sample users (admin, drivers, passengers)
                    var userIds = AddSampleUsers(connection);

                    // Add sample vehicles
                    AddSampleVehicles(connection, userIds.DriverIds);

                    // Add sample passengers
                    AddSamplePassengers(connection, userIds.PassengerIds);

                    // Add sample routes and assignments
                    AddSampleRoutes(connection, userIds.DriverIds, userIds.PassengerIds);

                    // Add sample scheduling settings
                    AddSampleSchedulingSettings(connection);

                    // Add sample settings
                    AddSampleSettings(connection);

                    // Commit all changes
                    transaction.Commit();

                    Console.WriteLine("Database seeded with sample data successfully.");
                }
                catch (Exception ex)
                {
                    // Roll back transaction if any error occurs
                    transaction.Rollback();
                    Console.WriteLine($"Error seeding database: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Adds a sample destination
        /// </summary>
        private void AddSampleDestination(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = @"
                    INSERT INTO Destinations (Name, Latitude, Longitude, Address, TargetTime, CreatedAt, UpdatedAt)
                    VALUES ('Office', 32.0853, 34.7818, '30 Rothschild Blvd, Tel Aviv', '09:00', @CreatedAt, @UpdatedAt)";

                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds sample users to the database
        /// </summary>
        /// <returns>Dictionary with user IDs by type</returns>
        private (List<int> DriverIds, List<int> PassengerIds) AddSampleUsers(SQLiteConnection connection)
        {
            var driverIds = new List<int>();
            var passengerIds = new List<int>();

            using (var command = new SQLiteCommand(connection))
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Add admin user
                command.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash, UserType, Name, Email, Phone, CreatedAt, UpdatedAt)
                    VALUES ('admin', @AdminPasswordHash, 'Admin', 'Administrator', 'admin@ridematch.com', '123-456-7890', @Timestamp, @Timestamp);
                    SELECT last_insert_rowid();";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@AdminPasswordHash", HashPassword("admin"));
                command.Parameters.AddWithValue("@Timestamp", timestamp);
                int adminId = Convert.ToInt32(command.ExecuteScalar());

                // Add sample drivers
                for (int i = 1; i <= 3; i++)
                {
                    command.CommandText = @"
                        INSERT INTO Users (Username, PasswordHash, UserType, Name, Email, Phone, CreatedAt, UpdatedAt)
                        VALUES (@Username, @PasswordHash, 'Driver', @Name, @Email, @Phone, @CreatedAt, @UpdatedAt);
                        SELECT last_insert_rowid();";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@Username", $"driver{i}");
                    command.Parameters.AddWithValue("@PasswordHash", HashPassword("password"));
                    command.Parameters.AddWithValue("@Name", $"Driver {i}");
                    command.Parameters.AddWithValue("@Email", $"driver{i}@example.com");
                    command.Parameters.AddWithValue("@Phone", $"555-000-{1000 + i}");
                    command.Parameters.AddWithValue("@CreatedAt", timestamp);
                    command.Parameters.AddWithValue("@UpdatedAt", timestamp);

                    int driverId = Convert.ToInt32(command.ExecuteScalar());
                    driverIds.Add(driverId);
                }

                // Add sample passengers
                for (int i = 1; i <= 10; i++)
                {
                    command.CommandText = @"
                        INSERT INTO Users (Username, PasswordHash, UserType, Name, Email, Phone, CreatedAt, UpdatedAt)
                        VALUES (@Username, @PasswordHash, 'Passenger', @Name, @Email, @Phone, @CreatedAt, @UpdatedAt);
                        SELECT last_insert_rowid();";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@Username", $"passenger{i}");
                    command.Parameters.AddWithValue("@PasswordHash", HashPassword("password"));
                    command.Parameters.AddWithValue("@Name", $"Passenger {i}");
                    command.Parameters.AddWithValue("@Email", $"passenger{i}@example.com");
                    command.Parameters.AddWithValue("@Phone", $"555-100-{1000 + i}");
                    command.Parameters.AddWithValue("@CreatedAt", timestamp);
                    command.Parameters.AddWithValue("@UpdatedAt", timestamp);

                    int passengerId = Convert.ToInt32(command.ExecuteScalar());
                    passengerIds.Add(passengerId);
                }
            }

            return (driverIds, passengerIds);
        }

        /// <summary>
        /// Adds sample vehicles for drivers
        /// </summary>
        private void AddSampleVehicles(SQLiteConnection connection, List<int> driverIds)
        {
            // Base coordinates (Tel Aviv area)
            double baseLat = 32.0700;
            double baseLng = 34.7800;
            Random rand = new Random();

            using (var command = new SQLiteCommand(connection))
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Add vehicles for each driver
                foreach (int driverId in driverIds)
                {
                    // Generate random location nearby base coordinates
                    double lat = baseLat + (rand.NextDouble() - 0.5) * 0.05;
                    double lng = baseLng + (rand.NextDouble() - 0.5) * 0.05;

                    command.CommandText = @"
                        INSERT INTO Vehicles (UserId, Capacity, Latitude, Longitude, Address, IsAvailable, CreatedAt, UpdatedAt)
                        VALUES (@UserId, @Capacity, @Latitude, @Longitude, @Address, @IsAvailable, @CreatedAt, @UpdatedAt)";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@UserId", driverId);
                    command.Parameters.AddWithValue("@Capacity", rand.Next(2, 7)); // Random capacity between 2-6
                    command.Parameters.AddWithValue("@Latitude", lat);
                    command.Parameters.AddWithValue("@Longitude", lng);
                    command.Parameters.AddWithValue("@Address", $"Driver Start Location {driverId}");
                    command.Parameters.AddWithValue("@IsAvailable", 1); // All sample vehicles are available
                    command.Parameters.AddWithValue("@CreatedAt", timestamp);
                    command.Parameters.AddWithValue("@UpdatedAt", timestamp);

                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Adds sample passengers to the database
        /// </summary>
        private void AddSamplePassengers(SQLiteConnection connection, List<int> passengerIds)
        {
            // Base coordinates (Tel Aviv area)
            double baseLat = 32.0700;
            double baseLng = 34.7800;
            Random rand = new Random();

            using (var command = new SQLiteCommand(connection))
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Add passenger profiles for each passenger user
                foreach (int passengerId in passengerIds)
                {
                    // Generate random location nearby base coordinates
                    double lat = baseLat + (rand.NextDouble() - 0.5) * 0.05;
                    double lng = baseLng + (rand.NextDouble() - 0.5) * 0.05;

                    command.CommandText = @"
                        INSERT INTO Passengers (UserId, Name, Latitude, Longitude, Address, IsAvailable, CreatedAt, UpdatedAt)
                        VALUES (@UserId, @Name, @Latitude, @Longitude, @Address, @IsAvailable, @CreatedAt, @UpdatedAt)";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@UserId", passengerId);
                    command.Parameters.AddWithValue("@Name", $"Passenger {passengerId}");
                    command.Parameters.AddWithValue("@Latitude", lat);
                    command.Parameters.AddWithValue("@Longitude", lng);
                    command.Parameters.AddWithValue("@Address", $"Pickup Location {passengerId}");
                    command.Parameters.AddWithValue("@IsAvailable", rand.Next(2) == 0 ? 1 : 0); // 50% are available
                    command.Parameters.AddWithValue("@CreatedAt", timestamp);
                    command.Parameters.AddWithValue("@UpdatedAt", timestamp);

                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Adds sample routes and passenger assignments
        /// </summary>
        private void AddSampleRoutes(SQLiteConnection connection, List<int> driverIds, List<int> passengerIds)
        {
            using (var command = new SQLiteCommand(connection))
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // Create a route for today
                command.CommandText = @"
                    INSERT INTO Routes (Date, GeneratedAt)
                    VALUES (@Date, @GeneratedAt);
                    SELECT last_insert_rowid();";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@Date", today);
                command.Parameters.AddWithValue("@GeneratedAt", timestamp);

                long routeId = Convert.ToInt64(command.ExecuteScalar());

                // Get vehicle IDs for drivers
                var vehicleIds = new List<int>();
                foreach (int driverId in driverIds)
                {
                    command.CommandText = "SELECT Id FROM Vehicles WHERE UserId = @UserId";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@UserId", driverId);

                    var vehicleId = Convert.ToInt32(command.ExecuteScalar());
                    vehicleIds.Add(vehicleId);
                }

                // Get passenger IDs
                var passengerProfiles = new List<(int Id, int UserId)>();
                command.CommandText = "SELECT Id, UserId FROM Passengers WHERE IsAvailable = 1";
                command.Parameters.Clear();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["Id"]);
                        int userId = Convert.ToInt32(reader["UserId"]);
                        passengerProfiles.Add((id, userId));
                    }
                }

                // Distribute passengers among vehicles
                int vehicleIndex = 0;
                Dictionary<int, List<int>> vehicleToPassengers = new Dictionary<int, List<int>>();

                // Initialize dictionary
                foreach (int vehicleId in vehicleIds)
                {
                    vehicleToPassengers[vehicleId] = new List<int>();
                }

                // Distribute passengers
                foreach (var passenger in passengerProfiles)
                {
                    int vehicleId = vehicleIds[vehicleIndex];
                    vehicleToPassengers[vehicleId].Add(passenger.Id);

                    // Move to next vehicle
                    vehicleIndex = (vehicleIndex + 1) % vehicleIds.Count;
                }

                // Create route details and passenger assignments
                foreach (var entry in vehicleToPassengers)
                {
                    int vehicleId = entry.Key;
                    List<int> assignedPassengers = entry.Value;

                    if (assignedPassengers.Count == 0)
                        continue;

                    // Create route detail
                    command.CommandText = @"
                        INSERT INTO RouteDetails (RouteId, VehicleId, TotalDistance, TotalTime, CreatedAt)
                        VALUES (@RouteId, @VehicleId, @TotalDistance, @TotalTime, @CreatedAt);
                        SELECT last_insert_rowid();";

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@RouteId", routeId);
                    command.Parameters.AddWithValue("@VehicleId", vehicleId);
                    command.Parameters.AddWithValue("@TotalDistance", 10.0 + new Random().NextDouble() * 5.0); // Random distance
                    command.Parameters.AddWithValue("@TotalTime", 30.0 + new Random().NextDouble() * 15.0); // Random time
                    command.Parameters.AddWithValue("@CreatedAt", timestamp);

                    long routeDetailId = Convert.ToInt64(command.ExecuteScalar());

                    // Create passenger assignments
                    DateTime basePickupTime = DateTime.Today.AddHours(8); // 8:00 AM base time

                    for (int i = 0; i < assignedPassengers.Count; i++)
                    {
                        int passengerId = assignedPassengers[i];
                        DateTime pickupTime = basePickupTime.AddMinutes(i * 10); // 10 minutes between pickups

                        command.CommandText = @"
                            INSERT INTO RoutePassengers (RouteDetailId, PassengerId, StopOrder, PickupTime)
                            VALUES (@RouteDetailId, @PassengerId, @StopOrder, @PickupTime)";

                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@RouteDetailId", routeDetailId);
                        command.Parameters.AddWithValue("@PassengerId", passengerId);
                        command.Parameters.AddWithValue("@StopOrder", i + 1);
                        command.Parameters.AddWithValue("@PickupTime", pickupTime.ToString("HH:mm:ss"));

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Adds sample scheduling settings
        /// </summary>
        private void AddSampleSchedulingSettings(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                command.CommandText = @"
                    INSERT INTO SchedulingSettings (IsEnabled, ScheduledTime, UpdatedAt)
                    VALUES (1, '07:00', @UpdatedAt)";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@UpdatedAt", timestamp);
                command.ExecuteNonQuery();

                // Add a sample scheduling log entry
                command.CommandText = @"
                    INSERT INTO SchedulingLog (RunTime, Status, RoutesGenerated, PassengersAssigned, ErrorMessage)
                    VALUES (@RunTime, 'Success', 3, 10, NULL)";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@RunTime", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds sample general settings
        /// </summary>
        private void AddSampleSettings(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Add API key setting
                command.CommandText = @"
                    INSERT INTO Settings (Key, Value, UpdatedAt)
                    VALUES ('GoogleMapsApiKey', 'YOUR_GOOGLE_MAPS_API_KEY', @UpdatedAt)";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@UpdatedAt", timestamp);
                command.ExecuteNonQuery();

                // Add default map provider setting
                command.CommandText = @"
                    INSERT INTO Settings (Key, Value, UpdatedAt)
                    VALUES ('DefaultMapProvider', '0', @UpdatedAt)";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@UpdatedAt", timestamp);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Creates a simple password hash for sample data
        /// This just reuses the existing hash method from RideMatchDbContext
        /// </summary>
        private string HashPassword(string password)
        {
            return _dbContext.GetType().GetMethod("HashPassword",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                .Invoke(_dbContext, new object[] { password }) as string;
        }
    }
}