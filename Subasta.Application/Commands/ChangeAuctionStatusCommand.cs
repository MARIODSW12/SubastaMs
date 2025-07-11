using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Application.Commands
{

    public class ChangeAuctionStatusCommand : IRequest<String>
    {
        public ChangeAuctionStatusDto AuctionDto { get; }

        public ChangeAuctionStatusCommand(ChangeAuctionStatusDto auctionDto)
        {
            AuctionDto = auctionDto ?? throw new ArgumentNullException(nameof(auctionDto));
        }
    }
}
