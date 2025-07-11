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
    public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, string>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CreateAuctionCommandHandler));

        public CreateAuctionCommandHandler(IAuctionRepository auctionRepository, IPublishEndpoint publishEndpoint)
        {
            _auctionRepository = auctionRepository ?? throw new ArgumentNullException(nameof(auctionRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(_publishEndpoint));
        }

        public async Task<string> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
        {
            _logger.Info($"Procesando comando CreateAuction para {request.AuctionDto.UserId}");

            try
            {
                var auctionId = Guid.NewGuid().ToString();
                var auction = AuctionFactory.Create(
                    new VOId(auctionId),
                    new VOUserId(request.AuctionDto.UserId),
                    new VOName(request.AuctionDto.Name),
                    new VODescription(request.AuctionDto.Description),
                    new VOBasePrice(request.AuctionDto.BasePrice),
                    new VODuration(request.AuctionDto.Duration),
                    new VOMinimumIncrease(request.AuctionDto.MinimumIncrease),
                    new VOReservePrice(request.AuctionDto.ReservePrice),
                    new VOStartDate(request.AuctionDto.StartDate),
                    new VOStatus("pending"),
                    new VOProductId(request.AuctionDto.ProductId),
                    new VOProductQuantity(request.AuctionDto.ProductQuantity)
                    );

                await _auctionRepository.CreateAuction(auction);

                var auctionCreatedEvent = new AuctionCreatedEvent(
                    auction.Id.Value, auction.UserId.Value, auction.Name.Value, auction.Description.Value, auction.BasePrice.Value,
                    auction.Duration.Value, auction.MinimumIncrease.Value, auction.ReservePrice.Value, auction.StartDate.Value, auction.Status.Value,
                    auction.ProductId.Value, auction.ProductQuantity.Value
                );
                await _publishEndpoint.Publish(new AuctionCreated(Guid.Parse(auctionId)));
                await _publishEndpoint.Publish(auctionCreatedEvent);
                _logger.Info($"Evento publicado: Subasta {auction.Id.Value} creada.");

                return auction.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al crear Subasta {request.AuctionDto.Name}", ex);
                throw;
            }
        }
    }
}