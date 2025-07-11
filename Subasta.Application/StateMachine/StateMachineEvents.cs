using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Subasta.Application.StateMachine
{
    public class AuctionCreated
    {
        public Guid AuctionId { get; }

        [JsonConstructor]
        public AuctionCreated(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }

    public class AuctionStarted
    {
        public Guid AuctionId { get; }

        [JsonConstructor]
        public AuctionStarted(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }

    public class AuctionFinished
    {
        public Guid AuctionId { get; }

        [JsonConstructor]
        public AuctionFinished(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }

    public class AuctionDeserted
    {
        public Guid AuctionId { get; }

        [JsonConstructor]
        public AuctionDeserted(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }

    public class AuctionCanceled
    {
        public Guid AuctionId { get; }

        [JsonConstructor]
        public AuctionCanceled(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }

    public class AuctionActived
    {
        public Guid AuctionId { get; }

        [JsonConstructor]
        public AuctionActived(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }

    public class PaymentReceived
    {
        public Guid AuctionId { get; }

        [JsonConstructor]
        public PaymentReceived(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }

    public class PrizeDelivered
    {
        public Guid AuctionId { get; }

        [JsonConstructor]
        public PrizeDelivered(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }

}
