using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Application.Commands
{

    public class CreateAuctionCommand : IRequest<String>
    {
        public CreateAuctionDto AuctionDto { get; }

        public CreateAuctionCommand(CreateAuctionDto auctionDto)
        {
            AuctionDto = auctionDto ?? throw new ArgumentNullException(nameof(auctionDto));
        }
    }
}