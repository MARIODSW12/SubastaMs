using MongoDB.Bson;

namespace Subasta.Infrastructure.Interfaces
{
    public interface IReadPrizeClaimRepository
    {
        Task CreatePrizeClaim(BsonDocument prizeClaim);
        Task<BsonDocument?> GetPrizeClaimById(string id);
        Task<BsonDocument?> GetPrizeClaimByUserAndAuction(string auctionId, string userId);

    }
}
