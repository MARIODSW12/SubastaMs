using MongoDB.Bson;

namespace Subasta.Infrastructure.Interfaces
{
    public interface IReadAuctionRepository
    {
        Task CreateAuction(BsonDocument auction);
        Task UpdateAuction(BsonDocument auction);
        Task UpdateAuctionStatus(string auctionId, string newStatus);
        Task DeleteAuction(string auctionId, string userId);
        Task<BsonDocument?> GetAuctionById(string id);
        Task<List<BsonDocument>> GetUserAuctions(string userId);
        Task<List<BsonDocument>> GetAuctionsInRange(DateTime from, DateTime to);
        Task<List<BsonDocument>> GetProductAuctions(string productId);
        Task<List<BsonDocument>> GetAuctionsByStatus(string status);
        Task<List<BsonDocument>> GetAuctionsByStatuses(List<string> statuses);

    }
}
