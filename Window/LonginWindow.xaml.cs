using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using ModernWpfApp.Services;
using ModernWpfApp.Utils;



namespace ModernWpfApp
{
    internal record LoginResponse(string token);
    internal record ErrorResponse(string error);

    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public AsyncRelayCommand LoginCommand { get; }

        private string _buttonText = "Login";
        public string ButtonText
        {
            get => _buttonText;
            set { _buttonText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonText))); }
        }

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this; // Bind UI to this class

            // Initialize our reusable async command
            LoginCommand = new AsyncRelayCommand(PerformLoginAsync);
        }

        private async Task PerformLoginAsync()
        {
            TxtError.Text = "";
            ButtonText = "Logging in...";

            string user = TxtUsername.Text;
            string pass = TxtPassword.Password;

            try
            {
                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                {
                    TxtError.Text = "Username and Password cannot be empty.";
                    return;
                }

                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync(ApiConfig.LoginUrl, new { name = user, password = pass, mode = "token" });

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    throw new Exception(errorBody?.error ?? "Login failed");
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result == null || string.IsNullOrEmpty(result.token))
                    TxtError.Text = "Invalid login response";

                // Save JWT locally
                JwtStorage.SaveToken(result.token);

                bool fetched = await Services.CurrentUserService.Instance.FetchCurrentUserAsync();
                await OrdersStore.Instance.FetchPendingOrdersAsync();
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                TxtError.Text = ex.Message;
            }
            finally
            {
                ButtonText = "Login";
            }
        }



    }
}