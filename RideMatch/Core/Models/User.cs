using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Core.Models
{
    public class User
    {
        // Properties for user details (ID, username, password hash, type, etc.)

        // Validates user credentials
        public bool ValidateCredentials(string password);

        // Checks if the user has admin privileges
        public bool IsAdmin();

        // Checks if the user has driver privileges 
        public bool IsDriver();

        // Checks if the user has passenger privileges
        public bool IsPassenger();
    }
}
