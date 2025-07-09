namespace EcommerceApiScrapingService.Models
{
    public class AccountToken
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Cookie { get; set; }
        public string UserAgent { get; set; }
        public string Csrftoken { get; set; }
        public string SPC_CDS { get; set; }
        public string SPC_CDS_VER { get; set; }
        public string XSapSec { get; set; }
        public string RawHeadersJson { get; set; }  // Optional: lưu luôn JSON header nếu muốn
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }    // Nếu cần lưu expired time
    }
}
