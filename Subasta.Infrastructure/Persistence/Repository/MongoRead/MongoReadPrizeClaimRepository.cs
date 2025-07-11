using MongoDB.Driver;
using MongoDB.Bson;
using log4net;
using Subasta.Infrastructure.Configurations;
using Subasta.Infrastructure.Interfaces;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Subasta.Infrastructure.Persistence.Repository.MongoRead
{
    public class MongoReadPrizeClaimRepository : IReadPrizeClaimRepository
    {
        private readonly IMongoCollection<BsonDocument> _prizeClaimCollection;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoReadPrizeClaimRepository));

        public MongoReadPrizeClaimRepository(MongoReadDbConfig mongoConfig)
        {
            _prizeClaimCollection = mongoConfig.db.GetCollection<BsonDocument>("prize_claim_read");
        }

        async public Task CreatePrizeClaim(BsonDocument prizeClaim)
        {
            try
            {
                await _prizeClaimCollection.InsertOneAsync(prizeClaim);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }

        async public Task<BsonDocument?> GetPrizeClaimById(string id)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var prizeClaimResult = await _prizeClaimCollection.Find(filter).FirstOrDefaultAsync();

                return prizeClaimResult;
            }
            catch (Exception ex) 
            {
                throw;
            }
        }

        async public Task<BsonDocument?> GetPrizeClaimByUserAndAuction(string auctionId, string userId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("userId", userId), Builders<BsonDocument>.Filter.Eq("auctionId", auctionId));
                var prizeClaimResult = await _prizeClaimCollection.Find(filter).FirstOrDefaultAsync();

                return prizeClaimResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}