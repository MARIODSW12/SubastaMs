using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Infrastructure.Queries
{
    public class GetPrizeClaimByUserAndAuctionQuery : IRequest<GetPrizeClaimDto?>
    {
        public string UserId { get; set; }
        public string AuctionId { get; set; }

        public GetPrizeClaimByUserAndAuctionQuery( string userId, string auctionId) {
            UserId = userId;
            AuctionId = auctionId;
        }
    }
}
