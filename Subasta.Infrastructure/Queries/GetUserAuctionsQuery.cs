using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Infrastructure.Queries
{
    public class GetUserAuctionsQuery : IRequest<List<GetAuctionDto>>
    {
        public string UserId { get; set; }

        public GetUserAuctionsQuery(string userId)
        {
            UserId = userId;
        }
    }
}
