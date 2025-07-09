namespace EcommerceApiScrapingService.DTOs
{
    public class AccountCreateDto
    {
        public string Username { get; set; }
        public string ShopeeUserId { get; set; }
        public string ShopId { get; set; }
        public string ShopeeToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? TokenExpiredAt { get; set; }
        public string Note { get; set; }
        public string ShopName { get; set; }
        public string Country { get; set; }
        public string Platform { get; set; } = "Shopee";
        public bool IsHost { get; set; } = false;

    }
}
