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
            // Do not show password :>
            // TxtConnectionString.Text = ConnectionService.Instance.ConnectionString;
            if(ConnectionService.Instance.ConnectionString == "")
                TxtConnectionLabel.Text = "Add connection string (SQL server):";
            else 
                TxtConnectionLabel.Text = "Replace connection string (SQL server):";

            ConnectCommand = new AsyncRelayCommand(PerformConnectionAsync);
        }

        private async Task PerformConnectionAsync()
        {
            TxtError.Text = "";
            ConnectButtonText = "Connecting...";

            try
            {
                await ConnectionService.Instance.SaveAndConnectAsync(TxtConnectionString.Text);
                
                TxtError.Text = "Success.";
                TxtError.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                TxtError.Text = ex.Message;
                TxtError.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
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