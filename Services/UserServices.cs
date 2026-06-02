using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ModernWpfApp.Utils;

namespace ModernWpfApp.Services
{
    public class CurrentUserService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private static CurrentUserService? _instance;
        public static CurrentUserService Instance => _instance ??= new CurrentUserService();
        private CurrentUserService()
        {
            //
        }
        public async Task CheckConnection()
        {
            try
            {
                var response = await _client.GetAsync(ApiConfig.BaseUrl + "/healthy");
                if (!response.IsSuccessStatusCode) {
                    IsConnected = false;
                    return;
                }

                IsConnected = true;
            }
            catch(Exception e)
            {
               Debug.WriteLine(e);
               IsConnected = false;
            }
        }

        public UserData? User { get; private set; }

        private readonly HttpClient _client = new HttpClient();

        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                _isConnected = value;

                if (value)
                    lConnectionColor = "#10B981";
                else
                    lConnectionColor = "#b93010";

                OnPropertyChanged(nameof(IsConnected));
            }
        }
        private string _lConnectionColor = "#b93010";
        public string lConnectionColor
        {
            get => _lConnectionColor;
            private set
            {
                _lConnectionColor = value;

                OnPropertyChanged(nameof(lConnectionColor));
            }
        }

        public async Task<bool> FetchCurrentUserAsync()
        {
            _ = CheckConnection();
            var token = JwtStorage.LoadToken();
            if (string.IsNullOrEmpty(token)) return false;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _client.GetAsync(ApiConfig.BaseUrl + "/users/me");
                if (!response.IsSuccessStatusCode) return false;

                var user = await response.Content.ReadFromJsonAsync<UserData>();
                if (user == null) return false;

                User = user;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string? GetAvatarLetter()
        {
            if (User == null || string.IsNullOrEmpty(User.Name)) return null;
            return User.Name; // User.Name.Substring(0, 1).ToUpper();
        }

        public void Logout()
        {
            JwtStorage.ClearToken();
            User = null;
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class UserData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}