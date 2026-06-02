using System.Collections.ObjectModel;
using System.ComponentModel;

using System.Windows;
using System.Windows.Input;
using ModernWpfApp.Services;
using System.Diagnostics;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Net.Http;
using ModernWpfApp.Utils;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ModernWpfApp
{
    public partial class UpdateDataWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Expose the singleton so XAML can bind to Connection.IsConnected
        // Dummy list to prevent errors if OrdersStore isn't fully set up yet
        public UpdateDataWindow()
        {
            InitializeComponent();
            DataContext = this;
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
                        MessageBox.Show("Count: " + ds.Tables["TmpCheckSQL"].Rows.Count);
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
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return result;

        }
        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
        private async void OnUpdateSlizgacze_Click(object sender, RoutedEventArgs e)
        {
            var data = await LoadTableDynamicAsync("SELECT * FROM dbo.DetaleSlizgacz;");

            var payload = new
            {
                items = data.Select(x => new
                {
                    typ = x["Typ"]?.ToString(),
                    extId = (int) x["ID"],
                    typEng = x["TypEng"]?.ToString()
                }).ToList()
            };

            var client = new HttpClient();
            var token = JwtStorage.LoadToken();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync(
                ApiConfig.BaseUrl + "/slider/recreate",
                payload
            );

            var result = await response.Content.ReadAsStringAsync();
            MessageBox.Show(result);
        }
        private async void OnUpdateKolory_Click(object sender, RoutedEventArgs e)
        {
            var data = await LoadTableDynamicAsync("SELECT * FROM dbo.Kolory;");

            var payload = new
            {
                items = data.Select(x => new
                {
                    plKolor = x["Kolor"]?.ToString(),
                    extId = (int) x["ID"],
                    engKolor = x["KolorEng"]?.ToString()
                }).ToList()
            };

            var client = new HttpClient();
            var token = JwtStorage.LoadToken();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync(
                ApiConfig.BaseUrl + "/colour/recreate",
                payload
            );

            var result = await response.Content.ReadAsStringAsync();
            MessageBox.Show(result);
        }
        private void OnUpdateKlienci_Click(object sender, RoutedEventArgs e)
        {
            //         LoadTableDynamicAsync("SELECT * FROM dbo.Kolory;");
        }
        private void OnUpdateKlienciAdresy_Click(object sender, RoutedEventArgs e)
        {
            //         LoadTableDynamicAsync("SELECT * FROM dbo.Kolory;");
        }
        private async void OnUpdateDrewno_Click(object sender, RoutedEventArgs e)
        {
            var data = await LoadTableDynamicAsync("SELECT * FROM dbo.Gatunek_Drewna WHERE EOrder != 0;");

            var payload = new
            {
                items = data.Select(x => new
                {
                    plGatunek = x["Gatunek"]?.ToString(),
                    extId = (int) x["ID"],
                    engGatunek = x["EngName"]?.ToString()
                }).ToList()
            };

            var client = new HttpClient();
            var token = JwtStorage.LoadToken();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync(
                ApiConfig.BaseUrl + "/wood/recreate",
                payload
            );

            var result = await response.Content.ReadAsStringAsync();
            MessageBox.Show(result);
        }
        private async void OnUpdateChairs_Click(object sender, RoutedEventArgs e)
        {
            var data = await LoadTableDynamicAsync("SELECT * FROM dbo.Modele_Krzesla_Drewniane WHERE EOrder != 0;");

            var payload = new
            {
                type = "CHAIR",
                items = data.Select(x => new
                {
                    name = x["Model"]?.ToString(),
                    extId = x["ID"]?.ToString()
                }).ToList()
            };

            var client = new HttpClient();
            var token = JwtStorage.LoadToken();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync(
                ApiConfig.BaseUrl + "/furniture/recreate",
                payload
            );

            var result = await response.Content.ReadAsStringAsync();
            MessageBox.Show(result);
        }
        private async void OnUpdateTables_Click(object sender, RoutedEventArgs e)
        {
            var data = await LoadTableDynamicAsync("SELECT * FROM dbo.Modele_Stoly_Drewniane WHERE EOrder != 0;");

            var payload = new
            {
                type = "TABLE",
                items = data.Select(x => new
                {
                    name = x["Model"]?.ToString(),
                    extId = x["ID"]?.ToString()
                }).ToList()
            };

            var client = new HttpClient();
            var token = JwtStorage.LoadToken();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync(
                ApiConfig.BaseUrl + "/furniture/recreate",
                payload
            );

            var result = await response.Content.ReadAsStringAsync();
            MessageBox.Show(result);
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