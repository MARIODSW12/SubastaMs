using Subasta.Domain.Aggregates;
using Subasta.Domain.ValueObjects;

namespace Subasta.Domain.Factories
{
    public static class AuctionFactory
    {
        public static Auction Create(VOId id, VOUserId userId, VOName name, VODescription description, VOBasePrice basePrice,
            VODuration duration, VOMinimumIncrease minimumIncrease, VOReservePrice reservePrice, VOStartDate startDate ,VOStatus status,
            VOProductId productId, VOProductQuantity productQuantity)
        {
            return new Auction(id, userId, name, description, basePrice, duration, minimumIncrease, reservePrice, startDate, status, productId, productQuantity);
        }
    }
}
