using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Infrastructure.Queries
{
    public class GetProductAuctionsQuery : IRequest<List<GetAuctionDto>>
    {
        public string ProductId { get; set; }

        public GetProductAuctionsQuery(string productId)
        {
            ProductId = productId;
        }
    }
}
