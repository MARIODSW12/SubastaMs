using MongoDB.Bson;
using MongoDB.Driver;
using log4net;

using Subasta.Domain.Entities;
using Subasta.Domain.Repositories;
using Subasta.Domain.Factories;
using Subasta.Domain.ValueObjects;

using Subasta.Infrastructure.Configurations;
using MongoDB.Bson.Serialization.Attributes;

namespace Subasta.Infrastructure.Persistence.Repository.MongoWrite
{
    public class MongoWritePrizeClaimRepository : IPrizeClaimRepository
    {
        private readonly IMongoCollection<BsonDocument> _prizeClaimCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoWritePrizeClaimRepository));

        public MongoWritePrizeClaimRepository(MongoWriteDbConfig mongoConfig)
        {
            _prizeClaimCollection = mongoConfig.db.GetCollection<BsonDocument>("prize_claim_write");

        }

        async public Task CreatePrizeClaim(PrizeClaimInfo prizeClaim)
        {
            try
            {
                var bsonPrizeClaim = new BsonDocument
                {
                    { "_id",  prizeClaim.PrizeId.Value},
                    {"userId", prizeClaim.PrizeUserId.Value},
                    {"auctionId", prizeClaim.AuctionId.Value},
                    {"deliverDirection", prizeClaim.DeliverDirection.Value},
                    {"deliverMethod", prizeClaim.DeliverMethod.Value},
                    {"claimDate", prizeClaim.ClaimedDate.Value}
                };

                await _prizeClaimCollection.InsertOneAsync(bsonPrizeClaim);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }
        async public Task<PrizeClaimInfo?> GetPrizeClaimById(string id)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var prizeClaimResult = await _prizeClaimCollection.Find(filter).FirstOrDefaultAsync();

                if (prizeClaimResult == null)
                    return null;

                var prizeClaim = PrizeClaimInfoFactory.Create(new VOPrizeId(prizeClaimResult["_id"].AsString),
                    new VOPrizeUserId(prizeClaimResult["userId"].AsString),
                    new VOId(prizeClaimResult["auctionId"].AsString), 
                    new VOClaimDate(prizeClaimResult["claimDate"].AsBsonDateTime.ToUniversalTime()),
                    new VODeliverDirection(prizeClaimResult["deliverDirection"].AsString),
                    new VODeliverMethod(prizeClaimResult["deliverMethod"].AsString));

                return prizeClaim;
            }
            catch (Exception ex) 
            { 
                return null;
            }
        }

    }
}