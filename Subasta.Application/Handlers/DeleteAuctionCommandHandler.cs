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
    public class DeleteAuctionCommandHandler : IRequestHandler<DeleteAuctionCommand, string>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ChangeAuctionStatusCommandHandler));

        public DeleteAuctionCommandHandler(IAuctionRepository auctionRepository, IPublishEndpoint publishEndpoint)
        {
            _auctionRepository = auctionRepository ?? throw new ArgumentNullException(nameof(auctionRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(_publishEndpoint));
        }

        public async Task<string> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var deletedCount = await _auctionRepository.DeleteAuction(request.AuctionDto.AuctionId, request.AuctionDto.UserId);

                if (deletedCount == 0) {
                    return "No hay subasta con ese id del usuario";
                }

                var auctionDeletedEvent = new AuctionDeletedEvent(
                    request.AuctionDto.AuctionId, request.AuctionDto.UserId
                );
                
                await _publishEndpoint.Publish(auctionDeletedEvent);

                return "Subasta eliminada con exito";
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}