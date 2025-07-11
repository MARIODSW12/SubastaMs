using MediatR;

namespace Subasta.Domain.Events
{
    public class AuctionCreatedEvent : INotification
    {
        public string AuctionId { get; }
        public string UserId { get; }
        public string ProductId { get; }
        public int ProductQuantity { get; }
        public string Name { get; }
        public string Description { get; }
        public decimal BasePrice { get; }
        public int Duration { get; private set; }
        public decimal MinimumIncrease { get; private set; }
        public decimal ReservePrice { get; private set; }
        public string Status { get; private set; }
        public DateTime StartDate { get; private set; }

        public AuctionCreatedEvent(string auctionId, string userId, string name, string description, decimal basePrice,
            int duration, decimal minimumIncrease, decimal reservePrice, DateTime startDate, string status, string productId, int productQuantity)
        {
            AuctionId = auctionId;
            UserId = userId;
            Name = name;
            Description = description;
            BasePrice = basePrice;
            Duration = duration;
            MinimumIncrease = minimumIncrease;
            ReservePrice = reservePrice;
            StartDate = startDate;
            Status = status;
            ProductId = productId;
            ProductQuantity = productQuantity;
        }
    }
}
