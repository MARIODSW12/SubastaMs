using Subasta.Domain.Entities;

namespace Subasta.Domain.Repositories
{
    public interface IPrizeClaimRepository
    {
        Task CreatePrizeClaim(PrizeClaimInfo prizeClaim);
        Task<PrizeClaimInfo?> GetPrizeClaimById(string id);
    }
}
