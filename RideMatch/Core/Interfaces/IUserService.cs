using RideMatch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Interfaces
{
    public interface IUserService
    {
        // Authenticates a user with username and password
        Task<(bool Success, User User)> AuthenticateAsync(string username, string password);

        // Gets a user by ID
        Task<User> GetUserByIdAsync(int userId);

        // Creates a new user
        Task<int> CreateUserAsync(User user, string password);

        // Updates an existing user
        Task<bool> UpdateUserAsync(User user);

        // Changes a user's password
        Task<bool> ChangePasswordAsync(int userId, string newPassword);

        // Deletes a user
        Task<bool> DeleteUserAsync(int userId);

        // Gets all users
        Task<IEnumerable<User>> GetAllUsersAsync();

        // Gets users by type
        Task<IEnumerable<User>> GetUsersByTypeAsync(string userType);
    }
}
