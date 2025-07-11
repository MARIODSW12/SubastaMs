using MediatR;

namespace Subasta.Domain.Events
{
    public class AuctionStatusChangedEvent : INotification
    {
        public string AuctionId { get; }
        public string UserId { get; }
        public string Status { get; }
        public AuctionStatusChangedEvent(string auctionId, string status, string userId)
        {
            AuctionId = auctionId;
            Status = status;
            UserId = userId;
        }
    }
}
