
namespace Subasta.Application.DTOs
{
    public class CreateAuctionDto
    {
        public string UserId { get; init; }
        public string Name { get; init; }
        public string ProductId { get; init; }
        public int ProductQuantity { get; init; }
        public string Description { get; init; }
        public decimal BasePrice { get; init; }
        public int Duration { get; init; }
        public decimal MinimumIncrease { get; init; }
        public decimal ReservePrice { get; init; }
        public DateTime StartDate { get; init; }
    }
}
