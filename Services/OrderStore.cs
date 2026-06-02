using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows.Input;
using ModernWpfApp.Utils;

namespace ModernWpfApp.Services
{
    public class OrdersStore
    {
        private static OrdersStore? _instance;
        public static OrdersStore Instance => _instance ??= new OrdersStore();

        private OrdersStore()
        {
            Orders = new ObservableCollection<OrderData>();
            NonPendingOrders = new ObservableCollection<OrderData>();
        }

        private readonly HttpClient _client = new HttpClient();
        public ObservableCollection<OrderData> Orders { get; }
        public ObservableCollection<OrderData> NonPendingOrders { get; }
        
        private void SetAuthorizationHeader()
        {
            var token = JwtStorage.LoadToken();
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task FetchNonPendingOrdersAsync()
        {
            SetAuthorizationHeader();
            try
            {
                var response = await _client.GetAsync($"{ApiConfig.BaseUrl}/orders/getAllNonPending");
                if (!response.IsSuccessStatusCode) return;

                var orders = await response.Content.ReadFromJsonAsync<OrderData[]>();
                if (orders == null) return;
                
                NonPendingOrders.Clear();
                foreach (var order in orders)
                {
                    order.SetupCommands(); 
                    NonPendingOrders.Add(order);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch orders: " + ex.Message);
            }
        }
        public async Task FetchPendingOrdersAsync()
        {
            SetAuthorizationHeader();
            try
            {
                var response = await _client.GetAsync($"{ApiConfig.BaseUrl}/orders/getAllPending");
                if (!response.IsSuccessStatusCode) return;

                var orders = await response.Content.ReadFromJsonAsync<OrderData[]>();
                if (orders == null) return;
                
                Orders.Clear();
                foreach (var order in orders)
                {
                    order.SetupCommands(); 
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch orders: " + ex.Message);
            }
        }

        public async Task<bool> CompleteOrderAsync(OrderData order, string EsteticaOrderId = "")
        {
            if (order == null) return false;

            SetAuthorizationHeader();
            try
            {
                var response = await _client.PostAsJsonAsync($"{ApiConfig.BaseUrl}/orders/complete",
                    new { orderId = order.Id, workerId = CurrentUserService.Instance.User?.Id, esteticaOrderId = EsteticaOrderId});

                if (response.IsSuccessStatusCode)
                {
                    Orders.Remove(order);
                    return true;
                }
                else
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Complete failed: " + msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Complete request failed: " + ex.Message);
                return false;
            }
        }
        public async Task<bool> RejectOrderAsync(OrderData order)
        {
            if (order == null) return false;

            SetAuthorizationHeader();
            try
            {
                var response = await _client.PostAsJsonAsync($"{ApiConfig.BaseUrl}/orders/reject",
                    new { orderId = order.Id, workerId = CurrentUserService.Instance.User?.Id });

                if (response.IsSuccessStatusCode)
                {
                    Orders.Remove(order);
                    return true;
                }
                else
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Reject failed: " + msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Reject request failed: " + ex.Message);
                return false;
            }
        }
        public async Task<bool> CancelOrderAsync(OrderData order)
        {
            if (order == null) return false;

            SetAuthorizationHeader();
            try
            {
                var response = await _client.PostAsJsonAsync($"{ApiConfig.BaseUrl}/orders/cancel",
                    new { orderId = order.Id });

                if (response.IsSuccessStatusCode)
                {
                    Orders.Remove(order);
                    return true;
                }
                else
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Cancel failed: " + msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cancel request failed: " + ex.Message);
                return false;
            }
        }

        public void ClearOrders() => Orders.Clear();
    }
    public class OrderData
    {
        public string ajramOrderId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? WorkerIdOfCompletingAssigning { get; set; }
        public UserDataIn? Worker { get; set; }
        public UserDataIn? User { get; set; }
        public List<FurnitureInOrderData> FurnitureInOrders { get; set; } = new();

    /*    public string FurnitureSummary =>
            FurnitureInOrders == null || FurnitureInOrders.Count == 0
                ? ""
                : string.Join(", ", FurnitureInOrders.Select(f => $"{f.Furniture?.Name ?? "Unknown"} x{f.Quantity}"));
        */

/*        public ICommand? CompleteCommand { get; private set; }
        public ICommand? CancelCommand { get; private set; }
        public ICommand? RejectCommand { get; private set; } */

        public void SetupCommands()
        {
/*            CompleteCommand = new AsyncRelayCommand(async (string s) =>
            {
                await OrdersStore.Instance.CompleteOrderAsync(this, s);
            });

            CancelCommand = new AsyncRelayCommand(async () =>
            {
                await OrdersStore.Instance.CancelOrderAsync(this);
            });

            RejectCommand = new AsyncRelayCommand(async () =>
            {
                await OrdersStore.Instance.RejectOrderAsync(this);
            }); */
        }
    }

    public class FurnitureInOrderData
    {
        public string id { get; set; } = string.Empty;
        public string productType { get; set; } = string.Empty;
        public string furnitureName { get; set; } = string.Empty;
        public int woodType { get; set; }
        public int quantity { get; set; }
        public string userOrderId { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string fabricType { get; set; } = string.Empty;
        public string fabric { get; set; } = string.Empty;
        public bool fabricSend { get; set; } = false;
        public string colour { get; set; } = string.Empty;
        public int height { get; set; }
        public int slider { get; set; }
        public string extrainfo { get; set; } = string.Empty;
        public string orderId { get; set; } = string.Empty;
    }
    public class UserDataIn
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "USER";
    }
}