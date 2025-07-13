using EcommerceApiScrapingService.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcommerceApiScrapingService.Models
{
    public class AccountToken : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; }
        public string Cookie { get; set; }
        public string UserAgent { get; set; }
        public string Csrftoken { get; set; }
        public string SPC_CDS { get; set; }
        public string SPC_CDS_VER { get; set; }
        public string XSapSec { get; set; }

        // Lưu nguyên map cookie name→value
        public Dictionary<string, string> Cookies { get; set; }

        // Lưu thô JSON của tất cả headers (cả cookie + userAgent + others)
        public string RawHeadersJson { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
    }
}
