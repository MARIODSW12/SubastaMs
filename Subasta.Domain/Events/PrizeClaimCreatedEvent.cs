using MediatR;
using Subasta.Domain.Enums;

namespace Subasta.Domain.Events
{
    public class PrizeClaimCreatedEvent : INotification
    {
        public string PrizeId { get;  }
        public string UserId { get; }
        public string AuctionId { get;  }
        public string DeliverDirection { get;  }
        public string DeliverMethod { get;  }
        public DateTime ClaimDate { get;  }

        public PrizeClaimCreatedEvent(
            string userId,
            string auctionId,
            string deliverDirection,
            string deliverMethod,
            DateTime claimDate,
            string prizeId)
        {
            UserId = userId;
            AuctionId = auctionId;
            DeliverDirection = deliverDirection;
            DeliverMethod = deliverMethod;
            ClaimDate = claimDate;
            PrizeId = prizeId;
        }
    }
}
