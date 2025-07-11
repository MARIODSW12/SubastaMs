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
    public class StartAuctionCommandHandler : IRequestHandler<StartAuctionCommand, string>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(StartAuctionCommandHandler));

        public StartAuctionCommandHandler(IAuctionRepository auctionRepository, IPublishEndpoint publishEndpoint)
        {
            _auctionRepository = auctionRepository ?? throw new ArgumentNullException(nameof(auctionRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(_publishEndpoint));
        }

        public async Task<string> Handle(StartAuctionCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var auction = await _auctionRepository.GetAuctionById(request.AuctionId);

                auction.ChangeStatus(new VOStatus("active"));

                await _auctionRepository.UpdateAuctionStatus(auction.Id.Value, auction.Status.Value);
                var auctionStartedEvent = new AuctionStatusChangedEvent(
                    auction.Id.Value, auction.Status.Value, auction.UserId.Value
                );
                await _publishEndpoint.Publish(new AuctionStarted(Guid.Parse(auction.Id.Value)));
                await _publishEndpoint.Publish(auctionStartedEvent);
                _logger.Info($"Evento publicado: Subasta {auction.Id.Value} creada.");

                return auction.Id.Value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}