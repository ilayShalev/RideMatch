using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using RideMatch.Models;

namespace RideMatch.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public DatabaseService()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RideMatch");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _databasePath = Path.Combine(appDataPath, "RideMatchDB.mdf");
            _connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={_databasePath};Integrated Security=True";

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // If database doesn't exist, create it
            if (!File.Exists(_databasePath))
            {
                string masterConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True";
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand($"CREATE DATABASE RideMatchDB ON PRIMARY (NAME = RideMatch_Data, FILENAME = '{_databasePath}')", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Create tables
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    // Create Drivers table
                    using (var command = new SqlCommand(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Drivers' and xtype='U')
                        CREATE TABLE Drivers (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Name NVARCHAR(100) NOT NULL,
                            PhoneNumber NVARCHAR(20) NOT NULL,
                            Address NVARCHAR(200) NOT NULL,
                            IsAvailable BIT NOT NULL DEFAULT 1,
                            LastUpdate DATETIME NOT NULL DEFAULT GETDATE()
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create Vehicles table
                    using (var command = new SqlCommand(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Vehicles' and xtype='U')
                        CREATE TABLE Vehicles (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            DriverId INT NOT NULL,
                            LicensePlate NVARCHAR(20) NOT NULL,
                            Capacity INT NOT NULL,
                            IsAvailable BIT NOT NULL DEFAULT 1,
                            LastUpdate DATETIME NOT NULL DEFAULT GETDATE(),
                            FOREIGN KEY (DriverId) REFERENCES Drivers(Id)
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create Passengers table
                    using (var command = new SqlCommand(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Passengers' and xtype='U')
                        CREATE TABLE Passengers (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Name NVARCHAR(100) NOT NULL,
                            PhoneNumber NVARCHAR(20) NOT NULL,
                            Address NVARCHAR(200) NOT NULL,
                            IsAvailable BIT NOT NULL DEFAULT 1,
                            LastUpdate DATETIME NOT NULL DEFAULT GETDATE(),
                            AssignedDriverId INT,
                            PickupTime DATETIME,
                            FOREIGN KEY (AssignedDriverId) REFERENCES Drivers(Id)
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Add some sample data
                    using (var command = new SqlCommand(@"
                        IF NOT EXISTS (SELECT * FROM Drivers)
                        BEGIN
                            INSERT INTO Drivers (Name, PhoneNumber, Address) VALUES 
                            (N'משה כהן', '050-1234567', N'רחוב הרצל 1, תל אביב'),
                            (N'דוד לוי', '052-7654321', N'רחוב ויצמן 15, חיפה');

                            INSERT INTO Vehicles (DriverId, LicensePlate, Capacity) VALUES 
                            (1, '12-345-67', 4),
                            (2, '98-765-43', 6);

                            INSERT INTO Passengers (Name, PhoneNumber, Address) VALUES 
                            (N'יעל ישראלי', '053-1112233', N'רחוב דיזנגוף 100, תל אביב'),
                            (N'רון חיים', '054-4445566', N'רחוב אלנבי 50, תל אביב'),
                            (N'מיכל שלום', '055-7778899', N'רחוב הנביאים 25, חיפה');
                        END", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<Driver> GetAllDrivers()
        {
            var drivers = new List<Driver>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT * FROM Drivers", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            drivers.Add(new Driver
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                PhoneNumber = reader.GetString(2),
                                Address = reader.GetString(3),
                                IsAvailable = reader.GetBoolean(4),
                                LastUpdate = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }
            return drivers;
        }

        public List<Passenger> GetAllPassengers()
        {
            var passengers = new List<Passenger>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT * FROM Passengers", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            passengers.Add(new Passenger
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                PhoneNumber = reader.GetString(2),
                                Address = reader.GetString(3),
                                IsAvailable = reader.GetBoolean(4),
                                LastUpdate = reader.GetDateTime(5),
                                AssignedDriverId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                                PickupTime = reader.IsDBNull(7) ? null : (DateTime?)reader.GetDateTime(7)
                            });
                        }
                    }
                }
            }
            return passengers;
        }

        public List<Vehicle> GetAllVehicles()
        {
            var vehicles = new List<Vehicle>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT * FROM Vehicles", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vehicles.Add(new Vehicle
                            {
                                Id = reader.GetInt32(0),
                                DriverId = reader.GetInt32(1),
                                LicensePlate = reader.GetString(2),
                                Capacity = reader.GetInt32(3),
                                IsAvailable = reader.GetBoolean(4),
                                LastUpdate = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }
            return vehicles;
        }

        public void UpdatePassengerAssignment(int passengerId, int? driverId, DateTime? pickupTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE Passengers SET AssignedDriverId = @DriverId, PickupTime = @PickupTime WHERE Id = @PassengerId", connection))
                {
                    command.Parameters.AddWithValue("@PassengerId", passengerId);
                    command.Parameters.AddWithValue("@DriverId", (object)driverId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PickupTime", (object)pickupTime ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateAvailability(int id, bool isAvailable, string type)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string tableName = type == "driver" ? "Drivers" : type == "passenger" ? "Passengers" : "Vehicles";
                using (var command = new SqlCommand($"UPDATE {tableName} SET IsAvailable = @IsAvailable, LastUpdate = @LastUpdate WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@IsAvailable", isAvailable);
                    command.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
} 