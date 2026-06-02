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
using System.Windows.Controls;
using System.ComponentModel.DataAnnotations;
using CustomExtensionMembers;
using System.Security.Cryptography;
using Microsoft.VisualBasic;

namespace ModernWpfApp
{
    public partial class OrderListWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<KlientWithOrderCount> cbKlienciByOrders { get; set; } = new ObservableCollection<KlientWithOrderCount>();
        
        private int _totalOrderCount = 0;
        public int TotalOrderCount 
        {
            get => _totalOrderCount;
            set
            {
                if (_totalOrderCount != value)
                {
                    _totalOrderCount = value;
                    OnPropertyChanged(nameof(TotalOrderCount));
                }
            }
        }
        public OrderListWindow()
        {
            DeclareOrderAsync();
            InitializeComponent();
            DataContext = this;
        }
        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
     
     
     /*       if (DropdownMenu.Visibility == Visibility.Visible &&
                !DropdownMenu.IsMouseOver && !TxtAvatarLetter.IsMouseOver)
            {
                DropdownMenu.Visibility = Visibility.Collapsed;
            } */
        }
        
        public async void DeclareOrderAsync()
        {
            await OrdersStore.Instance.FetchPendingOrdersAsync();
            await OrdersStore.Instance.FetchNonPendingOrdersAsync();
            foreach(var eorder in OrdersStore.Instance.Orders)
            {
                var fl = cbKlienciByOrders.FirstOrDefault(k => k.Firma == eorder.User?.Name);
                if(fl != null)
                    fl.iCount++;
                else
                    cbKlienciByOrders.Add(new KlientWithOrderCount(){Firma = eorder.User?.Name, iCount = 1});
            }
            foreach(var eorder in OrdersStore.Instance.NonPendingOrders)
            {
                var fl = cbKlienciByOrders.FirstOrDefault(k => k.Firma == eorder.User?.Name);
                if(fl != null)
                    fl.iCount++;
                else
                    cbKlienciByOrders.Add(new KlientWithOrderCount(){Firma = eorder.User?.Name, iCount = 1});
            }
            TotalOrderCount = OrdersStore.Instance.Orders.Count + OrdersStore.Instance.NonPendingOrders.Count;
            cbKlienciByOrders.Add(new KlientWithOrderCount(){Firma = "Wszyscy", iCount = TotalOrderCount});
        }
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}