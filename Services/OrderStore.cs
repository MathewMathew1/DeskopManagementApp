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
        }

        private readonly HttpClient _client = new HttpClient();
        public ObservableCollection<OrderData> Orders { get; }

        private void SetAuthorizationHeader()
        {
            var token = JwtStorage.LoadToken();
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

        public async Task<bool> CompleteOrderAsync(OrderData order)
        {
            if (order == null) return false;

            SetAuthorizationHeader();
            try
            {
                var response = await _client.PostAsJsonAsync($"{ApiConfig.BaseUrl}/orders/complete",
                    new { orderId = order.Id, workerId = order.Worker?.Id });

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
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? UserOrderId { get; set; }
        public string IsCompleted { get; set; } = "PENDING";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? WorkerIdOfCompletingAssigning { get; set; }
        public UserDataIn? Worker { get; set; }
        public UserDataIn? User { get; set; }
        public List<FurnitureInOrderData> FurnitureInOrders { get; set; } = new();

        public string FurnitureSummary =>
            FurnitureInOrders == null || FurnitureInOrders.Count == 0
                ? ""
                : string.Join(", ", FurnitureInOrders.Select(f => $"{f.Furniture?.Name ?? "Unknown"} x{f.Quantity}"));

        public ICommand? CompleteCommand { get; private set; }
        public ICommand? CancelCommand { get; private set; }

        public void SetupCommands()
        {
            CompleteCommand = new AsyncRelayCommand(async () =>
            {
                await OrdersStore.Instance.CompleteOrderAsync(this);
            });

            CancelCommand = new AsyncRelayCommand(async () =>
            {
                await OrdersStore.Instance.CancelOrderAsync(this);
            });
        }
    }

    public class FurnitureInOrderData
    {
        public string Id { get; set; } = string.Empty;
        public string FurnitureId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string WoodType { get; set; } = "OAK";
        public FurnitureData? Furniture { get; set; }
    }

    public class FurnitureData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "CHAIR";
    }

    public class UserDataIn
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "USER";
    }
}