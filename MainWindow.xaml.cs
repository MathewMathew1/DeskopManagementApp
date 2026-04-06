using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ModernWpfApp.Services;
using ModernWpfApp.Utils;

namespace ModernWpfApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _finishLoading = false;
        public bool FinishLoading
        {
            get => _finishLoading;
            set { _finishLoading = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinishLoading))); }
        }

        private bool _isUserLoggedIn = false;
        public bool IsUserLoggedIn
        {
            get => _isUserLoggedIn;
            set { _isUserLoggedIn = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUserLoggedIn))); }
        }

        public ObservableCollection<OrderData> Orders => OrdersStore.Instance.Orders;

        public MainWindow()
        {

            InitializeComponent();

            DataContext = this;

            // Check if user is already logged in
            Loaded += async (_, __) =>
            {
                bool hasUser = await CurrentUserService.Instance.FetchCurrentUserAsync();
                FinishLoading = true;

                if (hasUser)
                {
                     IsUserLoggedIn = true;
                    ShowAvatar();
                    await OrdersStore.Instance.FetchPendingOrdersAsync();
                }
                else
                {
                    ShowLoginButton();
                }
            };
        }

        private void OpenLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow { Owner = this };
            bool? result = loginWindow.ShowDialog();

            if (result == true && CurrentUserService.Instance.User != null)
            {
                ShowAvatar();
            }
        }

        private void ShowLoginButton()
        {
            BtnLogin.Visibility = Visibility.Visible;
            AvatarButton.Visibility = Visibility.Collapsed; // changed from AvatarContainer
            DropdownMenu.Visibility = Visibility.Collapsed; // hide dropdown as well
        }

        private void ShowAvatar()
        {
            BtnLogin.Visibility = Visibility.Collapsed;
            AvatarButton.Visibility = Visibility.Visible;

            var letter = CurrentUserService.Instance.GetAvatarLetter() ?? "?";
            TxtAvatarLetter.Text = letter;
        }

        private void Avatar_Click(object sender, MouseButtonEventArgs e)
        {
            DropdownMenu.Visibility = DropdownMenu.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUserService.Instance.Logout();
            ShowLoginButton();
        }

        // Close dropdown if click outside
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DropdownMenu.Visibility == Visibility.Visible &&
                !DropdownMenu.IsMouseOver && !TxtAvatarLetter.IsMouseOver)
            {
                DropdownMenu.Visibility = Visibility.Collapsed;
            }
        }
    }
}