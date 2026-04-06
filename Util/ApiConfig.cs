using System;

namespace ModernWpfApp.Utils
{
    public static class ApiConfig
    {
        private static readonly bool IsProd = false;

        private static readonly string DevBaseUrl = "http://localhost:3000/api";
        private static readonly string ProdBaseUrl = "https://your-prod-domain.com/api";

        public static string BaseUrl => IsProd ? ProdBaseUrl : DevBaseUrl;

        public static string LoginUrl => $"{BaseUrl}/users/login";
        public static string PendingOrdersUrl => $"{BaseUrl}/orders/pending";
    }
}