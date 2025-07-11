using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Infrastructure.Queries
{
    public class GetAuctionsByStatusQuery : IRequest<List<GetAuctionDto>>
    {
        public string Status { get; set; }
        public GetAuctionsByStatusQuery(string status)
        {
            Status = status;
        }
    }
}
