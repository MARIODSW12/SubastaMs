using MassTransit;
using MongoDB.Bson;
using log4net;

using Subasta.Domain.Events;

using Subasta.Infrastructure.Interfaces;
using Subasta.Domain.Aggregates;

namespace Subasta.Infrastructure.Consumer
{
    public class UpdateAuctionConsumer(IServiceProvider serviceProvider, IReadAuctionRepository auctionReadRepository) : IConsumer<AuctionUpdatedEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IReadAuctionRepository _auctionReadRepository = auctionReadRepository;

        public async Task Consume(ConsumeContext<AuctionUpdatedEvent> @event)
        {
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
                    { "productId", message.ProductId},
                    { "productQuantity", message.ProductQuantity},
                };

                await _auctionReadRepository.UpdateAuction(bsonAuction);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}