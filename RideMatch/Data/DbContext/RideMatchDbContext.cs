using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RideMatch.Data.DbContext
{
    /// <summary>
    /// Database context for the RideMatch application using SQLite
    /// </summary>
    public class RideMatchDbContext : IDisposable
    {
        private readonly string _databasePath;
        private readonly string _connectionString;
        private SQLiteConnection _connection;
        private bool _disposed = false;

        // Current database schema version
        private const int CURRENT_SCHEMA_VERSION = 1;

        /// <summary>
        /// Initializes the database context with the default database path
        /// </summary>
        public RideMatchDbContext()
        {
            // Set database path in the AppData folder
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RideMatch");

            // Create directory if it doesn't exist
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _databasePath = Path.Combine(appDataPath, "RideMatchDb.sqlite");
            _connectionString = $"Data Source={_databasePath};Version=3;";

            // Create the database if it doesn't exist
            if (!File.Exists(_databasePath))
            {
                CreateDatabase();
            }
            else
            {
                // Check if schema update is needed
                UpdateDatabaseSchemaIfNeeded();
            }
        }

        /// <summary>
        /// Gets the SQLite connection for direct queries
        /// </summary>
        /// <returns>The SQLite connection</returns>
        public SQLiteConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SQLiteConnection(_connectionString);
                _connection.Open();
            }
            else if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }

        /// <summary>
        /// Creates the database schema
        /// </summary>
        private void CreateDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Create schema version table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE SchemaVersion (
                            Version INTEGER NOT NULL,
                            UpdatedAt TEXT NOT NULL
                        )";
                    command.ExecuteNonQuery();

                    // Insert current schema version
                    command.CommandText = @"
                        INSERT INTO SchemaVersion (Version, UpdatedAt)
                        VALUES (@Version, @UpdatedAt)";
                    command.Parameters.AddWithValue("@Version", CURRENT_SCHEMA_VERSION);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }

                // Create users table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT NOT NULL UNIQUE,
                            PasswordHash TEXT NOT NULL,
                            UserType TEXT NOT NULL,
                            Name TEXT NOT NULL,
                            Email TEXT,
                            Phone TEXT,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL
                        )";
                    command.ExecuteNonQuery();
                }

                // Create vehicles table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE Vehicles (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            UserId INTEGER NOT NULL,
                            Capacity INTEGER NOT NULL,
                            Latitude REAL NOT NULL,
                            Longitude REAL NOT NULL,
                            Address TEXT NOT NULL,
                            IsAvailable INTEGER NOT NULL,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(Id)
                        )";
                    command.ExecuteNonQuery();
                }

                // Create passengers table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE Passengers (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            UserId INTEGER NOT NULL,
                            Name TEXT NOT NULL,
                            Latitude REAL NOT NULL,
                            Longitude REAL NOT NULL,
                            Address TEXT NOT NULL,
                            IsAvailable INTEGER NOT NULL,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(Id)
                        )";
                    command.ExecuteNonQuery();
                }

                // Create destinations table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE Destinations (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Latitude REAL NOT NULL,
                            Longitude REAL NOT NULL,
                            Address TEXT NOT NULL,
                            TargetTime TEXT,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL
                        )";
                    command.ExecuteNonQuery();
                }

                // Create routes table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE Routes (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Date TEXT NOT NULL,
                            GeneratedAt TEXT NOT NULL
                        )";
                    command.ExecuteNonQuery();
                }

                // Create route details table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE RouteDetails (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            RouteId INTEGER NOT NULL,
                            VehicleId INTEGER NOT NULL,
                            TotalDistance REAL NOT NULL,
                            TotalTime REAL NOT NULL,
                            CreatedAt TEXT NOT NULL,
                            FOREIGN KEY (RouteId) REFERENCES Routes(Id),
                            FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id)
                        )";
                    command.ExecuteNonQuery();
                }

                // Create route passenger assignments table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE RoutePassengers (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            RouteDetailId INTEGER NOT NULL,
                            PassengerId INTEGER NOT NULL,
                            StopOrder INTEGER NOT NULL,
                            PickupTime TEXT,
                            FOREIGN KEY (RouteDetailId) REFERENCES RouteDetails(Id),
                            FOREIGN KEY (PassengerId) REFERENCES Passengers(Id)
                        )";
                    command.ExecuteNonQuery();
                }

                // Create scheduling settings table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE SchedulingSettings (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            IsEnabled INTEGER NOT NULL,
                            ScheduledTime TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL
                        )";
                    command.ExecuteNonQuery();

                    // Insert default settings
                    command.CommandText = @"
                        INSERT INTO SchedulingSettings (IsEnabled, ScheduledTime, UpdatedAt)
                        VALUES (0, '07:00', @UpdatedAt)";
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }

                // Create scheduling log table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE SchedulingLog (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            RunTime TEXT NOT NULL,
                            Status TEXT NOT NULL,
                            RoutesGenerated INTEGER NOT NULL,
                            PassengersAssigned INTEGER NOT NULL,
                            ErrorMessage TEXT
                        )";
                    command.ExecuteNonQuery();
                }

                // Create general settings table
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE Settings (
                            Key TEXT PRIMARY KEY,
                            Value TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL
                        )";
                    command.ExecuteNonQuery();
                }

                // Create default admin user
                using (var command = new SQLiteCommand(connection))
                {
                    string hashedPassword = HashPassword("admin");
                    command.CommandText = @"
                        INSERT INTO Users (Username, PasswordHash, UserType, Name, Email, Phone, CreatedAt, UpdatedAt)
                        VALUES ('admin', @PasswordHash, 'Admin', 'Administrator', 'admin@ridematch.com', '123-456-7890', @CreatedAt, @UpdatedAt)";
                    command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }

                // Create default destination
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        INSERT INTO Destinations (Name, Latitude, Longitude, Address, TargetTime, CreatedAt, UpdatedAt)
                        VALUES ('Office', 32.0853, 34.7818, '30 Rothschild Blvd, Tel Aviv', '09:00', @CreatedAt, @UpdatedAt)";
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }

                // Create demo driver user
                using (var command = new SQLiteCommand(connection))
                {
                    string hashedPassword = HashPassword("password");
                    command.CommandText = @"
                        INSERT INTO Users (Username, PasswordHash, UserType, Name, Email, Phone, CreatedAt, UpdatedAt)
                        VALUES ('driver1', @PasswordHash, 'Driver', 'John Driver', 'john@example.com', '555-123-4567', @CreatedAt, @UpdatedAt)";
                    command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();

                    // Get the driver user ID
                    command.CommandText = "SELECT last_insert_rowid()";
                    long driverUserId = (long)command.ExecuteScalar();

                    // Create vehicle for the driver
                    command.CommandText = @"
                        INSERT INTO Vehicles (UserId, Capacity, Latitude, Longitude, Address, IsAvailable, CreatedAt, UpdatedAt)
                        VALUES (@UserId, 4, 32.0741, 34.7922, '123 Main St, Tel Aviv', 1, @CreatedAt, @UpdatedAt)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@UserId", driverUserId);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }

                // Create demo passenger user
                using (var command = new SQLiteCommand(connection))
                {
                    string hashedPassword = HashPassword("password");
                    command.CommandText = @"
                        INSERT INTO Users (Username, PasswordHash, UserType, Name, Email, Phone, CreatedAt, UpdatedAt)
                        VALUES ('passenger1', @PasswordHash, 'Passenger', 'Sarah Passenger', 'sarah@example.com', '555-987-6543', @CreatedAt, @UpdatedAt)";
                    command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();

                    // Get the passenger user ID
                    command.CommandText = "SELECT last_insert_rowid()";
                    long passengerUserId = (long)command.ExecuteScalar();

                    // Create passenger profile
                    command.CommandText = @"
                        INSERT INTO Passengers (UserId, Name, Latitude, Longitude, Address, IsAvailable, CreatedAt, UpdatedAt)
                        VALUES (@UserId, 'Sarah Passenger', 32.0733, 34.7805, '456 Park Ave, Tel Aviv', 1, @CreatedAt, @UpdatedAt)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@UserId", passengerUserId);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Updates the database schema if needed for older databases
        /// </summary>
        private void UpdateDatabaseSchemaIfNeeded()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Check if SchemaVersion table exists
                bool schemaVersionExists = false;
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT name FROM sqlite_master 
                        WHERE type='table' AND name='SchemaVersion'";
                    var result = command.ExecuteScalar();
                    schemaVersionExists = result != null;
                }

                // If SchemaVersion table doesn't exist, create it and set version to 1
                if (!schemaVersionExists)
                {
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = @"
                            CREATE TABLE SchemaVersion (
                                Version INTEGER NOT NULL,
                                UpdatedAt TEXT NOT NULL
                            )";
                        command.ExecuteNonQuery();

                        // Insert current schema version
                        command.CommandText = @"
                            INSERT INTO SchemaVersion (Version, UpdatedAt)
                            VALUES (@Version, @UpdatedAt)";
                        command.Parameters.AddWithValue("@Version", 1);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                    return;
                }

                // Get current schema version
                int currentVersion = 0;
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT MAX(Version) FROM SchemaVersion";
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        currentVersion = Convert.ToInt32(result);
                    }
                }

                // Apply migrations based on current version
                if (currentVersion < CURRENT_SCHEMA_VERSION)
                {
                    // In a real application, we would have migration scripts here
                    // For example:
                    // if (currentVersion < 2)
                    // {
                    //     // Apply migration script for version 2
                    //     using (var command = new SQLiteCommand(connection))
                    //     {
                    //         command.CommandText = "ALTER TABLE Users ADD COLUMN NewColumn TEXT";
                    //         command.ExecuteNonQuery();
                    //     }
                    // }

                    // Update schema version
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = @"
                            INSERT INTO SchemaVersion (Version, UpdatedAt)
                            VALUES (@Version, @UpdatedAt)";
                        command.Parameters.AddWithValue("@Version", CURRENT_SCHEMA_VERSION);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Simple password hashing for demo purposes
        /// In a real application, use a more secure hashing algorithm like bcrypt
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Create a random salt
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt);
                }

                // Combine password and salt
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] combined = new byte[passwordBytes.Length + salt.Length];
                Buffer.BlockCopy(passwordBytes, 0, combined, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, combined, passwordBytes.Length, salt.Length);

                // Compute hash
                byte[] hashBytes = sha256.ComputeHash(combined);

                // Combine hash and salt for storage
                byte[] hashWithSalt = new byte[hashBytes.Length + salt.Length];
                Buffer.BlockCopy(hashBytes, 0, hashWithSalt, 0, hashBytes.Length);
                Buffer.BlockCopy(salt, 0, hashWithSalt, hashBytes.Length, salt.Length);

                // Convert to base64 string
                return Convert.ToBase64String(hashWithSalt);
            }
        }

        /// <summary>
        /// Checks if a password matches a hash
        /// </summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Convert the stored hash back to bytes
            byte[] hashWithSalt = Convert.FromBase64String(hashedPassword);

            // The first 32 bytes are the hash, the rest is the salt
            byte[] storedHash = new byte[32];
            byte[] storedSalt = new byte[hashWithSalt.Length - 32];
            Buffer.BlockCopy(hashWithSalt, 0, storedHash, 0, 32);
            Buffer.BlockCopy(hashWithSalt, 32, storedSalt, 0, storedSalt.Length);

            // Hash the provided password with the same salt
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] combined = new byte[passwordBytes.Length + storedSalt.Length];
                Buffer.BlockCopy(passwordBytes, 0, combined, 0, passwordBytes.Length);
                Buffer.BlockCopy(storedSalt, 0, combined, passwordBytes.Length, storedSalt.Length);

                byte[] computedHash = sha256.ComputeHash(combined);

                // Compare the computed hash with the stored hash
                return storedHash.SequenceEqual(computedHash);
            }
        }

        /// <summary>
        /// Implements the IDisposable pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implements the IDisposable pattern
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose of managed resources
                    if (_connection != null)
                    {
                        if (_connection.State == ConnectionState.Open)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                        _connection = null;
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~RideMatchDbContext()
        {
            Dispose(false);
        }
    }
}