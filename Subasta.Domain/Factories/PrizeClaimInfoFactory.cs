using Subasta.Domain.Aggregates;
using Subasta.Domain.Entities;
using Subasta.Domain.ValueObjects;

namespace Subasta.Domain.Factories
{
    public static class PrizeClaimInfoFactory
    {
        public static PrizeClaimInfo Create(VOPrizeId prizeId, VOPrizeUserId prizeUserId, VOId auctionId, VOClaimDate claimedDate, VODeliverDirection deliverDirection, VODeliverMethod deliverMethod)
        {
            return new PrizeClaimInfo(prizeId, prizeUserId, auctionId, claimedDate, deliverDirection, deliverMethod);
        }
    }
}