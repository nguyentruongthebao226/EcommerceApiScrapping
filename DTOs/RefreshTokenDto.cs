namespace EcommerceApiScrapingService.DTOs
{
    public class RefreshTokenDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiredAt { get; set; }
    }
}
