using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace ModernWpfApp.Services
{
    public class ConnectionService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private static ConnectionService? _instance;
        public static ConnectionService Instance => _instance ??= new ConnectionService();

        private readonly string _saveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_connection.txt");

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                _isConnected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
            }
        }

        private string _connectionString = "";
        public string ConnectionString
        {
            get => _connectionString;
            private set
            {
                _connectionString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectionString)));
            }
        }

        private ConnectionService()
        {
            LoadConnection();
        }

        private void LoadConnection()
        {
            if (File.Exists(_saveFilePath))
            {
                ConnectionString = File.ReadAllText(_saveFilePath);
                // Assume it's connected if we have a saved string (you can change this logic)
                if (!string.IsNullOrWhiteSpace(ConnectionString))
                {
                    IsConnected = true;
                }
            }
        }

        public async Task SaveAndConnectAsync(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("Connection string cannot be empty.");

            // Simulate database connection attempt
            await Task.Delay(1500);

            // Save to file
            File.WriteAllText(_saveFilePath, connectionString);
            
            ConnectionString = connectionString;
            IsConnected = true;
        }

        public void Disconnect()
        {
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
            }
            ConnectionString = "";
            IsConnected = false;
        }
    }
}