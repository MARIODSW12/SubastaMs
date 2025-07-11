using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Subasta.Application.DTOs;

namespace Subasta.Infrastructure.Dtos
{
    public class GetUserParticipateBidsDto
    {
        public string Id { get; init; }
        public string UserId { get; init; }
        public string AuctionId { get; init; } 
        public string AuctionName { get; init; }
        public string Status { get; init; }
        public decimal Price { get; init; }
        public DateTime Date { get; init; }

    }
}
