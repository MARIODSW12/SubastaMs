using MassTransit;
using MongoDB.Bson;
using log4net;

using Subasta.Domain.Events;

using Subasta.Infrastructure.Interfaces;

namespace Subasta.Infrastructure.Consumer
{
    public class CreatePrizeClaimConsumer(IServiceProvider serviceProvider, IReadPrizeClaimRepository prizeClaimReadRepository) : IConsumer<PrizeClaimCreatedEvent>
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IReadPrizeClaimRepository _prizeClaimReadRepository = prizeClaimReadRepository;

        public async Task Consume(ConsumeContext<PrizeClaimCreatedEvent> @event)
        {
            try
            {
                var message = @event.Message;
                var bsonPrizeClaim = new BsonDocument
                {
                    { "_id",  message.PrizeId},
                    {"userId", message.UserId},
                    {"auctionId", message.AuctionId},
                    {"deliverDirection", message.DeliverDirection},
                    {"deliverMethod", message.DeliverMethod},
                    {"claimDate", message.ClaimDate}
                };

                await _prizeClaimReadRepository.CreatePrizeClaim(bsonPrizeClaim);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}