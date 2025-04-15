using RideMatch.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    /// <summary>
    /// Repository for user-related data access
    /// </summary>
    public class UserRepository
    {
        private readonly RideMatchDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the UserRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public UserRepository(RideMatchDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>Authentication result with user ID and type if successful</returns>
        public async Task<(bool Success, int UserId, string UserType)> AuthenticateUserAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return (false, 0, null);

            // Use Task.Run to wrap the synchronous database operation
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"SELECT Id, PasswordHash, UserType FROM Users WHERE Username = @Username";
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = Convert.ToInt32(reader["Id"]);
                            string hashedPassword = reader["PasswordHash"].ToString();
                            string userType = reader["UserType"].ToString();

                            // Verify password
                            bool isPasswordValid = _dbContext.VerifyPassword(password, hashedPassword);

                            return (isPasswordValid, userId, userType);
                        }
                    }
                }

                return (false, 0, null);
            });
        }

        /// <summary>
        /// Gets user information by ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>User information tuple</returns>
        public async Task<(string Username, string UserType, string Name, string Email, string Phone)> GetUserInfoAsync(int userId)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT Username, UserType, Name, Email, Phone 
                        FROM Users WHERE Id = @UserId";
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (
                                reader["Username"].ToString(),
                                reader["UserType"].ToString(),
                                reader["Name"].ToString(),
                                reader["Email"].ToString(),
                                reader["Phone"].ToString()
                            );
                        }
                    }
                }

                return (null, null, null, null, null);
            });
        }

        /// <summary>
        /// Adds a new user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="userType">The user type (Admin, Driver, Passenger)</param>
        /// <param name="name">The user's name</param>
        /// <param name="email">The user's email (optional)</param>
        /// <param name="phone">The user's phone (optional)</param>
        /// <returns>The new user ID</returns>
        public async Task<int> AddUserAsync(string username, string password, string userType, string name, string email, string phone)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    // Hash the password
                    string hashedPassword = "";
                    // In a real implementation, this would create a secure hash
                    // For this implementation, we're relying on the DbContext's password hashing

                    try
                    {
                        // Check if username already exists
                        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                        command.Parameters.AddWithValue("@Username", username);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        if (count > 0)
                            throw new Exception("Username already exists");

                        // Clear parameters for the next query
                        command.Parameters.Clear();

                        // Insert user
                        command.CommandText = @"
                            INSERT INTO Users (Username, PasswordHash, UserType, Name, Email, Phone, CreatedAt, UpdatedAt)
                            VALUES (@Username, @PasswordHash, @UserType, @Name, @Email, @Phone, @CreatedAt, @UpdatedAt);
                            SELECT last_insert_rowid();";

                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        command.Parameters.AddWithValue("@UserType", userType);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Email", email ?? "");
                        command.Parameters.AddWithValue("@Phone", phone ?? "");
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        // Execute and get the new user ID
                        object result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error adding user: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Updates a user's information
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="name">The user's name</param>
        /// <param name="email">The user's email</param>
        /// <param name="phone">The user's phone</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateUserAsync(int userId, string name, string email, string phone)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        command.CommandText = @"
                            UPDATE Users 
                            SET Name = @Name, Email = @Email, Phone = @Phone, UpdatedAt = @UpdatedAt
                            WHERE Id = @UserId";

                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Email", email ?? "");
                        command.Parameters.AddWithValue("@Phone", phone ?? "");
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error updating user: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Updates a user's profile including user type
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="userType">The user type</param>
        /// <param name="name">The user's name</param>
        /// <param name="email">The user's email</param>
        /// <param name="phone">The user's phone</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateUserProfileAsync(int userId, string userType, string name, string email, string phone)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        command.CommandText = @"
                            UPDATE Users 
                            SET UserType = @UserType, Name = @Name, Email = @Email, Phone = @Phone, UpdatedAt = @UpdatedAt
                            WHERE Id = @UserId";

                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@UserType", userType);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Email", email ?? "");
                        command.Parameters.AddWithValue("@Phone", phone ?? "");
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error updating user profile: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            return await Task.Run(() =>
            {
                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    try
                    {
                        // Hash the new password
                        string hashedPassword = "";
                        // In a real implementation, this would create a secure hash
                        // For this implementation, we're relying on the DbContext's password hashing

                        command.CommandText = @"
                            UPDATE Users 
                            SET PasswordHash = @PasswordHash, UpdatedAt = @UpdatedAt
                            WHERE Id = @UserId";

                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error changing password: {ex.Message}");
                        return false;
                    }
                }
            });
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>List of user information tuples</returns>
        public async Task<List<(int Id, string Username, string UserType, string Name, string Email, string Phone)>> GetAllUsersAsync()
        {
            return await Task.Run(() =>
            {
                var users = new List<(int Id, string Username, string UserType, string Name, string Email, string Phone)>();

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT Id, Username, UserType, Name, Email, Phone 
                        FROM Users
                        ORDER BY Name";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add((
                                Convert.ToInt32(reader["Id"]),
                                reader["Username"].ToString(),
                                reader["UserType"].ToString(),
                                reader["Name"].ToString(),
                                reader["Email"].ToString(),
                                reader["Phone"].ToString()
                            ));
                        }
                    }
                }

                return users;
            });
        }

        /// <summary>
        /// Gets users by type
        /// </summary>
        /// <param name="userType">The user type to filter by</param>
        /// <returns>List of user information tuples</returns>
        public async Task<List<(int Id, string Username, string Name)>> GetUsersByTypeAsync(string userType)
        {
            return await Task.Run(() =>
            {
                var users = new List<(int Id, string Username, string Name)>();

                using (var connection = _dbContext.GetConnection())
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        SELECT Id, Username, Name 
                        FROM Users
                        WHERE UserType = @UserType
                        ORDER BY Name";

                    command.Parameters.AddWithValue("@UserType", userType);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add((
                                Convert.ToInt32(reader["Id"]),
                                reader["Username"].ToString(),
                                reader["Name"].ToString()
                            ));
                        }
                    }
                }

                return users;
            });
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DeleteUserAsync(int userId)
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

                            // First check the user type
                            command.CommandText = "SELECT UserType FROM Users WHERE Id = @UserId";
                            command.Parameters.AddWithValue("@UserId", userId);
                            string userType = (string)command.ExecuteScalar();

                            // Delete associated records based on user type
                            if (userType == "Driver")
                            {
                                // Delete vehicle
                                command.CommandText = "DELETE FROM Vehicles WHERE UserId = @UserId";
                                command.ExecuteNonQuery();
                            }
                            else if (userType == "Passenger")
                            {
                                // Delete passenger and route passenger assignments
                                command.CommandText = @"
                                    DELETE FROM RoutePassengers 
                                    WHERE PassengerId IN (SELECT Id FROM Passengers WHERE UserId = @UserId)";
                                command.ExecuteNonQuery();

                                command.CommandText = "DELETE FROM Passengers WHERE UserId = @UserId";
                                command.ExecuteNonQuery();
                            }

                            // Finally delete the user
                            command.CommandText = "DELETE FROM Users WHERE Id = @UserId";
                            int rowsAffected = command.ExecuteNonQuery();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error
                        Console.WriteLine($"Error deleting user: {ex.Message}");
                        transaction.Rollback();
                        return false;
                    }
                }
            });
        }
    }
}