using MassTransit;
using MongoDB.Bson;
using log4net;

using Subasta.Domain.Events;

using Subasta.Infrastructure.Interfaces;
using Subasta.Domain.Aggregates;

namespace Subasta.Infrastructure.Consumer
{
    public class CreateAuctionConsumer(IServiceProvider serviceProvider, IReadAuctionRepository auctionReadRepository) : IConsumer<AuctionCreatedEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IReadAuctionRepository _auctionReadRepository = auctionReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CreateAuctionConsumer));

        public async Task Consume(ConsumeContext<AuctionCreatedEvent> @event)
        {
            _logger.Info($"Procesando AuctionCreatedEvent para usuario ID {@event.Message.UserId}");

            try
            {
                var message = @event.Message;
                var bsonAuction = new BsonDocument
                {
                    { "_id",  message.AuctionId},
                    {"userId", message.UserId},
                    {"name", message.Name},
                    {"description", message.Description},
                    {"basePrice", message.BasePrice},
                    {"duration", message.Duration},
                    {"minimumIncrease", message.MinimumIncrease},
                    {"reservePrice", message.ReservePrice},
                    {"startDate", message.StartDate},
                    { "status", message.Status},
                    { "productId", message.ProductId},
                    { "productQuantity", message.ProductQuantity},
                };

                await _auctionReadRepository.CreateAuction(bsonAuction);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al procesar AuctionCreatedEvent para {@event.Message.AuctionId}", ex);
                throw;
            }
        }
    }
}