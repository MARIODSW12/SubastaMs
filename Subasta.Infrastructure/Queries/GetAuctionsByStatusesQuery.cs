using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Infrastructure.Queries
{
    public class GetAuctionsByStatusesQuery : IRequest<List<GetAuctionDto>>
    {
        public List<string> Statuses { get; set; }
        public GetAuctionsByStatusesQuery(List<string> statuses)
        {
            Statuses = statuses;
        }
    }
}
