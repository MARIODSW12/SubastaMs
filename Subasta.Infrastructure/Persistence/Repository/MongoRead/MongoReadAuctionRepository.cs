using MongoDB.Driver;
using MongoDB.Bson;
using log4net;

using Subasta.Domain.Repositories;

using Subasta.Infrastructure.Configurations;
using Subasta.Infrastructure.Interfaces;
using Subasta.Domain.ValueObjects;
using Subasta.Domain.Aggregates;

namespace Subasta.Infrastructure.Persistence.Repository.MongoRead
{
    public class MongoReadAuctionRepository : IReadAuctionRepository
    {
        private readonly IMongoCollection<BsonDocument> _auctionCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoReadAuctionRepository));

        public MongoReadAuctionRepository(MongoReadDbConfig mongoConfig)
        {
            _auctionCollection = mongoConfig.db.GetCollection<BsonDocument>("auction_read");
        }

        async public Task CreateAuction(BsonDocument auction)
        {
            try
            {
                await _auctionCollection.InsertOneAsync(auction);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }

        async public Task UpdateAuction(BsonDocument auction)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", auction["_id"].AsString);
                var update = Builders<BsonDocument>.Update
                    .Set("userId", auction["userId"].AsString)
                    .Set("name", auction["name"].AsString)
                    .Set("description", auction["description"].AsString)
                    .Set("basePrice", auction["basePrice"].AsDecimal)
                    .Set("duration", auction["duration"].AsInt32)
                    .Set("minimumIncrease", auction["minimumIncrease"].AsDecimal)
                    .Set("reservePrice", auction["reservePrice"].AsDecimal)
                    .Set("startDate", auction["startDate"].AsBsonDateTime.ToUniversalTime())
                    .Set("productId", auction["productId"].AsString)
                    .Set("productQuantity", auction["productQuantity"].AsInt32);

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

        async public Task<BsonDocument?> GetAuctionById(string id)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var auctionResult = await _auctionCollection.Find(filter).FirstOrDefaultAsync();

                return auctionResult;
            }
            catch (Exception ex) 
            {
                throw;
            }
        }

        async public Task<List<BsonDocument>> GetUserAuctions(string userId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
                var auctionResult = await _auctionCollection.Find(filter).ToListAsync();

                return auctionResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async public Task<List<BsonDocument>> GetProductAuctions(string productId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("productId", productId);
                var auctionResult = await _auctionCollection.Find(filter).ToListAsync();

                return auctionResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async public Task<List<BsonDocument>> GetAuctionsByStatus(string status)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("status", status);
                var auctionResult = await _auctionCollection.Find(filter).ToListAsync();

                return auctionResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async public Task<List<BsonDocument>> GetAuctionsByStatuses(List<string> statuses)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.In("status", statuses);
                var auctionResult = await _auctionCollection.Find(filter).ToListAsync();

                return auctionResult;
            }
            catch (Exception ex)
            {
                throw;
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

        async public Task DeleteAuction(string auctionId, string userId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("_id", auctionId), Builders<BsonDocument>.Filter.Eq("userId", userId));
                

                var result = await _auctionCollection.DeleteOneAsync(filter);

                return;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async public Task<List<BsonDocument>> GetAuctionsInRange(DateTime from, DateTime to)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Gte("startDate", from.ToUniversalTime()),
                    Builders<BsonDocument>.Filter.Lte("startDate", to.ToUniversalTime())
                );

                var auctionResult = await _auctionCollection.Find(filter).ToListAsync();

                return auctionResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}