using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using ModernWpfApp.Services;

namespace ModernWpfApp
{
    public partial class ConnectionWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public AsyncRelayCommand ConnectCommand { get; }

        private string _connectButtonText = "Connect & Save";
        public string ConnectButtonText
        {
            get => _connectButtonText;
            set { _connectButtonText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectButtonText))); }
        }

        public ConnectionWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Load existing connection string if any
            TxtConnectionString.Text = ConnectionService.Instance.ConnectionString;

            ConnectCommand = new AsyncRelayCommand(PerformConnectionAsync);
        }

        private async Task PerformConnectionAsync()
        {
            TxtError.Text = "";
            ConnectButtonText = "Connecting...";

            try
            {
                await ConnectionService.Instance.SaveAndConnectAsync(TxtConnectionString.Text);
                
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                TxtError.Text = ex.Message;
            }
            finally
            {
                ConnectButtonText = "Connect & Save";
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectionService.Instance.Disconnect();
            TxtConnectionString.Text = "";
            TxtError.Text = "Disconnected successfully.";
            TxtError.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
        }
    }
}