using System.Text.Json.Serialization;

namespace EcommerceApiScrapingService.DTOs
{
    public class ShopeeLoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsHost { get; set; }
    }
}
