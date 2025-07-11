using MongoDB.Bson.Serialization.Attributes;
using Subasta.Domain.ValueObjects;

namespace Usuarios.Infrastructure.Persistence.Repository.MongoRead.Documents
{
    public class AuctionMongoRead
    {
        [BsonId]
        [BsonElement("id")]
        public required string Id { get; set; }

        [BsonElement("userId")]
        public required string UserId { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("description")]
        public required string Description { get; set; }

        [BsonElement("image")]
        public required string Image { get; set; }

        [BsonElement("basePrice")]
        public required decimal BasePrice { get; set; }

        [BsonElement("duration")]
        public required int Duration { get; set; }

        [BsonElement("minimumIncrease")]
        public required decimal MinimumIncrease { get; set; }

        [BsonElement("reservePrice")]
        public required decimal ReservePrice { get; set; }

        [BsonElement("startDate")]
        public required DateTime StartDate { get; set; }

        [BsonElement("status")]
        public required string Status { get; set; }

    }
}