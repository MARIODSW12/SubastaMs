using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Infrastructure.Queries
{
    public class GetAuctionByIdQuery : IRequest<GetAuctionDto>
    {
        public string Id { get; set; }

        public GetAuctionByIdQuery(string id)
        {
            Id = id;
        }
    }
}
