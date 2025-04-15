using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Data.DbContext
{
    public class RideMatchDbContext : IDisposable
    {
        // Gets the SQLite connection for direct queries
        public SQLiteConnection GetConnection();

        // Creates the database schema if it doesn't exist
        private void CreateDatabase();

        // Updates the database schema if needed for older databases
        private void UpdateDatabaseSchemaIfNeeded();

        // Simple password hashing for demo purposes
        private string HashPassword(string password);

        // IDisposable implementation
        public void Dispose();
    }
}
