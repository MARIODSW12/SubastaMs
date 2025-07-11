using MediatR;
using log4net;

using Subasta.Application.Commands;

using Subasta.Domain.Repositories;
using Subasta.Domain.ValueObjects;
using Subasta.Domain.Aggregates;
using Subasta.Domain.Events;
using Subasta.Domain.Factories;
using MassTransit;
using MassTransit.Transports;
using Subasta.Application.StateMachine;

namespace Subasta.Application.Handlers
{
    public class UpdateAuctionCommandHandler : IRequestHandler<UpdateAuctionCommand, string>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateAuctionCommandHandler(IAuctionRepository auctionRepository, IPublishEndpoint publishEndpoint)
        {
            _auctionRepository = auctionRepository ?? throw new ArgumentNullException(nameof(auctionRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(_publishEndpoint));
        }

        public async Task<string> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var dto = request.AuctionDto;
                var auction = await _auctionRepository.GetAuctionById(dto.AuctionId);
                auction.Update(dto.Name, dto.Description, dto.BasePrice, dto.Duration,
                    dto.MinimumIncrease, dto.ReservePrice, dto.StartDate, dto.ProductId, dto.ProductQuantity);

                await _auctionRepository.UpdateAuction(auction);

                var auctionUpdatedEvent = new AuctionUpdatedEvent(
                    auction.Id.Value, auction.UserId.Value, auction.Name.Value, auction.Description.Value, auction.BasePrice.Value,
                    auction.Duration.Value, auction.MinimumIncrease.Value, auction.ReservePrice.Value, auction.StartDate.Value,
                    auction.ProductId.Value, auction.ProductQuantity.Value
                );
                await _publishEndpoint.Publish(auctionUpdatedEvent);

                return auction.Id.Value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}