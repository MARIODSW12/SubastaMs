using MediatR;

namespace Subasta.Domain.Events
{
    public class AuctionUpdatedEvent : INotification
    {
        public string AuctionId { get; }
        public string UserId { get; }
        public string ProductId { get; }
        public int ProductQuantity { get; }
        public string Name { get; }
        public string Description { get; }
        public decimal BasePrice { get; }
        public int Duration { get; }
        public decimal MinimumIncrease { get; }
        public decimal ReservePrice { get; }
        public DateTime StartDate { get; }
        public AuctionUpdatedEvent(string auctionId, string userId, string name, string description, decimal basePrice,
            int duration, decimal minimumIncrease, decimal reservePrice, DateTime startDate, string productId, int productquantity)
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
            ProductId = productId;
            ProductQuantity = productquantity;
        }
    }
}
