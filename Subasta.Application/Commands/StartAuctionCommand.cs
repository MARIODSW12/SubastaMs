using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Application.Commands
{

    public class StartAuctionCommand : IRequest<String>
    {
        public String AuctionId { get; }

        public StartAuctionCommand(String auctionId)
        {
            AuctionId = auctionId ?? throw new ArgumentNullException(nameof(auctionId));
        }
    }
}
