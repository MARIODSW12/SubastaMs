using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Application.Commands
{

    public class UpdateAuctionCommand : IRequest<String>
    {
        public UpdateAuctionDto AuctionDto { get; }

        public UpdateAuctionCommand(UpdateAuctionDto auctionDto)
        {
            AuctionDto = auctionDto ?? throw new ArgumentNullException(nameof(auctionDto));
        }
    }
}
