using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Data.Repository
{
    public class UserRepository
    {
        // Authenticates a user
        public Task<(bool Success, int UserId, string UserType)> AuthenticateUserAsync(string username, string password);

        // Gets user information by ID
        public Task<(string Username, string UserType, string Name, string Email, string Phone)> GetUserInfoAsync(int userId);

        // Adds a new user
        public Task<int> AddUserAsync(string username, string password, string userType, string name, string email, string phone);

        // Updates a user's information
        public Task<bool> UpdateUserAsync(int userId, string name, string email, string phone);

        // Updates a user's profile including user type
        public Task<bool> UpdateUserProfileAsync(int userId, string userType, string name, string email, string phone);

        // Changes a user's password
        public Task<bool> ChangePasswordAsync(int userId, string newPassword);

        // Gets all users
        public Task<List<(int Id, string Username, string UserType, string Name, string Email, string Phone)>> GetAllUsersAsync();

        // Gets users by type
        public Task<List<(int Id, string Username, string Name)>> GetUsersByTypeAsync(string userType);

        // Deletes a user
        public Task<bool> DeleteUserAsync(int userId);
    }
}
