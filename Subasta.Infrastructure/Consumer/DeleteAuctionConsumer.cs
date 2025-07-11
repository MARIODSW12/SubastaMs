using MassTransit;
using MongoDB.Bson;
using log4net;

using Subasta.Domain.Events;

using Subasta.Infrastructure.Interfaces;
using Subasta.Domain.Aggregates;

namespace Subasta.Infrastructure.Consumer
{
    public class DeleteAuctionConsumer(IServiceProvider serviceProvider, IReadAuctionRepository auctionReadRepository) : IConsumer<AuctionDeletedEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IReadAuctionRepository _auctionReadRepository = auctionReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuctionStatusChangedConsumer));

        public async Task Consume(ConsumeContext<AuctionDeletedEvent> @event)
        {

            try
            {
                var message = @event.Message;

                await _auctionReadRepository.DeleteAuction(message.AuctionId, message.UserId);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar AuctionStartdEvent para {@event.Message.AuctionId}", ex);
                throw;
            }
        }
    }
}