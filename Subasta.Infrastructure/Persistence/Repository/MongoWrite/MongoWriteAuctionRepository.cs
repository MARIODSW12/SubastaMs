using MongoDB.Bson;
using MongoDB.Driver;
using log4net;

using Subasta.Domain.Aggregates;
using Subasta.Domain.Repositories;
using Subasta.Domain.Factories;
using Subasta.Domain.ValueObjects;

using Subasta.Infrastructure.Configurations;
using MongoDB.Bson.Serialization.Attributes;

namespace Subasta.Infrastructure.Persistence.Repository.MongoWrite
{
    public class MongoWriteAuctionRepository : IAuctionRepository
    {
        private readonly IMongoCollection<BsonDocument> _auctionCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoWriteAuctionRepository));

        public MongoWriteAuctionRepository(MongoWriteDbConfig mongoConfig)
        {
            _auctionCollection = mongoConfig.db.GetCollection<BsonDocument>("auction_write");

        }

        async public Task CreateAuction(Auction auction)
        {
            try
            {
                var bsonAuction = new BsonDocument
                {
                    { "_id",  auction.Id.Value},
                    {"userId", auction.UserId.Value},
                    {"name", auction.Name.Value},
                    {"description", auction.Description.Value},
                    {"basePrice", auction.BasePrice.Value},
                    {"duration", auction.Duration.Value},
                    {"minimumIncrease", auction.MinimumIncrease.Value},
                    {"reservePrice", auction.ReservePrice.Value},
                    {"startDate", auction.StartDate.Value},
                    {"productId", auction.ProductId.Value},
                    {"productQuantity", auction.ProductQuantity.Value},
                    { "status", auction.Status.Value}
                };

                await _auctionCollection.InsertOneAsync(bsonAuction);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }

        async public Task UpdateAuction(Auction auction)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", auction.Id.Value);
                var update = Builders<BsonDocument>.Update
                    .Set("userId", auction.UserId.Value)
                    .Set("name", auction.Name.Value)
                    .Set("description", auction.Description.Value)
                    .Set("basePrice", auction.BasePrice.Value)
                    .Set("duration", auction.Duration.Value)
                    .Set("minimumIncrease", auction.MinimumIncrease.Value)
                    .Set("reservePrice", auction.ReservePrice.Value)
                    .Set("startDate", auction.StartDate.Value)
                    .Set("productId", auction.ProductId.Value)
                    .Set("productQuantity", auction.ProductQuantity.Value);

                var result = await _auctionCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async public Task<Auction?> GetAuctionById(string id)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var auctionResult = await _auctionCollection.Find(filter).FirstOrDefaultAsync();

                if (auctionResult == null)
                    return null;

                var auction = AuctionFactory.Create(new VOId(auctionResult["_id"].AsString), new VOUserId(auctionResult["userId"].AsString),
                    new VOName(auctionResult["name"].AsString), new VODescription(auctionResult["description"].AsString),
                    new VOBasePrice(auctionResult["basePrice"].AsDecimal), new VODuration(auctionResult["duration"].AsInt32), 
                    new VOMinimumIncrease(auctionResult["minimumIncrease"].AsDecimal), new VOReservePrice(auctionResult["reservePrice"].AsDecimal),
                    new VOStartDate(auctionResult["startDate"].AsBsonDateTime.ToUniversalTime()), new VOStatus(auctionResult["status"].AsString),
                    new VOProductId(auctionResult["productId"].AsString), new VOProductQuantity(auctionResult["productQuantity"].AsInt32));

                return auction;
            }
            catch (Exception ex) 
            { 
                return null;
            }
        }

        async public Task UpdateAuctionStatus(string auctionId, string newStatus)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", auctionId);
                var update = Builders<BsonDocument>.Update
                    .Set("status", newStatus);

                var result = await _auctionCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    _logger.Warn($"No se modificó ningún documento");
                    return;
                }

                _logger.Info($"Subasta actualizada exitosamente. Documentos modificados: {result.ModifiedCount}");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async public Task<long> DeleteAuction(string auctionId, string userId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("_id", auctionId), Builders<BsonDocument>.Filter.Eq("userId", userId));


                var result = await _auctionCollection.DeleteOneAsync(filter);

                return result.DeletedCount;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}