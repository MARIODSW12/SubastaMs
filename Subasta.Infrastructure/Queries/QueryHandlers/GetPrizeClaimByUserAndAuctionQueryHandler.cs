using MediatR;
using log4net;

using Subasta.Application.DTOs;

using Subasta.Infrastructure.Interfaces;
using MongoDB.Bson;

namespace Subasta.Infrastructure.Queries.QueryHandlers
{
    public class GetPrizeClaimByUserAndPrizeClaimQueryHandler : IRequestHandler<GetPrizeClaimByUserAndAuctionQuery, GetPrizeClaimDto?>
    {
        private readonly IReadPrizeClaimRepository _prizeClaimReadRepository;

        public GetPrizeClaimByUserAndPrizeClaimQueryHandler(IReadPrizeClaimRepository prizeClaimReadRepository)
        {
            _prizeClaimReadRepository = prizeClaimReadRepository ?? throw new ArgumentNullException(nameof(prizeClaimReadRepository));
        }

        public async Task<GetPrizeClaimDto?> Handle(GetPrizeClaimByUserAndAuctionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var prizeClaim = await _prizeClaimReadRepository.GetPrizeClaimByUserAndAuction(request.AuctionId, request.UserId);

                if (prizeClaim == null)
                {
                    return null;
                }

                var prizeClaimDto = new GetPrizeClaimDto
                {
                    Id = prizeClaim["_id"].AsString,
                    UserId = prizeClaim["userId"].AsString,
                    AuctionId = prizeClaim["auctionId"].AsString,
                    DeliverDirection = prizeClaim["deliverDirection"].AsString,
                    DeliverMethod = prizeClaim["deliverMethod"].AsString,
                    ClaimDate = prizeClaim["claimDate"].ToUniversalTime(),
                };

                return prizeClaimDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
