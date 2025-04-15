namespace RideMatch.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string UserType { get; set; } // "Admin", "Driver", "Passenger"
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Constants for user types
        public const string AdminType = "Admin";
        public const string DriverType = "Driver";
        public const string PassengerType = "Passenger";

        // Validates user credentials
        public bool ValidateCredentials(string password)
        {
            // This would use a password hasher in a real implementation
            return PasswordHash == password;
        }

        // Checks if the user has admin privileges
        public bool IsAdmin()
        {
            return UserType == AdminType;
        }

        // Checks if the user has driver privileges 
        public bool IsDriver()
        {
            return UserType == DriverType || IsAdmin();
        }

        // Checks if the user has passenger privileges
        public bool IsPassenger()
        {
            return UserType == PassengerType || IsAdmin();
        }
    }
}