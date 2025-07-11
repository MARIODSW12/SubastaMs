using MediatR;

namespace Subasta.Domain.Events
{
    public class AuctionDeletedEvent : INotification
    {
        public string AuctionId { get; }
        public string UserId { get; }
        public AuctionDeletedEvent(string auctionId, string userId)
        {
            AuctionId = auctionId;
            UserId = userId;
        }
    }
}
