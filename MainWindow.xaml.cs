using System.Collections.ObjectModel;
using System.ComponentModel;

using System.Windows;
using System.Windows.Input;
using ModernWpfApp.Services;
using System.Diagnostics;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using BoardGameFrontend.AjramData;

namespace ModernWpfApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Expose the singleton so XAML can bind to Connection.IsConnected
        public ConnectionService Connection => ConnectionService.Instance;
        public CurrentUserService CurrentUserService => CurrentUserService.Instance;

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

        // Dummy list to prevent errors if OrdersStore isn't fully set up yet
        public ObservableCollection<OrderData> Orders => OrdersStore.Instance.Orders;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += async (_, __) =>
            {
                // Wait for your services
                bool hasUser = await CurrentUserService.Instance.FetchCurrentUserAsync();
                FinishLoading = true;

                if (hasUser)
                {
                    IsUserLoggedIn = true;
                    ShowAvatar();
                //    await OrdersStore.Instance.FetchPendingOrdersAsync();
                }
                else
                {
                    ShowLoginButton();
                }
            };
        }

        // --- NEW: Open Connection Window ---
        private void OpenConnection_Click(object sender, RoutedEventArgs e)
        {
            var connWindow = new ConnectionWindow { Owner = this };
            connWindow.ShowDialog();
        }
        private void OnUpdateSlizgacze_Click(object sender, RoutedEventArgs e)
        {
            // DetaleSlizgacz    ID  Typ TypEng
            CheckJustSqlConnection("SELECT ID, Typ, TypEng FROM dbo.DetaleSlizgacz;");
          //  LoadTableDynamicAsync("SELECT * FROM dbo.DetaleSlizgacz;");
        }

        public void CheckJustSqlConnection(string query)
        {
            string connectionstring = @"Data Source=TOMPC\SQLAJRAMTEST;Initial Catalog=AjramTets3;Integrated Security=True;Encrypt=False";
            SqlConnection conn = new SqlConnection(connectionstring);
        //    SqlConnection conn = new SqlConnection("brazil");
        //    SqlConnection conn = new SqlConnection(@"Data Source = SERWERAJRAM01; Initial Catalog = AJRAMDB_SQL_2024; Persist Security Info = True; User ID = AJRAMaccess2024; Password = ajramDBsql_ht321!");
            
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
/*                SqlConnection thisConnection = new SqlConnection(@"Server=TOMPC\SQLAJRAMTEST;Database=AjramTets3;Trusted_Connection=Yes;Encrypt=No");
                thisConnection.Open();

                string Get_Data = query;     

                SqlCommand cmd = thisConnection.CreateCommand();
                cmd.CommandText = Get_Data;

                SqlDataAdapter sda = new SqlDataAdapter(cmd);               
                DataTable dt = new DataTable("emp");
                sda.Fill(dt);
                conn.Open();
                    MessageBox.Show(query); */
                SqlCommand cmd = new SqlCommand(query, conn);
                adapter.SelectCommand = cmd;
                adapter.Fill(ds, "MyQuery");

                foreach (DataRow row in ds.Tables["MyQuery"].Rows)
                {
                    int tmpID = (int) row["ID"];
                    string tmpTyp = (string) row["Typ"];
                    string tmpTypEng = (string) row["TypEng"];
                    MessageBox.Show("ID: " + tmpID.ToString() + ", Typ: " + tmpTyp + ", TypEng: " + tmpTypEng);
                }
                
            /*    conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Wartość FROM dbo.Wartosci_Pomocnicze WHERE Nazwa = 'Wersja'", conn);
                adapter.SelectCommand = cmd;
                MessageBox.Show("1");
                adapter.Fill(ds, "MyQuery");
                MessageBox.Show("2");

                foreach (DataRow row in ds.Tables["MyQuery"].Rows)
                {
                    int iVersion = (int) row["Wartość"];
                    MessageBox.Show("Test: " + iVersion.ToString());
                }
                conn.Close(); */
            }
            catch(Exception e){
                Debug.WriteLine(e.ToString());
                MessageBox.Show(e.ToString());
            }
        }
        public async Task<List<Dictionary<string, object>>> LoadTableDynamicAsync(string query)
        {
            if (!ConnectionService.Instance.IsConnected)
                throw new InvalidOperationException("Local database is not connected.");

             var result = new List<Dictionary<string, object>>();
            try
            {
                            MessageBox.Show("Query: " + query);
                using (var connection = ConnectionService.Instance.GetConnection())
                {
                    await connection.OpenAsync();

                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    try
                    {
                        SqlCommand cmd = new SqlCommand(query, connection);
                        adapter.SelectCommand = cmd;
                        adapter.Fill(ds, "TmpCheckSQL");

                        foreach (DataRow row in ds.Tables["TmpCheckSQL"].Rows)
                        {
                            int tmpID = (int) row["ID"];
                            int tmpTyp = (int) row["Typ"];
                            int tmpTypEng = (int) row["TypEng"];
                            MessageBox.Show("ID: " + tmpID.ToString() + ", Typ: " + tmpTyp + ", TypEng: " + tmpTypEng);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Brak połączenia z serwerem lokalnym.");
                        return result;
                    }



                    using var command = new SqlCommand(query, connection);
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = await reader.IsDBNullAsync(i)
                                ? null
                                : reader.GetValue(i);

                            row[reader.GetName(i)] = value!;
                        }

                        result.Add(row);
                    }

                    connection.Close();
                }

                return result;
            }catch(Exception e){
                Debug.WriteLine(e);
            }

            return result;

        }
        private void NewOrders_Click(object sender, RoutedEventArgs e)
        {
            if(Connection.IsConnected)
                AjramStaticData.DoInitializeAjramData();

            var gotoWindow = new NewOrderWindow();

            gotoWindow.Show();

            Close(); // or this.Close();
        }
        private void UpdateData_Click(object sender, RoutedEventArgs e)
        {
            var updateWindow = new UpdateDataWindow();

            updateWindow.Show();

            Close(); // or this.Close();
        }
        private void ListOfOrder_Click(object sender, RoutedEventArgs e)
        {
            if(Connection.IsConnected)
                AjramStaticData.DoInitializeAjramData();

            var gotoWindow = new OrderListWindow();

            gotoWindow.Show();

            Close(); // or this.Close();
        }
        private void OpenLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow { Owner = this };
            bool? result = loginWindow.ShowDialog();

            if (result == true && CurrentUserService.Instance.User != null)
            {
                IsUserLoggedIn = true;
                ShowAvatar();
            }
        }

        private void ShowLoginButton()
        {
            BtnLogin.Visibility = Visibility.Visible;
            LoginStackPanel.Visibility = Visibility.Collapsed;
        //    DropdownMenu.Visibility = Visibility.Collapsed;
        }

        private void ShowAvatar()
        {
            BtnLogin.Visibility = Visibility.Collapsed;
            LoginStackPanel.Visibility = Visibility.Visible;

            var letter = CurrentUserService.Instance.GetAvatarLetter() ?? "Niezalogowany";
            TxtAvatarLetter.Text = letter;
        }

    /*    private void Avatar_Click(object sender, MouseButtonEventArgs e)
        {
            DropdownMenu.Visibility = DropdownMenu.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        } */

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUserService.Instance.Logout();
            IsUserLoggedIn = false;
            ShowLoginButton();
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
     
     
     /*       if (DropdownMenu.Visibility == Visibility.Visible &&
                !DropdownMenu.IsMouseOver && !TxtAvatarLetter.IsMouseOver)
            {
                DropdownMenu.Visibility = Visibility.Collapsed;
            } */
        }
    }
}