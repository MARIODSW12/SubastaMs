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
    public class ChangeAuctionStatusCommandHandler : IRequestHandler<ChangeAuctionStatusCommand, string>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ChangeAuctionStatusCommandHandler));

        public ChangeAuctionStatusCommandHandler(IAuctionRepository auctionRepository, IPublishEndpoint publishEndpoint)
        {
            _auctionRepository = auctionRepository ?? throw new ArgumentNullException(nameof(auctionRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(_publishEndpoint));
        }

        public async Task<string> Handle(ChangeAuctionStatusCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var auction = await _auctionRepository.GetAuctionById(request.AuctionDto.AuctionId);

                auction.ChangeStatus(new VOStatus(request.AuctionDto.Status.ToLower()));

                await _auctionRepository.UpdateAuctionStatus(auction.Id.Value, auction.Status.Value);
                var auctionStatusChangedEvent = new AuctionStatusChangedEvent(
                    auction.Id.Value, auction.Status.Value, auction.UserId.Value
                );
                
                switch (request.AuctionDto.Status.ToLower())
                {
                    case "ended": 
                        await _publishEndpoint.Publish(new AuctionFinished(Guid.Parse(auction.Id.Value)));
                        break;
                    case "deserted":
                        await _publishEndpoint.Publish(new AuctionDeserted(Guid.Parse(auction.Id.Value)));
                        break;
                    case "canceled":
                        await _publishEndpoint.Publish(new AuctionCanceled(Guid.Parse(auction.Id.Value)));
                        break;
                    case "pending":
                        await _publishEndpoint.Publish(new AuctionActived(Guid.Parse(auction.Id.Value)));
                        break;
                    case "completed":
                        await _publishEndpoint.Publish(new PaymentReceived(Guid.Parse(auction.Id.Value)));
                        break;
                    case "delivered":
                        await _publishEndpoint.Publish(new PrizeDelivered(Guid.Parse(auction.Id.Value)));
                        break;
                }
                await _publishEndpoint.Publish(auctionStatusChangedEvent);
                _logger.Info($"Evento publicado: Subasta {auction.Id.Value} creada.");

                return "estado cambiado con exito";
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}