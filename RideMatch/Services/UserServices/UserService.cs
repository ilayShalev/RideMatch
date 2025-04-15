using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Services.UserServices
{
    public class UserService : IUserService
    {
        // Authenticates a user with username and password
        public Task<(bool Success, User User)> AuthenticateAsync(string username, string password);

        // Gets a user by ID
        public Task<User> GetUserByIdAsync(int userId);

        // Creates a new user
        public Task<int> CreateUserAsync(User user, string password);

        // Updates an existing user
        public Task<bool> UpdateUserAsync(User user);

        // Changes a user's password
        public Task<bool> ChangePasswordAsync(int userId, string newPassword);

        // Deletes a user
        public Task<bool> DeleteUserAsync(int userId);

        // Gets all users
        public Task<IEnumerable<User>> GetAllUsersAsync();

        // Gets users by type
        public Task<IEnumerable<User>> GetUsersByTypeAsync(string userType);
    }
}
