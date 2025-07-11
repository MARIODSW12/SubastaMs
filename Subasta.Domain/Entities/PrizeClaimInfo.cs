using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Subasta.Domain.ValueObjects;

namespace Subasta.Domain.Entities
{
    public class PrizeClaimInfo
    {
        public VOPrizeId PrizeId { get; private set; }
        public VOPrizeUserId PrizeUserId { get; private set; }
        public VOId AuctionId { get; private set; }
        public VOClaimDate ClaimedDate { get; private set; }
        public VODeliverDirection DeliverDirection { get; private set; }
        public VODeliverMethod DeliverMethod { get; private set; }

        public PrizeClaimInfo(VOPrizeId prizeId, VOPrizeUserId prizeUserId, VOId auctionId, VOClaimDate claimedDate, VODeliverDirection deliverDirection, VODeliverMethod deliverMethod)
        {
            PrizeId = prizeId;
            PrizeUserId = prizeUserId;
            AuctionId = auctionId;
            ClaimedDate = claimedDate;
            DeliverDirection = deliverDirection;
            DeliverMethod = deliverMethod;
        }

    }
}
