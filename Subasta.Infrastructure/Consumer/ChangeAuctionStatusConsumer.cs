using MassTransit;
using MongoDB.Bson;
using log4net;

using Subasta.Domain.Events;

using Subasta.Infrastructure.Interfaces;
using Subasta.Domain.Aggregates;

namespace Subasta.Infrastructure.Consumer
{
    public class AuctionStatusChangedConsumer(IServiceProvider serviceProvider, IReadAuctionRepository auctionReadRepository) : IConsumer<AuctionStatusChangedEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IReadAuctionRepository _auctionReadRepository = auctionReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuctionStatusChangedConsumer));

        public async Task Consume(ConsumeContext<AuctionStatusChangedEvent> @event)
        {

            try
            {
                var message = @event.Message;

                await _auctionReadRepository.UpdateAuctionStatus(message.AuctionId, message.Status);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar AuctionStartdEvent para {@event.Message.AuctionId}", ex);
                throw;
            }
        }
    }
}