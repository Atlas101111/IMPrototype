using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImPrototype.Hubs
{
    public class ImMongoClient
    {
        private static MongoClient _mongoClient;

        public ImMongoClient(string connectionString)
        {
             if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ArgumentNullException("XiaoiceMongoClient connection string");
                }

            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
            clientSettings.ConnectTimeout = TimeSpan.FromSeconds(5);
            clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            clientSettings.AllowInsecureTls = true;
            _mongoClient = new MongoClient(clientSettings);
        }

        public IMongoClient GetClient()
        {
            return _mongoClient;
        }
    }
}
