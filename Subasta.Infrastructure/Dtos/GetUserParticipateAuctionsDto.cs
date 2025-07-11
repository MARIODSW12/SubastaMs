using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Subasta.Application.DTOs;

namespace Subasta.Infrastructure.Dtos
{
    public class GetUserParticipateAuctionsDto
    {
        public string Id { get; init; }
        public string UserId { get; init; }
        public string ProductId { get; init; }
        public string ProductName { get; init; }
        public int ProductQuantity { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public string Status { get; init; }
        public decimal BasePrice { get; init; }
        public int Duration { get; init; }
        public decimal MinimumIncrease { get; init; }
        public decimal ReservePrice { get; init; }
        public DateTime StartDate { get; init; }
        public GetBidDto WinnerBid { get; init; }
        public List<GetBidDto> Bids { get; init; }

    }
}
