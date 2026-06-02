using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BoardGameFrontend.AjramData;
using Microsoft.Data.SqlClient; // ONLY this one.

namespace ModernWpfApp.Services
{
    public class ConnectionService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private static ConnectionService? _instance;
        public static ConnectionService Instance => _instance ??= new ConnectionService();
        private readonly string _saveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_connection.dat");
        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            private set { _isConnected = value; 
            
            if(value)
                lConnectionColor = "#10B981";
            else
                lConnectionColor = "#b93010";
            
            OnPropertyChanged(nameof(IsConnected)); }
        }

        private string _lConnectionColor = "#b93010";
        public string lConnectionColor
        {
            get => _lConnectionColor;
            private set { _lConnectionColor = value; 
            
            OnPropertyChanged(nameof(lConnectionColor)); }
        }


        private string _connectionString = "";
        public string ConnectionString
        {
            get => _connectionString;
            private set { _connectionString = value ?? ""; OnPropertyChanged(nameof(ConnectionString)); }
        }

        private ConnectionService() 
        {
            // We just load the string, we don't "test" it in the constructor 
            // to avoid UI deadlocks that cause NullReferenceExceptions.
            LoadSavedString();
        }

        private void LoadSavedString()
        {
            if (!File.Exists(_saveFilePath)) return;
            try
            {
                var encrypted = File.ReadAllBytes(_saveFilePath);
                var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                ConnectionString = Encoding.UTF8.GetString(decrypted);
                if(!string.IsNullOrEmpty(ConnectionString))
                    CheckConnection(ConnectionString);
            }
            catch { Disconnect(); }
        }

        public async Task CheckConnection(string rawConnectionString)
        {
            try
            {
                using (var connection = new SqlConnection(rawConnectionString))
                {
                    if (connection == null) throw new Exception("Failed to initialize SqlConnection object.");
                    
                    await connection.OpenAsync();
                }
                IsConnected = true;
            }
            catch
            {
                IsConnected = false;
            }
        }
        public async Task SaveAndConnectAsync(string rawConnectionString)
        {
            // 1. Critical Guard: If the UI passes a null string, this is your NullRef.
            if (string.IsNullOrWhiteSpace(rawConnectionString))
                throw new ArgumentException("Connection string is null or empty.");

            string processedString = "";

            try
            {

                var builder = new SqlConnectionStringBuilder(rawConnectionString);
                
       
                builder.TrustServerCertificate = true; 
                builder.Encrypt = false;
                builder.ConnectTimeout = 5;
                
                processedString = builder.ConnectionString;

                using (var connection = new SqlConnection(processedString))
                {
                    if (connection == null) throw new Exception("Failed to initialize SqlConnection object.");
                    
                    await connection.OpenAsync();
                }

                var bytes = Encoding.UTF8.GetBytes(processedString);
                var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                await File.WriteAllBytesAsync(_saveFilePath, encrypted);

                ConnectionString = processedString;
                IsConnected = true;
            }
            catch (SqlException ex)
            {
                Disconnect();
                throw new Exception($"SQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Disconnect();
                throw new Exception($"System Error: {ex.Message}");
            }
        }

        public SqlConnection GetConnection()
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected.");
            return new SqlConnection(ConnectionString);
        }

        public void Disconnect()
        {
            if (File.Exists(_saveFilePath)) File.Delete(_saveFilePath);
            ConnectionString = "";
            IsConnected = false;
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}