﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcommerceApiScrapingService.Repositories
{
    public interface IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        string Id { get; set; }
    }
}
