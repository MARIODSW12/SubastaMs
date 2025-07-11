
namespace Subasta.Application.DTOs
{
    public class GetAuctionReportDto
    {
        public string Id { get; init; }
        public string ProductName { get; init; }
        public string Name { get; init; }
        public DateTime StartDate { get; init; }
        public List<string>? Participants { get; init; }
        public decimal FinalPrice { get; init; }
    }
}
