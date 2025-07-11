
namespace Subasta.Application.DTOs
{
    public class GetBidDto
    {
        public string id { get; init; }
        public string userId { get; init; }
        public string auctionId { get; init; }
        public decimal price { get; init; }
        public DateTime date { get; init; }
    }
}
