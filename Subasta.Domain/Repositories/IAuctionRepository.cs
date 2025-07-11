using Subasta.Domain.Aggregates;

namespace Subasta.Domain.Repositories
{
    public interface IAuctionRepository
    {
        Task CreateAuction(Auction auction);
        Task UpdateAuction(Auction auction);
        Task UpdateAuctionStatus(string auctionId, string newStatus);
        Task<long> DeleteAuction(string auctionId, string userId);
        Task<Auction?> GetAuctionById(string id);

    }
}
