using DotNetEnv;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScoutLibrary
{
    public class DBWorker
    {
        /// <summary>
        /// The SQL connection used to interact with the database.
        /// </summary>
        private MySqlConnection _connection = null!;

        /// <summary>
        /// The connection string used to connect to the database.
        /// </summary>
        private string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBWorker"/> class with the specified connection string.
        /// </summary>
        public MySqlConnection Connection => _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBWorker"/> class and loads the connection string from the `DB_CONNECTION_STRING` environment variable.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the `DB_CONNECTION_STRING` environment variable is not set.</exception>
        public DBWorker()
        {
            // Walks up from the current directory looking for a .env file
            Env.TraversePath().Load();
            
            _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? throw new InvalidOperationException("CONNECTION_STRING environment variable is not set.");
        }

        /// <summary>
        /// Connects to the database using the connection string and returns the established SQL connection.
        /// </summary>
        /// <returns>The established SQL connection.</returns>
        public MySqlConnection Connect()
        {
            _connection = new MySqlConnection(_connectionString);
            _connection.Open();
            return _connection;
        }

    }
}