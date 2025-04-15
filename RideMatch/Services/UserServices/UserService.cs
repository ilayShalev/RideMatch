using RideMatch.Core.Interfaces;
using RideMatch.Core.Models;
using RideMatch.Data.Repository;
using RideMatch.Utilities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RideMatch.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;

        // Constructor
        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        // Authenticates a user with username and password
        public async Task<(bool Success, User User)> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            var result = await _userRepository.AuthenticateUserAsync(username, password);

            if (!result.Success)
                return (false, null);

            // Load full user information
            var userInfo = await _userRepository.GetUserInfoAsync(result.UserId);

            // Create user object
            var user = new User
            {
                Id = result.UserId,
                Username = username,
                UserType = result.UserType,
                Name = userInfo.Name,
                Email = userInfo.Email,
                Phone = userInfo.Phone
            };

            return (true, user);
        }

        // Gets a user by ID
        public async Task<User> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(userId));

            var userInfo = await _userRepository.GetUserInfoAsync(userId);

            return new User
            {
                Id = userId,
                Username = userInfo.Username,
                UserType = userInfo.UserType,
                Name = userInfo.Name,
                Email = userInfo.Email,
                Phone = userInfo.Phone
            };
        }

        // Creates a new user
        public async Task<int> CreateUserAsync(User user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Validate user data
            if (string.IsNullOrEmpty(user.Username))
                throw new ArgumentException("Username cannot be null or empty", nameof(user.Username));

            if (string.IsNullOrEmpty(user.UserType))
                throw new ArgumentException("User type cannot be null or empty", nameof(user.UserType));

            if (string.IsNullOrEmpty(user.Name))
                throw new ArgumentException("Name cannot be null or empty", nameof(user.Name));

            // Create user in repository
            return await _userRepository.AddUserAsync(
                user.Username,
                password,
                user.UserType,
                user.Name,
                user.Email ?? string.Empty,
                user.Phone ?? string.Empty);
        }

        // Updates an existing user
        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (user.Id <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(user.Id));

            // Validate user data
            if (string.IsNullOrEmpty(user.Name))
                throw new ArgumentException("Name cannot be null or empty", nameof(user.Name));

            // For admin user, allow updating the user type
            if (user.IsAdmin())
            {
                return await _userRepository.UpdateUserProfileAsync(
                    user.Id,
                    user.UserType,
                    user.Name,
                    user.Email ?? string.Empty,
                    user.Phone ?? string.Empty);
            }
            else
            {
                // For regular user, just update basic info
                return await _userRepository.UpdateUserAsync(
                    user.Id,
                    user.Name,
                    user.Email ?? string.Empty,
                    user.Phone ?? string.Empty);
            }
        }

        // Changes a user's password
        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(userId));

            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("New password cannot be null or empty", nameof(newPassword));

            return await _userRepository.ChangePasswordAsync(userId, newPassword);
        }

        // Deletes a user
        public async Task<bool> DeleteUserAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(userId));

            return await _userRepository.DeleteUserAsync(userId);
        }

        // Gets all users
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();

            return users.Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                UserType = u.UserType,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone
            });
        }

        // Gets users by type
        public async Task<IEnumerable<User>> GetUsersByTypeAsync(string userType)
        {
            if (string.IsNullOrEmpty(userType))
                throw new ArgumentException("User type cannot be null or empty", nameof(userType));

            var users = await _userRepository.GetUsersByTypeAsync(userType);

            return users.Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Name = u.Name
            });
        }
    }
}