using EcommerceApiScrapingService.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcommerceApiScrapingService.Models
{
    public class Account : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public bool IsHost { get; set; } = false;
        public string Username { get; set; }         // Tên đăng nhập Shopee
        public string ShopeeUserId { get; set; }     // UserId Shopee (nên có, dùng khi call API)
        public string ShopId { get; set; }           // ShopId (bắt buộc khi thao tác API sản phẩm/đơn hàng)
        public string ShopeeToken { get; set; }      // Access token (hoặc session/cookie)
        public string RefreshToken { get; set; }     // Refresh token (nếu sử dụng OAuth Shopee Open Platform)
        public DateTime? TokenExpiredAt { get; set; }// Thời gian token hết hạn (để tự động refresh)
        public string Note { get; set; }             // Ghi chú (nếu cần: shop của ai, nguồn, mô tả...)
        public bool IsActive { get; set; } = true;   // Đánh dấu có đang sử dụng/ẩn shop không

        // Các trường bổ sung khác (nếu bạn cần):
        public string ShopName { get; set; }         // Tên shop hiển thị
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }   // Lần cuối đăng nhập
        public string Country { get; set; }          // Quốc gia (Shopee VN, ID, SG, PH...)
        // Nếu dùng thêm đa nền tảng (Amazon, Yahoo...) có thể bổ sung PlatformType
        public string Platform { get; set; } = "Shopee"; // Shopee, Amazon, Yahoo, v.v.
    }
}
