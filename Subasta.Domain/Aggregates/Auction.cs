using Subasta.Domain.Exceptions;
using Subasta.Domain.ValueObjects;

namespace Subasta.Domain.Aggregates
{
    public class Auction
    {
        public VOId Id { get; private set; }
        public VOUserId UserId { get; private set; }
        public VOProductId ProductId { get; private set; }
        public VOProductQuantity ProductQuantity { get; private set; }
        public VOName Name {  get; private set; }
        public VODescription Description { get; private set; }
        public VOBasePrice BasePrice { get; private set; }
        public VODuration Duration { get; private set; }
        public VOMinimumIncrease MinimumIncrease { get; private set; }
        public VOReservePrice ReservePrice { get; private set; }
        public VOStartDate StartDate { get; private set; }
        public VOStatus Status { get; private set; }


        public Auction(VOId id, VOUserId userId, VOName name, VODescription description, VOBasePrice basePrice,
            VODuration duration, VOMinimumIncrease minimumIncrease, VOReservePrice reservePrice, VOStartDate startDate, VOStatus status, VOProductId productId, VOProductQuantity productQuantity)
        {
            Id = id;
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
            Validate();
        }

        public void Update(string? name, string? description, decimal? basePrice,
            int? duration, decimal? minimumIncrease, decimal? reservePrice, DateTime? startDate,
            string? productId, int? productQuantity)
        {
            if (name != null)
                this.Name = new VOName(name);
            if (description != null) this.Description = new VODescription(description);
            if (basePrice != null) this.BasePrice = new VOBasePrice(basePrice.Value);
            if (duration != null) this.Duration = new VODuration(duration.Value);
            if (minimumIncrease != null) this.MinimumIncrease = new VOMinimumIncrease(minimumIncrease.Value);
            if (reservePrice != null) this.ReservePrice = new VOReservePrice(reservePrice.Value);
            if (startDate != null) this.StartDate = new VOStartDate(startDate.Value);
            if (productId != null) this.ProductId = new VOProductId(productId);
            if (productQuantity !=  null) this.ProductQuantity = new VOProductQuantity(productQuantity.Value);
        }

        public void ChangeStatus(VOStatus status)
        {
            Status = status;
        }

        private void Validate()
        {
            if (this.BasePrice.Value >= this.ReservePrice.Value)
                throw new InvalidAuctionPricesException();
        }
    }
}