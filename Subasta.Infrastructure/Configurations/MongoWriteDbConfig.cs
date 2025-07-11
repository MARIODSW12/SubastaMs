using MongoDB.Driver;
using log4net;
using Subasta.Domain.Exceptions;

namespace Subasta.Infrastructure.Configurations
{
    public class MongoWriteDbConfig
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MongoWriteDbConfig));
        public MongoClient client;
        public IMongoDatabase db;

        public MongoWriteDbConfig()
        {
            try
            {
                string connectionUri = Environment.GetEnvironmentVariable("MONGODB_CNN_WRITE");

                if (string.IsNullOrWhiteSpace(connectionUri))
                {
                    throw new ArgumentException("La cadena de conexión de MongoDB no está definida.");
                }

                var settings = MongoClientSettings.FromConnectionString(connectionUri);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);

                client = new MongoClient(settings);

                string databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME_WRITE");
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    throw new ArgumentException("El nombre de la base de datos de MongoDB no está definido.");
                }

                db = client.GetDatabase(databaseName);
            }
            catch (MongoException ex)
            {
                throw new MongoDBConnectionException();
            }
            catch (Exception ex)
            {
                throw new MongoDBUnnexpectedException();
            }
        }
    }
}