using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RideMatch.Models;

namespace RideMatch.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\RideMatchDB.mdf;Integrated Security=True";
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