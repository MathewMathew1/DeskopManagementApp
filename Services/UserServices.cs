using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ModernWpfApp.Utils;

namespace ModernWpfApp.Services
{
    public class CurrentUserService
    {
        private static CurrentUserService? _instance;
        public static CurrentUserService Instance => _instance ??= new CurrentUserService();

        private CurrentUserService() { }

        public UserData? User { get; private set; }

        private readonly HttpClient _client = new HttpClient();

        public async Task<bool> FetchCurrentUserAsync()
        {
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
            return User.Name.Substring(0, 1).ToUpper();
        }

        public void Logout()
        {
            JwtStorage.ClearToken();
            User = null;
        }

    }

    public class UserData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}