using EcommerceApiScrapingService.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcommerceApiScrapingService.Models
{
    public class ProductCloned : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
