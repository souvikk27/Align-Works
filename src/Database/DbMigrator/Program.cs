using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DbMigrator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string? serverConnString = config.GetConnectionString("SqlConnection");
            string? dbName = config.GetSection("Settings")["DbName"];
            string? dbUser = config.GetSection("Settings")["DbUser"];
            string? dbPassword = config.GetSection("Settings")["DbPassword"];
            string scriptPath = "";

            try
            {
                using (var conn = new NpgsqlConnection(serverConnString))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS {dbName}";
                        cmd.ExecuteNonQuery();
                    }

                    string dbConnString = $"Host=localhost;Username={dbUser};Password={dbPassword};Database={dbName}";
                    using (var dbConn = new NpgsqlConnection(dbConnString))
                    {
                        dbConn.Open();

                        string script;
                        using (var reader = new System.IO.StreamReader(scriptPath))
                        {
                            script = reader.ReadToEnd();
                        }

                        using (var cmd = new NpgsqlCommand(script, dbConn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        Console.WriteLine("Script executed successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}