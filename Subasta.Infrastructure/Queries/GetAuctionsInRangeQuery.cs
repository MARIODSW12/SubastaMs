using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Infrastructure.Queries
{
    public class GetAuctionsInRangeQuery : IRequest<List<GetAuctionDto>>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public GetAuctionsInRangeQuery( DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }
    }
}
