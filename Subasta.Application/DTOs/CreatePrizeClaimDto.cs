
using Subasta.Domain.Enums;

namespace Subasta.Application.DTOs
{
    public class CreatePrizeClaimDto
    {
        public string UserId { get; init; }
        public string AuctionId { get; init; }
        public string DeliverDirection { get; init; }
        public string DeliverMethod { get; init; }
    }
}
