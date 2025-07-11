using MongoDB.Driver;
using log4net;
using Subasta.Domain.Exceptions;

namespace Subasta.Infrastructure.Configurations
{
    public class MongoReadDbConfig
    {
        public MongoClient client;
        public IMongoDatabase db;

        public MongoReadDbConfig()
        {
            try
            {
                string connectionUri = Environment.GetEnvironmentVariable("MONGODB_CNN_READ");

                if (string.IsNullOrWhiteSpace(connectionUri))
                {
                    throw new ArgumentException("La cadena de conexión de MongoDB no está definida.");
                }

                var settings = MongoClientSettings.FromConnectionString(connectionUri);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);

                client = new MongoClient(settings);

                string databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME_READ");
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