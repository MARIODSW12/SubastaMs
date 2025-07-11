using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Application.Commands
{

    public class DeleteAuctionCommand : IRequest<String>
    {
        public DeleteAuctionDto AuctionDto { get; }

        public DeleteAuctionCommand(DeleteAuctionDto auctionDto)
        {
            AuctionDto = auctionDto ?? throw new ArgumentNullException(nameof(auctionDto));
        }
    }
}
